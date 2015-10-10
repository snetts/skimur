﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace Subs.Commands
{
    public class ChangeModPermissionsForSub : ICommandReturns<ChangeModPermissionsForSubResponse>
    {
        public string SubName { get; set; }

        public Guid? SubId { get; set; }

        public Guid RequestingUser { get; set; }

        public string UserNameToChange { get; set; }

        public Guid? UserIdToChange { get; set; }

        public ModeratorPermissions Permissions { get; set; }
    }

    public class ChangeModPermissionsForSubResponse
    {
        public string Error { get; set; }
    }
}
