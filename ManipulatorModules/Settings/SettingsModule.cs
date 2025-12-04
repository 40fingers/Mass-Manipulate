using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Settings;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.Search
{
    public class SettingsModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/Settings/";

        /// <summary>
        /// Return the name of the tab in MassManipulator
        /// </summary>
        /// <returns></returns>
        public override string TabName()
        {
            return "Settings";
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
            var model = new SettingsModel();
            model.Context = Context;
            model.Help = Localization.GetString("Help.Html",
                "~\\DesktopModules\\40Fingers\\DnnMassManipulate\\ManipulatorModules\\Settings\\App_LocalResources\\Settings.resx");

            foreach (var mModule in Modules.Where(m => m.TabName() != this.TabName()))
            {
                bool.TryParse(DnnModuleSettings[$"Tab_{mModule.GetType().Name}"]?.ToString() ?? bool.TrueString, out var enabled);
                model.Modules.Add(new KeyValuePair<string, (string, bool)>(mModule.GetType().Name, (mModule.TabName(), enabled)));
            }

            if (RazorUtils.Render(model, "Settings.cshtml", ScriptsPath, null, out retval, out msg))
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

        public List<ManipulatorModuleBase> Modules { get; set; } = new List<ManipulatorModuleBase>();
        public Hashtable DnnModuleSettings { get; set; }
    }
}
