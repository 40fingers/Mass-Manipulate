using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.ManipulatorModules.DbReplace;
using FortyFingers.FortyFingersLib;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.DbReplace
{
    public class DbReplaceModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/DbReplace/";

        /// <summary>
        /// Return the name of the tab in MassManipulator
        /// </summary>
        /// <returns></returns>
        public override string TabName()
        {
            return "DB Text Replace";
        }

        /// <summary>
        /// Return wether or not tab should be visible for Admins (if false: only superusers will see it)
        /// </summary>
        /// <returns></returns>
        public override bool AllowAdministrator()
        {
            return false;
        }

        /// <summary>
        /// Return the HTML for the tab contents.
        /// </summary>
        /// <returns></returns>
        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new DbReplaceModel();
            model.Context = Context;
            model.Help = Localization.GetString("Help.Html",
                "~\\DesktopModules\\40Fingers\\DnnMassManipulate\\ManipulatorModules\\DbReplace\\App_LocalResources\\DbReplace.resx");

            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
            {
                model.ConnectionStrings.Add(new KeyValuePair<string, string>(cs.Name, cs.ConnectionString));
            }
            
            if (RazorUtils.Render(model, "DbReplace.cshtml", ScriptsPath, null, out retval, out msg))
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