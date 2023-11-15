﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PugSharp.Translation.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PugSharp.Translation.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hello **{0:playerName}**, welcome to match {1:matchId}.
        /// </summary>
        public static string PugSharp_Hello {
            get {
                return ResourceManager.GetString("PugSharp.Hello", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Map **{0:mapName}** was banned by {1:teamName}!.
        /// </summary>
        public static string PugSharp_Match_BannedMap {
            get {
                return ResourceManager.GetString("PugSharp.Match.BannedMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You already banned mapnumber {0: mapNumber}: {1:mapName}!.
        /// </summary>
        public static string PugSharp_Match_Error_AlreadyBannedMap {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.AlreadyBannedMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ou already voted for team {0:teamNumber}: {1:teamName}!.
        /// </summary>
        public static string PugSharp_Match_Error_AlreadyVotedForTeam {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.AlreadyVotedForTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Currently no map vote is active!.
        /// </summary>
        public static string PugSharp_Match_Error_NoMapVoteExpected {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.NoMapVoteExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Currently ready state is not awaited!.
        /// </summary>
        public static string PugSharp_Match_Error_NoReadyExpected {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.NoReadyExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Currently no team vote is active!.
        /// </summary>
        public static string PugSharp_Match_Error_NoTeamVoteExpected {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.NoTeamVoteExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are currently not permitted to ban a map!.
        /// </summary>
        public static string PugSharp_Match_Error_NotPermittedToBanMap {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.NotPermittedToBanMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are currently not permitted to vote for a team!.
        /// </summary>
        public static string PugSharp_Match_Error_NotPermittedToVoteForTeam {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.NotPermittedToVoteForTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pause is currently not possible!.
        /// </summary>
        public static string PugSharp_Match_Error_PauseNotPossible {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.PauseNotPossible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Team with name {0:teamName} is not available!.
        /// </summary>
        public static string PugSharp_Match_Error_TeamNotAvailable {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.TeamNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mapnumber **{0:mapNumber}** is not available!.
        /// </summary>
        public static string PugSharp_Match_Error_UnknownMapNumber {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.UnknownMapNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unpause is currently not possible!.
        /// </summary>
        public static string PugSharp_Match_Error_UnpauseNotPossible {
            get {
                return ResourceManager.GetString("PugSharp.Match.Error.UnpauseNotPossible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Match is **LIVE**.
        /// </summary>
        public static string PugSharp_Match_Info_IsLive {
            get {
                return ResourceManager.GetString("PugSharp.Match.Info.IsLive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:playerName} is not ready! {1:readyPlayers} of {2:requiredPlayers} are ready..
        /// </summary>
        public static string PugSharp_Match_Info_NotReady {
            get {
                return ResourceManager.GetString("PugSharp.Match.Info.NotReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:playerName} is ready! {1:readyPlayers} of {2:requiredPlayers} are ready..
        /// </summary>
        public static string PugSharp_Match_Info_Ready {
            get {
                return ResourceManager.GetString("PugSharp.Match.Info.Ready", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting Match. **{0:team1Name}** as **{1:team1Side}**. **{2:team2Name}** as **{team2Side}**.
        /// </summary>
        public static string PugSharp_Match_Info_StartMatch {
            get {
                return ResourceManager.GetString("PugSharp.Match.Info.StartMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Waiting for all players to be ready..
        /// </summary>
        public static string PugSharp_Match_Info_WaitingForAllPlayers {
            get {
                return ResourceManager.GetString("PugSharp.Match.Info.WaitingForAllPlayers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not Ready.
        /// </summary>
        public static string PugSharp_Match_NotReadyTag {
            get {
                return ResourceManager.GetString("PugSharp.Match.NotReadyTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ready.
        /// </summary>
        public static string PugSharp_Match_ReadyTag {
            get {
                return ResourceManager.GetString("PugSharp.Match.ReadyTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are !!not!! ready! Type `!ready` if you are ready..
        /// </summary>
        public static string PugSharp_Match_RemindReady {
            get {
                return ResourceManager.GetString("PugSharp.Match.RemindReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:teamName} selected 1:side} as startside!.
        /// </summary>
        public static string PugSharp_Match_SelectedTeam {
            get {
                return ResourceManager.GetString("PugSharp.Match.SelectedTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You voted for **{0:teamName}**.
        /// </summary>
        public static string PugSharp_Match_VotedForTeam {
            get {
                return ResourceManager.GetString("PugSharp.Match.VotedForTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You voted to ban **{0:mapName}**.
        /// </summary>
        public static string PugSharp_Match_VotedToBanMap {
            get {
                return ResourceManager.GetString("PugSharp.Match.VotedToBanMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vote to ban map: type `!&lt;mapnumber&gt;`.
        /// </summary>
        public static string PugSharp_Match_VoteMapMenuHeader {
            get {
                return ResourceManager.GetString("PugSharp.Match.VoteMapMenuHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Choose starting side:.
        /// </summary>
        public static string PugSharp_Match_VoteTeamMenuHeader {
            get {
                return ResourceManager.GetString("PugSharp.Match.VoteTeamMenuHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Waiting for other Team to vote!.
        /// </summary>
        public static string PugSharp_Match_WaitForOtherTeam {
            get {
                return ResourceManager.GetString("PugSharp.Match.WaitForOtherTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to type `!ready` to be marked as ready for the match.
        /// </summary>
        public static string PugSharp_NotifyReady {
            get {
                return ResourceManager.GetString("PugSharp.NotifyReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to powered by **PugSharp** (https://github.com/Lan2Play/PugSharp/).
        /// </summary>
        public static string PugSharp_PoweredBy {
            get {
                return ResourceManager.GetString("PugSharp.PoweredBy", resourceCulture);
            }
        }
    }
}
