using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Search;

namespace FortyFingers.DnnMassManipulate
{
    public partial class View : PortalModuleBase
    {
        private MassManipulateConfig config;
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/Views/_default/";

        protected void Page_Load(object sender, EventArgs e)
        {

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ClientResourceManager.RegisterScript(Page, "desktopmodules/40fingers/DnnMassManipulate/js/40F-Common.js", FileOrder.Js.jQuery);

            config = new MassManipulateConfig(Settings, ModuleId, TabModuleId);

            string msg;
            string retval;

            if (RazorUtils.Render(GetModel(), "Index.cshtml", ScriptsPath, ModuleContext, out retval, out msg))
            {
                ContentLiteralControl.Text = retval;
            }
            else if (UserInfo.IsSuperUser)
            {
                ContentLiteralControl.Text = msg;
            }
        }

        private ModulesModel GetModel()
        {
            var retval = new ModulesModel();

            var baseType = typeof(ManipulatorModuleBase);
            // get all assemblies in AppDomain
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            foreach (var assembly in allAssemblies)
            {
                // create a list to keep all found moduleTypes
                var moduleTypes = new List<Type>();
                // find all Types in the assembly that inherit the baseType
                // but not the basetype itself
                try
                {
                    moduleTypes.AddRange(assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t)).ToList());
                    foreach (var moduleType in moduleTypes)
                    {
                        var mModule = (ManipulatorModuleBase)Reflection.CreateObject(moduleType);
                        mModule.Context = new ContextHelper(ModuleConfiguration, UserInfo, PortalSettings);

                        bool.TryParse(this.Settings[$"Tab_{mModule.GetType().Name}"]?.ToString() ?? bool.TrueString, out var enabled);
                        mModule.Enabled = enabled;

                        retval.Modules.Add(mModule);
                    }
                }
                catch
                {
                }
            }

            // Place any module with TabName() == "Settings" at the end; otherwise order by TabName()
            retval.Modules = retval.Modules
                .OrderBy(m => string.Equals(m.TabName(), "Settings", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(m => m.TabName())
                .ToList();

            // the last one should be the settings module now
            var settingsModule = retval.Modules.Last() as SettingsModule;
            settingsModule.Modules = retval.Modules;
            settingsModule.DnnModuleSettings = this.ModuleContext.Settings;
            return retval;
        }

    }

    public class ModulesModel
    {
        public ModulesModel()
        {
            Modules = new List<ManipulatorModuleBase>();
        }
        public List<ManipulatorModuleBase> Modules { get; set; }
        public List<ManipulatorModuleBase> EnabledModules => Modules?.Where(m => m.Enabled).ToList();
    }
}