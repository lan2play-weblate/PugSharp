﻿using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using PugSharp.Api.Contract;
using PugSharp.Logging;
using System.Globalization;
namespace PugSharp.ApiStats
{
    public class ApiStats : BaseApi, IApiProvider
    {
        private static readonly ILogger<ApiStats> _Logger = LogManager.CreateLogger<ApiStats>();

        public ApiStats(string? apiStatsUrl, string? apiStatsKey) : base(apiStatsUrl, apiStatsKey)
        {
            _Logger.LogInformation("Create Api Stats with BaseUrl: {url}", apiStatsUrl);
        }

        public async Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            try
            {
                var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {ApiStatsConstants.StatsMapName, goingLiveParams.MapName},
                };

                var uri = QueryHelpers.AddQueryString(string.Create(CultureInfo.InvariantCulture, $"golive/{goingLiveParams.MapNumber}"), queryParams);

                var response = await HttpClient.PostAsync(uri, content: null, cancellationToken).ConfigureAwait(false);

                await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error sending going live!");
            }
        }

        public async Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            try
            {
                var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"team1score", CreateIntParam(finalizeMapParams.Team1Score)},
                    {"team2score", CreateIntParam(finalizeMapParams.Team2Score)},
                    {ApiStatsConstants.StatsMapWinner, finalizeMapParams.WinnerTeamName},
                };

                var uri = QueryHelpers.AddQueryString(string.Create(CultureInfo.InvariantCulture, $"finalize/{finalizeMapParams.MapNumber}"), queryParams);

                var response = await HttpClient.PostAsync(uri, content: null, cancellationToken).ConfigureAwait(false);

                await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error sending map result!");
            }
        }


        public async Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            try
            {
                var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"team1score", CreateIntParam(roundStatusUpdateParams.CurrentMap.Team1.Score)},
                    {"team2score", CreateIntParam(roundStatusUpdateParams.CurrentMap.Team2.Score)},
                };

                var uri = QueryHelpers.AddQueryString(string.Create(CultureInfo.InvariantCulture, $"updateround/{roundStatusUpdateParams.MapNumber}"), queryParams);

                var response = await HttpClient.PostAsync(uri, content: null, cancellationToken).ConfigureAwait(false);

                await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);

                await UpdatePlayerStatsInternalAsync(roundStatusUpdateParams.MapNumber, roundStatusUpdateParams.TeamInfo1, roundStatusUpdateParams.TeamInfo2, roundStatusUpdateParams.CurrentMap, cancellationToken).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error sending round results!");
            }
        }

        private async Task UpdatePlayerStatsInternalAsync(int mapNumber, ITeamInfo teamInfo1, ITeamInfo teamInfo2, IMap currentMap, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            var dict = new Dictionary<string, IMapTeamInfo>(StringComparer.OrdinalIgnoreCase)
            {
                { teamInfo1.TeamName, currentMap.Team1},
                { teamInfo2.TeamName, currentMap.Team2 },
            };

            foreach (var team in dict)
            {
                var teamName = team.Key;

                var players = team.Value.Players;

                foreach (var player in players)
                {
                    var playerStatistics = player.Value;

                    Dictionary<string, string> queryParams = CreateUpdatePlayerQueryParameters(teamName, playerStatistics);

                    var uri = QueryHelpers.AddQueryString(string.Create(CultureInfo.InvariantCulture, $"updateplayer/{mapNumber}/{player.Key}"), queryParams);

                    var response = await HttpClient.PostAsync(uri, content: null, cancellationToken).ConfigureAwait(false);

                    await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static Dictionary<string, string> CreateUpdatePlayerQueryParameters(string teamName, IPlayerStatistics playerStatistics)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {ApiStatsConstants.StatsTeamName, teamName},
                        {ApiStatsConstants.StatsName, playerStatistics.Name},
                        {ApiStatsConstants.StatsKills, CreateIntParam(playerStatistics.Kills)},
                        {ApiStatsConstants.StatsDeaths, CreateIntParam(playerStatistics.Deaths)},
                        {ApiStatsConstants.StatsAssists, CreateIntParam(playerStatistics.Assists)},
                        {ApiStatsConstants.StatsFlashbangAssists, CreateIntParam(playerStatistics.FlashbangAssists)},
                        {ApiStatsConstants.StatsTeamKills, CreateIntParam(playerStatistics.TeamKills)},
                        {ApiStatsConstants.StatsSuicides, CreateIntParam(playerStatistics.Suicides)},
                        {ApiStatsConstants.StatsDamage, CreateIntParam(playerStatistics.Damage)},
                        {ApiStatsConstants.StatsUtilityDamage, CreateIntParam(playerStatistics.UtilityDamage)},
                        {ApiStatsConstants.StatsEnemiesFlashed, CreateIntParam(playerStatistics.EnemiesFlashed)},
                        {ApiStatsConstants.StatsFriendliesFlashed, CreateIntParam(playerStatistics.FriendliesFlashed)},
                        {ApiStatsConstants.StatsKnifeKills, CreateIntParam(playerStatistics.KnifeKills)},
                        {ApiStatsConstants.StatsHeadshotKills, CreateIntParam(playerStatistics.HeadshotKills)},
                        {ApiStatsConstants.StatsRoundsPlayed, CreateIntParam(playerStatistics.RoundsPlayed)},
                        {ApiStatsConstants.StatsBombPlants, CreateIntParam(playerStatistics.BombPlants)},
                        {ApiStatsConstants.StatsBombDefuses, CreateIntParam(playerStatistics.BombDefuses)},
                        {ApiStatsConstants.Stats1K, CreateIntParam(playerStatistics.Count1K)},
                        {ApiStatsConstants.Stats2K, CreateIntParam(playerStatistics.Count2K)},
                        {ApiStatsConstants.Stats3K, CreateIntParam(playerStatistics.Count3K)},
                        {ApiStatsConstants.Stats4K, CreateIntParam(playerStatistics.Count4K)},
                        {ApiStatsConstants.Stats5K, CreateIntParam(playerStatistics.Count5K)},
                        {ApiStatsConstants.StatsV1, CreateIntParam(playerStatistics.V1)},
                        {ApiStatsConstants.StatsV2, CreateIntParam(playerStatistics.V2)},
                        {ApiStatsConstants.StatsV3, CreateIntParam(playerStatistics.V3)},
                        {ApiStatsConstants.StatsV4, CreateIntParam(playerStatistics.V4)},
                        {ApiStatsConstants.StatsV5, CreateIntParam(playerStatistics.V5)},
                        {ApiStatsConstants.StatsFirstKillT, CreateIntParam(playerStatistics.FirstKillT)},
                        {ApiStatsConstants.StatsFirstKillCt, CreateIntParam(playerStatistics.FirstKillCt)},
                        {ApiStatsConstants.StatsFirstDeathT, CreateIntParam(playerStatistics.FirstDeathT)},
                        {ApiStatsConstants.StatsFirstDeathCt, CreateIntParam(playerStatistics.FirstDeathCt)},
                        {ApiStatsConstants.StatsTradeKill, CreateIntParam(playerStatistics.TradeKill)},
                        {ApiStatsConstants.StatsKast, CreateIntParam(playerStatistics.Kast)},
                        {ApiStatsConstants.StatsContributionScore, CreateIntParam(playerStatistics.ContributionScore)},
                        {ApiStatsConstants.StatsMvp, CreateIntParam(playerStatistics.Mvp)},
                    };
        }

        internal static string CreateIntParam(int param)
        {
            return param.ToString(CultureInfo.InvariantCulture);
        }

        public async Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            if (HttpClient == null)
            {
                return;
            }

            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {ApiStatsConstants.StatsSeriesWinner, seriesResultParams.WinnerTeamName},
                {ApiStatsConstants.StatsSeriesForfeit, CreateIntParam(Convert.ToInt32(seriesResultParams.Forfeit))},
            };

            var uri = QueryHelpers.AddQueryString($"finalize", queryParams);

            var response = await HttpClient.PostAsync(uri, content: null, cancellationToken).ConfigureAwait(false);

            await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);

            // Wait before freeing server
            await Task.Delay(TimeSpan.FromMilliseconds(seriesResultParams.TimeBeforeFreeingServerMs), cancellationToken).ConfigureAwait(false);
        }

        public async Task FreeServerAsync(CancellationToken cancellationToken)
        {
            if (HttpClient == null)
            {
                return;
            }

            var response = await HttpClient.PostAsync("freeserver", content: null, cancellationToken).ConfigureAwait(false);

            await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        private static class ApiStatsConstants
        {
#pragma warning disable S125 // Sections of code should not be commented out
            // Series stats(root section)
            public const string StatsSeriesWinner = "winner";

            //public const string StatsSeriesType = "series_type";
            //public const string StatsSeriesTeamId = "id";
            //public const string StatsSeriesTeamName = "name";
            public const string StatsSeriesForfeit = "forfeit";

            // Map stats (under "map0", "map1", etc.)
            public const string StatsMapName = "mapname";
            public const string StatsMapWinner = "winner";
            //public const string StatsDemoFilename = "demo_filename";

            // Team stats (under map section, then "team1" or "team2")
            //public const string StatsTeamScore = "score";
            //public const string StatsTeamScoreCt = "score_ct";
            //public const string StatsTeamScoreT = "score_t";
            //public const string StatsStartingSide = "starting_side";

            public const string StatsTeamName = "team";

            // Player stats (under map section, then team section, then player's steam64)
            // If adding stuff here, also add to the InitPlayerStats function!
            //public const string StatsInit = "init"; // used to zero-fill stats only. Not a real stat.
            //public const string StatsCoaching = "coaching"; // indicates if the player is a coach.
            public const string StatsName = "name";
            public const string StatsKills = "kills";
            public const string StatsDeaths = "deaths";
            public const string StatsAssists = "assists";
            public const string StatsFlashbangAssists = "flashbang_assists";
            public const string StatsTeamKills = "teamkills";
            public const string StatsSuicides = "suicides";
            public const string StatsDamage = "damage";
            public const string StatsUtilityDamage = "util_damage";
            public const string StatsEnemiesFlashed = "enemies_flashed";
            public const string StatsFriendliesFlashed = "friendlies_flashed";
            public const string StatsKnifeKills = "knife_kills";
            public const string StatsHeadshotKills = "headshot_kills";
            public const string StatsRoundsPlayed = "roundsplayed";
            public const string StatsBombDefuses = "bomb_defuses";
            public const string StatsBombPlants = "bomb_plants";
            public const string Stats1K = "1kill_rounds";
            public const string Stats2K = "2kill_rounds";
            public const string Stats3K = "3kill_rounds";
            public const string Stats4K = "4kill_rounds";
            public const string Stats5K = "5kill_rounds";
            public const string StatsV1 = "v1";
            public const string StatsV2 = "v2";
            public const string StatsV3 = "v3";
            public const string StatsV4 = "v4";
            public const string StatsV5 = "v5";
            public const string StatsFirstKillT = "firstkill_t";
            public const string StatsFirstKillCt = "firstkill_ct";
            public const string StatsFirstDeathT = "firstdeath_t";
            public const string StatsFirstDeathCt = "firstdeath_ct";
            public const string StatsTradeKill = "tradekill";
            public const string StatsKast = "kast";
            public const string StatsContributionScore = "contribution_score";
            public const string StatsMvp = "mvp";
#pragma warning restore S125 // Sections of code should not be commented out
        }
    }
}
