﻿using Microsoft.Extensions.Logging;
using PugSharp.ApiStats;
using PugSharp.Logging;
using PugSharp.Match.Contract;
using Stateless;
using Stateless.Graph;

namespace PugSharp.Match;

public class Match : IDisposable
{
    private static readonly ILogger<Match> _Logger = LogManager.CreateLogger<Match>();

    private readonly System.Timers.Timer _VoteTimer = new();
    private readonly System.Timers.Timer _ReadyReminderTimer = new(10000);
    private readonly IMatchCallback _MatchCallback;
    private readonly StateMachine<MatchState, MatchCommand> _MatchStateMachine;
    private readonly ApiStats.ApiStats? _ApiStats;
    private readonly List<Vote> _MapsToSelect;
    private readonly List<Vote> _TeamVotes = new() { new("T"), new("CT") };

    private readonly MatchInfo _MatchInfo;

    private MatchTeam? _CurrentMatchTeamToVote;
    private bool disposedValue;

    public MatchState CurrentState => _MatchStateMachine.State;

    public Config.MatchConfig Config { get; }

    public MatchTeam MatchTeam1 { get; }

    public MatchTeam MatchTeam2 { get; }

    public IEnumerable<MatchPlayer> AllMatchPlayers => MatchTeam1.Players.Concat(MatchTeam2.Players);

    public Match(IMatchCallback matchCallback, Config.MatchConfig matchConfig)
    {
        _MatchCallback = matchCallback;
        Config = matchConfig;
        _MatchInfo = new MatchInfo(matchConfig.NumMaps);
        MatchTeam1 = new MatchTeam(Config.Team1);
        MatchTeam2 = new MatchTeam(Config.Team2);

        _VoteTimer.Interval = Config.VoteTimeout;
        _VoteTimer.Elapsed += VoteTimer_Elapsed;
        _ReadyReminderTimer.Elapsed += ReadyReminderTimer_Elapsed;

        if (!string.IsNullOrEmpty(Config.EventulaApistatsUrl) && !string.IsNullOrEmpty(Config.EventulaApistatsToken))
        {
            _ApiStats = new ApiStats.ApiStats(Config.EventulaApistatsUrl, Config.EventulaApistatsToken);
        }

        _MapsToSelect = matchConfig.Maplist.Select(x => new Vote(x)).ToList();

        _MatchStateMachine = new StateMachine<MatchState, MatchCommand>(MatchState.None);

        _MatchStateMachine.Configure(MatchState.None)
            .Permit(MatchCommand.LoadMatch, MatchState.WaitingForPlayersConnectedReady);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersConnectedReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MapVote, AllPlayersAreReady)
            .OnEntry(SetAllPlayersNotReady)
            .OnEntry(StartReadyReminder)
            .OnExit(StopReadyReminder);

        _MatchStateMachine.Configure(MatchState.MapVote)
            .PermitReentryIf(MatchCommand.VoteMap, MapIsNotSelected)
            .PermitIf(MatchCommand.VoteMap, MatchState.TeamVote, MapIsSelected)
            .OnEntry(SendRemainingMapsToVotingTeam)
            .OnExit(RemoveBannedMap);

        _MatchStateMachine.Configure(MatchState.TeamVote)
            .Permit(MatchCommand.VoteTeam, MatchState.SwitchMap)
            .OnEntry(SendTeamVoteToVotingteam)
            .OnExit(SetSelectedTeamSite);

        _MatchStateMachine.Configure(MatchState.SwitchMap)
            .Permit(MatchCommand.SwitchMap, MatchState.WaitingForPlayersReady)
            .OnEntry(SwitchToMatchMap);

        _MatchStateMachine.Configure(MatchState.WaitingForPlayersReady)
            .PermitIf(MatchCommand.PlayerReady, MatchState.MatchStarting, AllPlayersAreReady)
            .OnEntry(SetAllPlayersNotReady)
            .OnEntry(StartReadyReminder)
            .OnExit(StopReadyReminder);

        _MatchStateMachine.Configure(MatchState.MatchStarting)
            .Permit(MatchCommand.StartMatch, MatchState.MatchRunning)
            .OnEntry(StartMatch);

        _MatchStateMachine.Configure(MatchState.MatchRunning)
            .Permit(MatchCommand.DisconnectPlayer, MatchState.MatchPaused)
            .Permit(MatchCommand.Pause, MatchState.MatchPaused)
            .PermitIf(MatchCommand.CompleteMap, MatchState.MapCompleted, IsMatchReady)
            .OnEntryAsync(MatchLiveAsync);

        _MatchStateMachine.Configure(MatchState.MatchPaused)
            .PermitIf(MatchCommand.ConnectPlayer, MatchState.MatchRunning, AllPlayersAreConnected)
            .PermitIf(MatchCommand.Unpause, MatchState.MatchRunning, AllTeamsUnpaused)
            .OnEntry(PauseMatch)
            .OnExit(UnpauseMatch);

        _MatchStateMachine.Configure(MatchState.MapCompleted)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.MatchCompleted, AllMapsArePlayed)
            .PermitIf(MatchCommand.CompleteMatch, MatchState.WaitingForPlayersConnectedReady, NotAllMapsArePlayed)
            .OnEntry(SendMapResults)
            .OnEntry(TryCompleteMatch);

        _MatchStateMachine.Configure(MatchState.MatchCompleted)
            .OnEntry(CompleteMatch);

        _MatchStateMachine.OnTransitioned(OnMatchStateChanged);

        _MatchStateMachine.Fire(MatchCommand.LoadMatch);
    }

    private void StartReadyReminder()
    {
        _Logger.LogInformation("Start ReadyReminder");
        _ReadyReminderTimer.Start();
    }

    private void StopReadyReminder()
    {
        _Logger.LogInformation("Stop ReadyReminder");
        _ReadyReminderTimer.Stop();
    }

    private bool IsMatchReady()
    {
        // TODO Check if one team has x rounds won or one team has given up?
        return true;
    }

    private void UnpauseMatch()
    {
        _MatchCallback.UnpauseServer();
    }

    private void PauseMatch()
    {
        MatchTeam1.IsPaused = true;
        MatchTeam2.IsPaused = true;

        _MatchCallback.PauseServer();
    }

    private void StartMatch()
    {
        foreach (var player in AllMatchPlayers.Where(x => x.Player.MatchStats != null))
        {
            player.Player.MatchStats!.ResetStats();
        }

        _MatchCallback.EndWarmup();
        _MatchCallback.DisableCheats();
        _MatchCallback.SetupRoundBackup();
        _MatchCallback.StartDemoRecording();

        _MatchCallback.SendMessage($" {ChatColors.Default}Starting Match. {ChatColors.Highlight}{MatchTeam1.TeamConfig.Name} {ChatColors.Default}as {ChatColors.Highlight}{MatchTeam1.StartTeam}{ChatColors.Default}. {ChatColors.Highlight}{MatchTeam2.TeamConfig.Name}{ChatColors.Default} as {ChatColors.Highlight}{MatchTeam2.StartTeam}");

        _ = _ApiStats?.SendGoingLiveAsync(new GoingLiveParams(_MatchInfo.CurrentMap.MapName, _MatchInfo.CurrentMap.MapNumber), CancellationToken.None);

        TryFireState(MatchCommand.StartMatch);
    }

    private void SendMapResults()
    {
        if (_MatchInfo.CurrentMap.Winner == null)
        {
            throw new NotSupportedException("Map Winner is not yet set. Can not send map results");
        }

        var mapResultParams = new MapResultParams(_MatchInfo.CurrentMap.Winner.TeamConfig.Name, _MatchInfo.CurrentMap.Team1Points, _MatchInfo.CurrentMap.Team2Points, _MatchInfo.CurrentMap.MapNumber);
        _ = _ApiStats?.SendMapResultAsync(mapResultParams, CancellationToken.None);
    }

    private void TryCompleteMatch()
    {
        TryFireState(MatchCommand.CompleteMatch);
    }

    private void CompleteMatch()
    {
        _MatchCallback.StopDemoRecording();
        //_ApiStats?.SendSeriesResultAsync(new SeriesResultParams(_Match))
    }

    private async Task MatchLiveAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            _MatchCallback.SendMessage($" {ChatColors.Default}Match is {ChatColors.Green}LIVE");

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }
    }

    private void OnMatchStateChanged(StateMachine<MatchState, MatchCommand>.Transition transition)
    {
        _Logger.LogInformation("MatchState Changed: {source} => {destination}", transition.Source, transition.Destination);
    }

    private void SwitchToMatchMap()
    {
        _MatchCallback.SwitchMap(_MatchInfo.CurrentMap.MapName);
        TryFireState(MatchCommand.SwitchMap);
    }

    private void VoteTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _VoteTimer.Stop();
        switch (CurrentState)
        {
            case MatchState.MapVote:
                TryFireState(MatchCommand.VoteMap);
                break;
            case MatchState.TeamVote:
                TryFireState(MatchCommand.VoteTeam);
                break;
        }
    }

    private void ReadyReminderTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_ReadyReminderTimer.Enabled)
        {
            return;
        }

        try
        {
            _Logger.LogInformation("ReadyReminder Elapsed");
            var readyPlayerIds = AllMatchPlayers.Where(p => p.IsReady).Select(x => x.Player.SteamID).ToList();
            var notReadyPlayers = _MatchCallback.GetAllPlayers().Where(p => !readyPlayerIds.Contains(p.SteamID));

            var remindMessage = $" {ChatColors.Default}You are {ChatColors.Error}not {ChatColors.Default}ready! Type {ChatColors.Command}!ready {ChatColors.Default}if you are ready.";
            foreach (var player in notReadyPlayers)
            {
                player.PrintToChat(remindMessage);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error sending vote reminder");
        }
    }

    public string CreateDotGraph()
    {
        return UmlDotGraph.Format(_MatchStateMachine.GetInfo());
    }

    private void SendRemainingMapsToVotingTeam()
    {
        SwitchVotingTeam();

        _MapsToSelect.ForEach(m => m.Votes.Clear());

        var mapOptions = new List<MenuOption>();

        for (int i = 0; i < _MapsToSelect.Count; i++)
        {
            var mapNumber = i;
            string? map = _MapsToSelect[mapNumber].Name;
            mapOptions.Add(new MenuOption(map, (opt, player) => BanMap(player, mapNumber)));
        }

        ShowMenuToTeam(_CurrentMatchTeamToVote!, $" {ChatColors.Default}Vote to ban map: type {ChatColors.Command}!<mapnumber>", mapOptions);

        _VoteTimer.Start();
    }

    private void RemoveBannedMap()
    {
        _VoteTimer.Stop();

        var mapToBan = _MapsToSelect.MaxBy(m => m.Votes.Count);
        _MapsToSelect.Remove(mapToBan!);
        _MapsToSelect.ForEach(x => x.Votes.Clear());

        _MatchCallback.SendMessage($" {ChatColors.Default}Map {ChatColors.Highlight}{mapToBan!.Name} {ChatColors.Default}was banned by {_CurrentMatchTeamToVote?.TeamConfig.Name}!");

        if (_MapsToSelect.Count == 1)
        {
            _MatchInfo.CurrentMap.MapName = _MapsToSelect[0].Name;
        }
    }

    private void SendTeamVoteToVotingteam()
    {
        SwitchVotingTeam();

        var mapOptions = new List<MenuOption>()
        {
            new("T", (opt, player) => VoteTeam(player, "T")),
            new("CT", (opt, player) => VoteTeam(player, "CT")),
        };

        ShowMenuToTeam(_CurrentMatchTeamToVote!, "Choose starting side:", mapOptions);

        _VoteTimer.Start();
    }

    private void SetSelectedTeamSite()
    {
        _VoteTimer.Stop();

        var startTeam = _TeamVotes.MaxBy(m => m.Votes.Count)!.Name.Equals("T") ? Team.Terrorist : Team.CounterTerrorist;
        _Logger.LogInformation("Set selected teamsite to {startTeam}. Voted by {team}", startTeam, _CurrentMatchTeamToVote!.StartTeam.ToString());

        if (_CurrentMatchTeamToVote!.StartTeam != startTeam)
        {
            _CurrentMatchTeamToVote.StartTeam = startTeam;
            var otherTeam = _CurrentMatchTeamToVote == MatchTeam1 ? MatchTeam2 : MatchTeam1;
            otherTeam.StartTeam = startTeam == Team.Terrorist ? Team.CounterTerrorist : Team.Terrorist;
        }

        _MatchCallback.SendMessage($"{_CurrentMatchTeamToVote!.TeamConfig.Name} selected {startTeam} as startside!");
    }

    private void ShowMenuToTeam(MatchTeam team, string title, IEnumerable<MenuOption> options)
    {
        team.Players.ForEach(p =>
        {
            p.Player.ShowMenu(title, options);
        });
    }

    private void SwitchVotingTeam()
    {
        if (_CurrentMatchTeamToVote == null)
        {
            _CurrentMatchTeamToVote = MatchTeam1;
        }
        else
        {
            _CurrentMatchTeamToVote = _CurrentMatchTeamToVote == MatchTeam1 ? MatchTeam2 : MatchTeam1;
        }
    }

    private bool AllPlayersAreConnected()
    {
        var players = _MatchCallback.GetAllPlayers();
        var connectedPlayerSteamIds = players.Select(p => p.SteamID).ToList();
        var allPlayerIds = Config.Team1.Players.Keys.Concat(Config.Team2.Players.Keys);
        if (allPlayerIds.All(p => connectedPlayerSteamIds.Contains(p)))
        {
            return true;
        }

        return false;
    }

    private bool AllPlayersAreReady()
    {
        var readyPlayers = AllMatchPlayers.Where(p => p.IsReady);
        var rquiredPlayers = Config.PlayersPerTeam * 2;

        _Logger.LogInformation($"Match has {readyPlayers.Count()} of {rquiredPlayers} ready players: {string.Join("; ", readyPlayers.Select(a => $"{a.Player.PlayerName}[{a.IsReady}]"))}");

        return readyPlayers.Count() == rquiredPlayers;
    }

    private bool AllTeamsUnpaused() => !MatchTeam1.IsPaused && !MatchTeam2.IsPaused;

    private bool AllMapsArePlayed()
    {
        var teamWithMostWins = _MatchInfo.MatchMaps.Where(x => x.Winner != null).GroupBy(x => x.Winner).MaxBy(x => x.Count());
        if (teamWithMostWins?.Key == null)
        {
            return false;
        }

        return teamWithMostWins.Count() > Config.NumMaps / 2d;
    }

    private bool NotAllMapsArePlayed() => !AllMapsArePlayed();

    private void SetAllPlayersNotReady()
    {
        _Logger.LogInformation("Reset Readystate for all players");

        foreach (var player in AllMatchPlayers)
        {
            player.IsReady = false;
        }

        _MatchCallback.SendMessage($"Waiting for all players to be ready.");
        _MatchCallback.SendMessage($" {ChatColors.Command}!ready {ChatColors.Default}to toggle your ready state.");
    }

    private bool MapIsSelected()
    {
        // The SelectedCount is checked when the Votes are done but the map is still in the list
        return _MapsToSelect.Count == 2;
    }

    private bool MapIsNotSelected()
    {
        return !MapIsSelected();
    }

    private bool TryFireState(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            _MatchStateMachine.Fire(command);
            return true;
        }

        return false;
    }

    private Task TryFireStateAsync(MatchCommand command)
    {
        if (_MatchStateMachine.CanFire(command))
        {
            return _MatchStateMachine.FireAsync(command);
        }

        return Task.CompletedTask;
    }

    private MatchTeam? GetMatchTeam(ulong steamID)
    {
        if (MatchTeam1.Players.Exists(x => x.Player.SteamID == steamID))
        {
            return MatchTeam1;
        }

        if (MatchTeam1.Players.Exists(x => x.Player.SteamID == steamID))
        {
            return MatchTeam2;
        }

        return null;
    }

    private MatchTeam? GetMatchTeam(Team team)
    {
        return MatchTeam1.StartTeam == team ? MatchTeam1 : MatchTeam2;
    }

    private MatchPlayer GetMatchPlayer(ulong steamID)
    {
        return AllMatchPlayers.First(x => x.Player.SteamID == steamID);
    }

    #region Match Functions

    public bool TryAddPlayer(IPlayer player)
    {
        //var playerTeam = GetConfigTeam(player.SteamID);
        //if (playerTeam == Team.None)
        //{
        //    return false;
        //}

        var isTeam1 = Config.Team1.Players.ContainsKey(player.SteamID);
        var teamName = isTeam1 ? Config.Team1.Name : Config.Team2.Name;
        var startSite = isTeam1 ? Team.Terrorist : Team.CounterTerrorist;

        _Logger.LogInformation("Player {playerName} belongs to {teamName}", player.PlayerName, teamName);

        if (player.Team != startSite)
        {
            player.SwitchTeam(startSite);
        }

        var team = isTeam1 ? MatchTeam1 : MatchTeam2;

        var existingPlayer = team.Players.Find(x => x.Player.SteamID.Equals(player.SteamID));
        if (existingPlayer != null)
        {
            team.Players.Remove(existingPlayer);
        }

        team.Players.Add(new MatchPlayer(player));

        _ = TryFireStateAsync(MatchCommand.ConnectPlayer);
        return true;
    }

    public void SetPlayerDisconnected(IPlayer player)
    {
        var matchTeam = GetMatchTeam(player.SteamID);
        if (matchTeam == null)
        {
            return;
        }

        // TODO Error when Player was not ready
        var matchPlayer = GetMatchPlayer(player.SteamID);
        TryFireState(MatchCommand.DisconnectPlayer);

        switch (CurrentState)
        {
            case MatchState.WaitingForPlayersConnectedReady:
                matchPlayer.IsReady = false;
                break;
            case MatchState.MapVote:
            case MatchState.TeamVote:
            case MatchState.SwitchMap:
            case MatchState.WaitingForPlayersReady:
            case MatchState.MatchStarting:
            case MatchState.MatchRunning:
            case MatchState.MatchPaused:
            case MatchState.MatchCompleted:
                break;
            default:
                matchTeam.Players.Remove(matchPlayer);
                break;
        }
    }

    public async Task TogglePlayerIsReadyAsync(IPlayer player)
    {
        if (CurrentState != MatchState.WaitingForPlayersConnectedReady && CurrentState != MatchState.WaitingForPlayersReady)
        {
            player.PrintToChat("Currently ready state is not awaited!");
            return;
        }

        var matchPlayer = GetMatchPlayer(player.SteamID);
        matchPlayer.IsReady = !matchPlayer.IsReady;

        var readyPlayers = MatchTeam1.Players.Count(x => x.IsReady) + MatchTeam2.Players.Count(x => x.IsReady);

        // Min Players per Team
        var requiredPlayers = Config.MinPlayersToReady * 2;

        if (matchPlayer.IsReady)
        {
            _MatchCallback.SendMessage($"{player.PlayerName} is ready! {readyPlayers} of {requiredPlayers} are ready.");
            await TryFireStateAsync(MatchCommand.PlayerReady).ConfigureAwait(false);
        }
        else
        {
            _MatchCallback.SendMessage($"{player.PlayerName} is not ready! {readyPlayers} of {requiredPlayers} are ready.");
        }
    }

    public Team GetPlayerTeam(ulong steamID)
    {
        var matchTeam = GetMatchTeam(steamID);

        if (matchTeam != null)
        {
            return matchTeam.StartTeam;
        }

        return GetConfigTeam(steamID);
    }

    private Team GetConfigTeam(ulong steamID)
    {
        if (Config.Team1.Players.ContainsKey(steamID))
        {
            return Team.Terrorist;
        }

        if (Config.Team2.Players.ContainsKey(steamID))
        {
            return Team.CounterTerrorist;
        }

        return Team.None;
    }

    public bool BanMap(IPlayer player, int mapNumber)
    {
        if (CurrentState != MatchState.MapVote)
        {
            player.PrintToChat("Currently no map vote is active!");
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            player.PrintToChat("There is not current matchteam to vote!");
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to ban a map!");
            return false;
        }


        if (_MapsToSelect.Count <= mapNumber || mapNumber < 0)
        {
            player.PrintToChat($"Mapnumber {mapNumber} is not available!");
            return false;
        }

        var bannedMap = _MapsToSelect.Find(x => x.Votes.Exists(x => x.UserId == player.UserId));
        if (bannedMap != null)
        {
            player.PrintToChat($"You already banned mapnumber {_MapsToSelect.IndexOf(bannedMap)}: {bannedMap.Name} !");
            return false;
        }

        var mapToSelect = _MapsToSelect[mapNumber];
        mapToSelect.Votes.Add(player);

        if (_MapsToSelect.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteMap);
        }

        return true;
    }

    public bool VoteTeam(IPlayer player, string teamName)
    {
        if (CurrentState != MatchState.TeamVote)
        {
            player.PrintToChat("Currently no team vote is active!");
            return false;
        }

        if (_CurrentMatchTeamToVote == null)
        {
            player.PrintToChat("There is not current matchteam to vote!");
            return false;
        }

        if (!_CurrentMatchTeamToVote.Players.Select(x => x.Player.UserId).Contains(player.UserId))
        {
            player.PrintToChat("You are currently not permitted to vote for a team!");
            return false;
        }

        var votedTeam = _TeamVotes.Find(x => x.Votes.Exists(x => x.UserId == player.UserId));
        if (votedTeam != null)
        {
            player.PrintToChat($"You already voted for team {_MapsToSelect.IndexOf(votedTeam)}: {votedTeam.Name} !");
            return false;
        }

        var teamToVote = _TeamVotes.Find(x => x.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
        if (teamToVote == null)
        {
            player.PrintToChat($"Team with name {teamName} is not available!");
            return false;
        }

        teamToVote.Votes.Add(player);

        player.PrintToChat($" {ChatColors.Default}You voted for {ChatColors.Highlight}{teamToVote.Name}");

        if (_TeamVotes.Sum(x => x.Votes.Count) >= Config.PlayersPerTeam)
        {
            TryFireState(MatchCommand.VoteTeam);
        }

        return true;
    }

    public void Pause(IPlayer player)
    {
        if (!TryFireState(MatchCommand.Pause))
        {
            player.PrintToChat("Pause is currently not possible!");
        }
    }

    public void Unpause(IPlayer player)
    {
        var team = GetMatchTeam(player.SteamID);
        if (team == null)
        {
            player.PrintToChat("Unpause is currently not possible!");
            return;
        }

        team.IsPaused = false;
        TryFireState(MatchCommand.Unpause);
    }

    //public void RoundCompleted()
    //{
    //    var teamInfo1 = new TeamInfo
    //    {
    //        //Id = Config.Team1.Name,
    //        TeamName = Config.Team1.Name
    //    };

    //    var roundStats = new RoundStatusUpdateParams(_MatchInfo.CurrentMap.MapNumber, teamInfo1, teamInfo2, new Map { });
    //    _ApiStats?.SendRoundStatsUpdateAsync(roundStats, CancellationToken.None);
    //}

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _VoteTimer.Elapsed -= VoteTimer_Elapsed;
                _VoteTimer.Dispose();

                _ReadyReminderTimer.Elapsed -= ReadyReminderTimer_Elapsed;
                _ReadyReminderTimer.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void CompleteMap(int tPoints, int ctPoints)
    {
        var winner = tPoints > ctPoints ? Team.Terrorist : Team.CounterTerrorist;

        var winnerTeam = GetMatchTeam(winner);
        if (winnerTeam == null)
        {
            throw new NotSupportedException("Winner Team could not be found!");
        }

        _MatchInfo.CurrentMap.Winner = winnerTeam;

        var configWinnerTeam = GetConfigTeam(winnerTeam.Players.First().Player.SteamID);
        if (configWinnerTeam == Team.Terrorist)
        {
            // Team 1 won
            _MatchInfo.CurrentMap.Team1Points = winner == Team.Terrorist ? tPoints : ctPoints;
            _MatchInfo.CurrentMap.Team2Points = winner == Team.Terrorist ? ctPoints : tPoints;
        }
        else
        {
            // Team 2 won
            _MatchInfo.CurrentMap.Team1Points = winner == Team.Terrorist ? ctPoints : tPoints;
            _MatchInfo.CurrentMap.Team2Points = winner == Team.Terrorist ? tPoints : ctPoints;
        }

        _Logger.LogInformation("The winner is: {winner}", winnerTeam!.TeamConfig.Name);
        TryFireState(MatchCommand.CompleteMap);
    }

    #endregion
}
