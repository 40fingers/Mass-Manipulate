using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.HashPasswords
{
    public class HashPasswordsModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/HashPasswords/";

        public override string TabName()
        {
            return "Hash Passwords";
        }

        public override bool AllowAdministrator()
        {
            return false;
        }

        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new HashPasswordsModel();
            model.Context = Context;

            if (RazorUtils.Render(model, "HashPasswords.cshtml", ScriptsPath, null, out retval, out msg))
            {
                return retval;
            }
            else if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return msg;
            }
            else
            {
                return "Something went wrong";
            }
        }

    }
}