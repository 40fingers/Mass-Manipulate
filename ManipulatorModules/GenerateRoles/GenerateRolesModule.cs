using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateRoles
{
    public class GenerateRolesModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/GenerateRoles/";

        public override string TabName()
        {
            return "Generate Roles";
        }

        public override bool AllowAdministrator()
        {
            return true;
        }

        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new GenerateRolesModel();
            model.Context = Context;

            if (RazorUtils.Render(model, "GenerateRoles.cshtml", ScriptsPath, null, out retval, out msg))
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