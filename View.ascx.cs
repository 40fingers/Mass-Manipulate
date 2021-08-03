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
                        var mModule = (ManipulatorModuleBase) Reflection.CreateObject(moduleType);
                        mModule.Context = new ContextHelper(ModuleConfiguration, UserInfo, PortalSettings);
                        retval.Modules.Add(mModule);
                        //var key = $"{providerType.FullName},{assembly.FullName.Split(',')[0]}";
                        //var providerConfig = GetFile(providerType);
                        //_instances.Add(key, providerConfig);
                    }
                }
                catch
                {
                }
            }

            retval.Modules = retval.Modules.OrderBy(m => m.TabName()).ToList();

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
    }
}