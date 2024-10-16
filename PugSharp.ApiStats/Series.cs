﻿using System.Globalization;

namespace PugSharp.ApiStats
{
    public class Series
    {
        public required IDictionary<string, Map> Maps { get; init; }

        public required TeamInfo Team1 { get; init; }

        public required TeamInfo Team2 { get; init; }

        public string SeriesType => string.Create(CultureInfo.CurrentCulture, $"bo{Maps.Count}");

        public required Config.Team Winner { get; init; }

        public required bool Forfeit { get; init; }

        public Map GetMap(int mapNumber)
        {
            var mapNumberString = $"map{mapNumber}";

            return Maps[mapNumberString];
        }
    }
}
