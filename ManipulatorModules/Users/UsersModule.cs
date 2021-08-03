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

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Users
{
    public class UsersModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/Users/";

        /// <summary>
        /// Return the name of the tab in MassManipulator
        /// </summary>
        /// <returns></returns>
        public override string TabName()
        {
            return "Users";
        }

        /// <summary>
        /// Return wether or not tab should be visible for Admins (if false: only superusers will see it)
        /// </summary>
        /// <returns></returns>
        public override bool AllowAdministrator()
        {
            return true;
        }

        /// <summary>
        /// Return the HTML for the tab contents.
        /// </summary>
        /// <returns></returns>
        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new UsersModel();
            model.Context = Context;

            if (RazorUtils.Render(model, "Users.cshtml", ScriptsPath, null, out retval, out msg))
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