﻿using PugSharp.Api.Contract;

namespace PugSharp.ApiStats
{
    public class TeamInfo : ITeamInfo
    {
        public required string TeamName { get; init; }
    }
}
