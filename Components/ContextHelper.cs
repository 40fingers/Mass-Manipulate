using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace FortyFingers.DnnMassManipulate.Components
{
    /// <summary>
    /// ContextHelper class for ModuleContext
    /// </summary>
    public class ContextHelper
    {
        /// <summary>
        /// ModuleContext itself
        /// </summary>
        public ModuleInfo Module { get; set; }
        /// <summary>
        /// Reference to WebPage containing the module
        /// </summary>
        public System.Web.UI.Page Page { get; set; }
        /// <summary>
        /// Reference to the user requesting the page
        /// </summary>
        public UserInfo User { get; set; }

        /// <summary>
        /// Reference tp the current portal's portalsettings
        /// </summary>
        public PortalSettings Portal { get; set; }

        /// <summary>
        /// Instanciates ContextHelper from DNN ApiController
        /// </summary>
        /// <param name="context"></param>
        public ContextHelper(DnnApiController context)
        {
            Requires.NotNull("context", context);
            Module = context.ActiveModule;
            User =context.UserInfo;
            Portal = context.PortalSettings;
        }

        /// <summary>
        /// Instanciates ContextHelper from DNN ApiController
        /// </summary>
        public ContextHelper(ModuleInfo module, UserInfo user, PortalSettings ps)
        {
            Module = module;
            User =user;
            Portal = ps;
        }


    }
}