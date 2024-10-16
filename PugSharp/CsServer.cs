﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using PugSharp.Logging;
using PugSharp.Server.Contract;

namespace PugSharp;

public class CsServer : ICsServer
{
    private static readonly ILogger<CsServer> _Logger = LogManager.CreateLogger<CsServer>();

    public string GameDirectory => CounterStrikeSharp.API.Server.GameDirectory;

    public void ExecuteCommand(string v)
    {
        CounterStrikeSharp.API.Server.ExecuteCommand(v);
    }

    public bool IsMapValid(string selectedMap)
    {
        return CounterStrikeSharp.API.Server.IsMapValid(selectedMap);
    }

    public (int CtScore, int TScore) LoadTeamsScore()
    {
        var teamEntities = Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager");
        int ctScore = 0;
        int tScore = 0;
        foreach (var team in teamEntities)
        {
            if (team.Teamname.Equals("CT", StringComparison.OrdinalIgnoreCase))
            {
                ctScore = team.Score;
            }
            else if (team.Teamname.Equals("TERRORIST", StringComparison.OrdinalIgnoreCase))
            {
                tScore = team.Score;
            }
            else
            {
                _Logger.LogError("TeamName '{name}'is not supported!", team.Teamname);
            }
        }

        return (ctScore, tScore);
    }

    public void NextFrame(Action value)
    {
        CounterStrikeSharp.API.Server.NextFrame(value);
    }

    public void PrintToChatAll(string message)
    {
        CounterStrikeSharp.API.Server.PrintToChatAll(message);
    }
}
