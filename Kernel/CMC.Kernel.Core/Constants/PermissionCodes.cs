using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace CMC.Kernel.Core.Constants
{
    public static class PermissionCodes
    {
        //System
        public const string SystemSettings = "System.Settings";

        //Users
        public const string WebUsersView = "Web.Users.View";
        public const string WebUsersAdd = "Web.Users.Add";
        public const string WebUsersDelete = "Web.Users.Delete";


        //Players
        public const string WebPlayerView = "Web.Players.View";


        //Competitions
        public const string WebCompetitionView = "Web.Competition.View";
        public const string WebCompetitionCreate = "Web.Competition.Create";
        public const string WebCompetitionDelete = "Web.Competition.Delete";
        public const string WebCompetitionStart = "Web.Competition.Start";
    }
}
