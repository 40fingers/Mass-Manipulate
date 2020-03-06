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
using FortyFingers.FortyFingersLib;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs
{
    public class GenerateTabsModule : ManipulatorModuleBase
    {
        private readonly string ScriptsPath = $"~/DesktopModules/40Fingers/DnnMassManipulate/ManipulatorModules/GenerateTabs/";

        public override string TabName()
        {
            return "Add Pages";
        }

        public override bool AllowAdministrator()
        {
            return true;
        }

        public override string GetHtml()
        {
            string msg;
            string retval;
            var model = new GenerateTabsModel();
            model.Context = Context;
            model.Tabs = GetTabs(true, true, true);

            if (RazorUtils.Render(model, "GenerateTabs.cshtml", ScriptsPath, null, out retval, out msg))
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

        /// <summary>
        ///     ''' Gets the tabs.
        ///     ''' </summary>
        ///     ''' <param name="includeCurrent">if set to <c>true</c> [include current].</param>
        ///     ''' <param name="includeURL">if set to <c>true</c> [include URL].</param>
        ///     ''' <param name="includeParent">if set to <c>true</c> [include parent].</param>
        ///     ''' <returns></returns>
        private List<TabInfo> GetTabs(bool includeCurrent, bool includeURL, bool includeParent)
        {
            string noneSpecified = "<" + Localization.GetString("None_Specified") + ">";
            List<TabInfo> tabs = new List<TabInfo>();
            TabController controller = new TabController();

            var ps = PortalSettings.Current;
            var user = UserController.Instance.GetCurrentUserInfo();

            int excludeTabId = Null.NullInteger;
            if (!includeCurrent)
                excludeTabId = ps.ActiveTab.TabID;

            if (ps.ActiveTab.IsSuperTab)
            {
                TabInfo objTab = new TabInfo();
                objTab.TabID = -1;
                objTab.TabName = noneSpecified;
                objTab.TabOrder = 0;
                objTab.ParentId = -2;
                tabs.Add(objTab);

                TabController.Instance.GetTabsByPortal(Null.NullInteger).ForEach(kvp => tabs.Add(kvp.Value));
            }
            else
            {
                tabs = TabController.GetPortalTabs(ps.PortalId, excludeTabId, PortalSecurity.IsInRole("Administrators"), noneSpecified, true, false, includeURL, false, true);
                // Need to include the Parent Tab if its not already in the list of tabs
                var parentTab = tabs.FirstOrDefault(t => t.TabID == ps.ActiveTab.ParentId);

                if (includeParent && ps.ActiveTab.ParentId != Null.NullInteger & parentTab == null)
                    tabs.Add(controller.GetTab(ps.ActiveTab.ParentId, ps.PortalId, false));
                if (user.IsSuperUser & ps.ActiveTab.TabID == Null.NullInteger)
                    TabController.Instance.GetTabsByPortal(Null.NullInteger).ForEach(kvp => tabs.Add(kvp.Value));
            }

            return tabs;
        }

    }
}