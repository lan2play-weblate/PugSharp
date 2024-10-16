﻿namespace PugSharp.Match.Contract;

public enum MatchCommand
{
    LoadMatch,
    ConnectPlayer,
    DisconnectPlayer,
    PlayerReady,
    VoteMap,
    VoteTeam,
    SwitchMap,
    StartMatch,
    CompleteMatch,
    CompleteMap,
    Pause,
    Unpause,
}
