using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;
using FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs;

namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class GenerateTabsController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage DoOne(GenerateOneTabPostModel model)
        {
            var objTabs = new TabController();
            var startingLevel = 0;
            var parentTabId = Null.NullInteger;
            TabPermissionCollection defaultTabPermissionCollection = CreateTabPermissions();

            // see if user selected another page to put the tree under. 
            // we need the startingTabID and the StartingLevel
            if (model.ParentTabId > 0)
            {
                TabInfo tempTab = objTabs.GetTab(model.ParentTabId, PortalSettings.PortalId, true);
                startingLevel = tempTab.Level + 1;
                parentTabId = model.ParentTabId;
            }

            string prevTabName = "";
            int prevLevel = 0;
            TabInfo prevTab = null;
            if (model.PreviousTabId == 0)
            {
                // it's the first tab
                parentTabId = model.ParentTabId;
            }
            else
            {
                prevTabName = model.PreviousTabName;
                prevLevel = startingLevel;
                prevTab = objTabs.GetTab(model.PreviousTabId, PortalSettings.PortalId, true);
                GetLevelFromPageName(model.IndentCharacter, ref prevTabName, ref prevLevel);
            }

            var tabName = model.TabName;
            var level = startingLevel;
            GetLevelFromPageName(model.IndentCharacter, ref tabName, ref level);

            if (level == 0)
            {
                // top level page: nothing to do
            }
            else if (model.PreviousTabId > 0 && level == prevLevel)
            {
                // sibling of previous page, so same parent
                parentTabId = prevTab.ParentId;
            }
            else if (model.PreviousTabId > 0 && level == prevLevel + 1)
            {
                // child of previous tab
                parentTabId = model.PreviousTabId;
            }
            else if (model.PreviousTabId > 0 && level < prevLevel)
            {
                var tempTab = prevTab;
                for (int i = 0; i < prevLevel - level; i++)
                {
                    tempTab = objTabs.GetTab(tempTab.ParentId, PortalSettings.PortalId, true);
                }

                parentTabId = tempTab.ParentId;
            }

            // create the new page
            TabInfo newTab = new TabInfo()
            {
                PortalID = PortalSettings.PortalId,
                TabName = tabName,
                ParentId = parentTabId > 0 ? parentTabId : Null.NullInteger,
                IsVisible = true,
                DisableLink = false,
                IsSecure = false,
                PermanentRedirect = false,
                SiteMapPriority = (float)0.5,
                Level = level
            };
            newTab.TabPermissions.AddRange(defaultTabPermissionCollection);

            try
            {
                // adding the tab returns a tabId, so we can set that to the tab object
                newTab.TabID = objTabs.AddTab(newTab);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { TabId = newTab.TabID });
        }

        [HttpPost]
        public HttpResponseMessage ValidatePageTree(GenerateTabsPostModel model)
        {
            int level = 0;
            int previousLevel = -1;
            int lineCounter = 0;

            foreach (string pageName in model.PageTree.Split('\n'))
            {
                // reset level to 0
                level = 0;
                // save the original pagenane, since GetLevelFromPageName is going to alter the pagename
                string originalPageName = pageName;
                string newPageName = pageName;

                // get the level from the pagename. This removes the indent characters from the beginning from the pagename
                level = GetLevelFromPageName(model.IndentCharacter, ref newPageName, ref level);

                // there might a typo in the list. 
                // eg if item x has text ">>PageName", 
                // then item x+1 cannot have either just "PageName" or ">>>>PageName"
                // in other words, levels cannot differ by more than 1
                if (previousLevel < 0 && pageName.StartsWith(model.IndentCharacter))
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, $"PageTree being added cannot start with the set indent character ({model.IndentCharacter})");
                }
                else if (previousLevel >= 0 && (level - previousLevel > 1) && (level != 0))
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Unexpected Level at line {lineCounter + 1}, text: {originalPageName}");
                }

                lineCounter++;
                previousLevel = level;
            }
            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [HttpPost]
        public HttpResponseMessage Do(GenerateTabsPostModel model)
        {
            List<string> PageTree = model.PageTree.Split('\n').ToList();

            TabController objTabs = new TabController();

            if (PageTree.First().StartsWith(model.IndentCharacter))
            {
                // we have to make sure that the first item in the list is not starting with an indent,
                // since we are adding this tree (optionaly) below another page anyway
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"PageTree being added cannot start with the set indent character ({model.IndentCharacter})");

            }

            TabPermissionCollection defaultTabPermissionCollection = CreateTabPermissions();

            int level = 0;
            int startingLevel = 0;
            int previousLevel = -1;
            int lineCounter = 0;
            int startingParentTabId = Null.NullInteger;

            // see if user selected another page to put the tree under. 
            // we need the startingTabID and the StartingLevel
            if (model.ParentTabId > 0)
            {
                startingParentTabId = model.ParentTabId;
                TabInfo tempTab = objTabs.GetTab(startingParentTabId, PortalSettings.PortalId, true);
                startingLevel = tempTab.Level + 1;
            }
            int parentTabId = Null.NullInteger;
            List<TabInfo> tabList = new List<TabInfo>();

            // first check whether all levels are ok, if there is an error we are not going to do anything
            foreach (string pageName in PageTree)
            {
                // reset level to 0
                level = 0;
                // save the original pagenane, since GetLevelFromPageName is going to alter the pagename
                string originalPageName = pageName;
                string newPageName = pageName;

                // get the level from the pagename. This removes the indent characters from the beginning from the pagename
                level = GetLevelFromPageName(model.IndentCharacter, ref newPageName, ref level);

                // there might a typo in the list. 
                // eg if item x has text ">>PageName", 
                // then item x+1 cannot have either just "PageName" or ">>>>PageName"
                // in other words, levels cannot differ by more than 1
                if ((level - previousLevel > 1) && (level != 0))
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, $"Unexpected Level at line {lineCounter + 1}, text: {originalPageName}");
                }
                lineCounter += 1;
                previousLevel = level;
            }

            lineCounter = 0;
            previousLevel = -1;
            foreach (string pageName in PageTree)
            {
                level = 0;
                // save the original pagename, since GetLevelFromPageName is going to alter the pagename
                string originalPageName = pageName;
                string newPageName = pageName;

                // get the level from the pagename. This removes the indent characters from the beginning from the pagename
                level = GetLevelFromPageName(model.IndentCharacter, ref newPageName, ref level);

                // get the parentTabId
                parentTabId = startingParentTabId;
                if ((level > 0) && (lineCounter >= tabList.Count))
                {
                    // For i As Integer = tabList.Count - 1 To 0 Step -1
                    for (int i = 0; i <= tabList.Count - 1; i++)
                    {
                        // walk the list of already created back up
                        // in order to find the first previous tab of 1 level up
                        if (tabList[i].Level == startingLevel + level - 1)
                            parentTabId = tabList[i].TabID;
                    }
                }

                // create the new page
                TabInfo newTab = new TabInfo()
                {
                    PortalID = PortalSettings.PortalId,
                    TabName = newPageName,
                    ParentId = parentTabId,
                    IsVisible = true,
                    DisableLink = false,
                    IsSecure = false,
                    PermanentRedirect = false,
                    SiteMapPriority = (float)0.5,
                    Level = level
                };
                newTab.TabPermissions.AddRange(defaultTabPermissionCollection);

                try
                {
                    // adding the tab returns a tabId, so we can set that to the tab object
                    newTab.TabID = objTabs.AddTab(newTab);

                    // add the newly created tab to a list
                    tabList.Add(newTab);
                }
                catch (Exception ex)
                {
                }

                lineCounter += 1;
                previousLevel = level;
            }

            // Clear the tabcache, so we know for sure that we will get a correct menu next page refresh
            //DotNetNuke.Common.Utilities.DataCache.ClearTabsCache(PortalId);

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }
        private TabPermissionCollection CreateTabPermissions()
        {
            TabPermissionCollection returnCollection = new TabPermissionCollection();

            PermissionController pc = new PermissionController();
            var viewPermission = pc.GetPermissionByCodeAndKey("SYSTEM_TAB", "VIEW").ToList<PermissionInfo>();
            var editPermission = pc.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT").ToList<PermissionInfo>();
            int adminRoleId = PortalSettings.AdministratorRoleId;
            TabPermissionInfo objViewPermission = new TabPermissionInfo(viewPermission[0]);

            objViewPermission.TabID = PortalSettings.ActiveTab.TabID;
            objViewPermission.RoleID = PortalSettings.AdministratorRoleId;
            objViewPermission.RoleName = PortalSettings.AdministratorRoleName;
            objViewPermission.AllowAccess = true;
            objViewPermission.UserID = Null.NullInteger;
            objViewPermission.DisplayName = "";
            returnCollection.Add(objViewPermission, true);

            TabPermissionInfo objEditPermission = new TabPermissionInfo(editPermission[0]);
            objEditPermission.TabID = PortalSettings.ActiveTab.TabID;
            objEditPermission.RoleID = PortalSettings.AdministratorRoleId;
            objEditPermission.RoleName = PortalSettings.AdministratorRoleName;
            objEditPermission.AllowAccess = true;
            objEditPermission.UserID = Null.NullInteger;
            objEditPermission.DisplayName = "";
            returnCollection.Add(objEditPermission, true);

            return returnCollection;
        }
        /// <summary>
        ///     ''' Gets the level of the page from the page name in the list.
        ///     ''' </summary>
        ///     ''' <param name="pageName">Name of the page.</param>
        ///     ''' <param name="beginLevel">The begin level.</param>
        ///     ''' <returns></returns>
        private int GetLevelFromPageName(string indentChar, ref string pageName, ref int beginLevel)
        {
            if (!pageName.StartsWith(indentChar))
                return beginLevel;
            else
            {
                pageName = pageName.Substring(1);
                beginLevel += 1;
                return GetLevelFromPageName(indentChar, ref pageName, ref beginLevel);
            }
        }

    }
}