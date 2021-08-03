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

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.CopyPagePermissions
{
    public class CopyPagePermissionsModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/CopyPagePermissions/";

        public override string TabName()
        {
            return "Copy Page Permissions";
        }

        public override bool AllowAdministrator()
        {
            return true;
        }

        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new CopyPagePermissionsModel();
            model.Context = Context;

            if (RazorUtils.Render(model, "CopyPagePermissions.cshtml", ScriptsPath, null, out retval, out msg))
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