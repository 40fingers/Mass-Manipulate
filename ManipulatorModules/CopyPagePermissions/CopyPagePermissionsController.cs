using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.ManipulatorModules.CopyPagePermissions;
using FortyFingers.Library;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class CopyPagePermissionsController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Do(CopyPagePermissionsPostModel model)
        {
            string sOut = ProcessRoleTabsRights(model.FromRole, model.ToRole, model.DoCopy ? "Copy" : "List"); //HandleRolesRegex(model.RolesRegex, model.DoRemove ? "Remove" : "Find");

            return Request.CreateResponse(HttpStatusCode.OK, new { message = sOut });
        }

        private string ProcessRoleTabsRights(string fromRole, string toRole, string mode)
        {
            string sOut = string.Empty;

            if (RoleExists(fromRole))
            {
                // Loop through all Tabs
                foreach (var oTab in TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, false))
                {
                    switch (mode)
                    {
                        case "List":
                            {
                                var sRoleReport = RoleRightsReport(fromRole, oTab);
                                if (sRoleReport.Length > 0)
                                    sOut += $"{sRoleReport} > \"{oTab.TabName}\" || <a href='{oTab.FullUrl}' target='_blank'>Open</a> <br>";
                                break;
                            }

                        case "Copy":
                            {
                                if (fromRole.ToLower() != toRole.ToLower())
                                    sOut += CopyPageRoleRights(oTab, fromRole, toRole);
                                else
                                    sOut = "From & To Roles cannot be the same";

                                if (sOut.Length > 0)
                                    DataCache.ClearCache();
                                break;
                            }
                    }
                }

                if (string.IsNullOrEmpty(sOut)) sOut = $"No permissions found for role {fromRole}.";
            }
            else
                sOut = $"Role \"{fromRole}\" not found";

            return sOut;
        }

        private string RoleRightsReport(string roleName, TabInfo oTab)
        {

            // Return a string with the found role rights
            string sOut = string.Empty;


            RoleController oRoleC = new RoleController();
            RoleInfo oRole = oRoleC.GetRoleByName(PortalSettings.PortalId, roleName);

            PermissionProvider permProv = new PermissionProvider();

            if (oRole != null)
            {
                foreach (TabPermissionInfo oTabPermission in TabPermissionController.GetTabPermissions(oTab.TabID,PortalSettings.PortalId))
                {
                    // Correct role?
                    if (oTabPermission.RoleName == roleName)
                    {
                        string s = string.Empty;

                        if (oTabPermission.AllowAccess == false)
                            s = "Deny ";
                        s += oTabPermission.PermissionName;
                        sOut = AddWithTrailingString(sOut, s, " / ");
                    }
                }
            }

            return sOut;
        }


        private bool RoleExists(string roleName)
        {
            RoleController oRoleC = new RoleController();
            RoleInfo oRole = oRoleC.GetRoleByName(PortalSettings.PortalId, roleName);

            if (oRole != null)
                return true;
            else
                return false;
        }


        //private bool RoleHasViewRights(string RoleName, TabInfo oTab)
        //{
        //    // Check if the passed role has view rights on the Passed tab

        //    int iViewId = System.Security.SecurityAccessLevel.View;

        //    RoleController oRoleC = new RoleController();
        //    RoleInfo oRole = oRoleC.GetRoleByName(PortalSettings.PortalId, RoleName);

        //    PermissionProvider permProv = new PermissionProvider();

        //    if (!oRole == null)
        //    {
        //        foreach (TabPermissionInfo oTP in TabPermissionController.GetTabPermissions(oTab.TabID, PortalId))
        //        {
        //            if (oTP.RoleName == RoleName & oTP.PermissionID == iViewId & oTP.AllowAccess == true)
        //            {
        //                return true;
        //                break;
        //            }
        //        }
        //    }

        //    return false;
        //}




        private TabPermissionInfo GetTabRolePermissions(string roleName, TabInfo oTab)
        {
            // Get tabpermissions for a Role on a Tab
            RoleController oRoleC = new RoleController();
            RoleInfo oRole = oRoleC.GetRoleByName(PortalSettings.PortalId, roleName);

            PermissionProvider permProv = new PermissionProvider();

            if (oRole != null)
            {
                foreach (TabPermissionInfo oTP in TabPermissionController.GetTabPermissions(oTab.TabID,PortalSettings.PortalId))
                {
                    if (oTP.RoleName == roleName)
                        return oTP;
                }
            }

            return null;
        }

        private string CopyPageRoleRights(TabInfo oTab, string fromRole, string toRole)
        {
            // Loop through the Permissions for this tab and when old role is found copy it to the new role
            string sOut = string.Empty;


            RoleController oRoleC = new RoleController();

            RoleInfo oFromRole = oRoleC.GetRoleByName(PortalSettings.PortalId, fromRole);
            RoleInfo oToRole = oRoleC.GetRoleByName(PortalSettings.PortalId, toRole);

            PermissionProvider oPermProv = new PermissionProvider();

            if (oFromRole != null && oToRole != null)
            {
                // Get the permissions for the page and check if there are any
                TabPermissionCollection oTabPermCol = TabPermissionController.GetTabPermissions(oTab.TabID, PortalSettings.PortalId);

                sOut += "<p>";

                // Get current list (changes while running this code)
                foreach (TabPermissionInfo oTabPermission in oTabPermCol.ToList())
                {
                    if (oTabPermission.RoleName == fromRole)
                        sOut += CopyPageRoleRight(oFromRole, oToRole, oTabPermission);
                }

                sOut += "</p>";
            }
            else
                sOut = $"Role {fromRole} not found";


            return sOut;
        }



        private string CopyPageRoleRight(RoleInfo roleInfoFrom, RoleInfo roleInfoTo, TabPermissionInfo tabPermInfo)
        {

            // Will copy a passed Tab Permission from one role to the other.


            string sOut = string.Empty;

            if (!(roleInfoFrom == null | roleInfoTo == null))
            {
                TabController oTabC = new TabController();

                // Get the page
                TabInfo oTab = oTabC.GetTab(tabPermInfo.TabID,PortalSettings.PortalId, true);

                // Create and set the new permissions Object
                var NewTabPerms = new TabPermissionInfo()
                {
                    TabID = oTab.TabID,
                    PermissionID = tabPermInfo.PermissionID,
                    PermissionName = tabPermInfo.PermissionName,
                    AllowAccess = tabPermInfo.AllowAccess,
                    RoleID = roleInfoTo.RoleID,
                    RoleName = roleInfoTo.RoleName
                };


                // Change its permissions

                // sOut &= "Before Remove : " & oTab.TabID & " >> " & RoleRightsReport(RoleInfoTo.RoleName, oTab) & "<br>"

                // Look for already existing permissions and remove them
                foreach (var permissionInfoBase in oTab.TabPermissions.ToList())
                {
                    var oTabPerm = (TabPermissionInfo) permissionInfoBase;
                    if (oTabPerm.RoleID == NewTabPerms.RoleID & oTabPerm.PermissionID == NewTabPerms.PermissionID)
                    {
                        oTab.TabPermissions.Remove(oTabPerm);

                        // sOut &= "After Remove : " & oTab.TabID & " >> " & RoleRightsReport(RoleInfoTo.RoleName, oTab) & "<br>"

                        sOut += $"Page: \"{oTab.TabName}\" || role: \"{roleInfoTo.RoleName}\" || Rights Removed: \"{NewTabPerms.PermissionName}\" <br />";
                    }
                }

                var TabPermCol = oTab.TabPermissions;

                TabPermCol.Add(NewTabPerms, true); // true if checkForDuplicates


                // Save the permissions
                PermissionProvider oPermissionProv = new PermissionProvider();
                oPermissionProv.SaveTabPermissions(oTab);


                sOut += $"Page: \"{oTab.TabName}\" || role: \"{roleInfoTo.RoleName}\" || Rights Set: \"{NewTabPerms.PermissionName}\" <br />";

                DataCache.ClearTabsCache(PortalSettings.PortalId);
            }
            else
                sOut += $"Role {roleInfoTo?.RoleName} does not exist <br />";

            return sOut;
        }




        private string SetPageViewRights(TabInfo oTab, string RoleName)
        {
            RoleController oRoleC = new RoleController();
            RoleInfo oRole = oRoleC.GetRoleByName(PortalSettings.PortalId, RoleName);

            if (oRole != null)
            {
                TabController oTabC = new TabController();
                TabPermissionInfo infPerm = new TabPermissionInfo();
                PermissionProvider permProv = new PermissionProvider();

                int RoleId = 0;

                infPerm.AllowAccess = true;
                infPerm.RoleID = oRole.RoleID;
                infPerm.RoleName = RoleName;
                infPerm.TabID = oTab.TabID;
                infPerm.PermissionID = 3; // View

                // Change its permissions
                TabPermissionCollection Perm = oTab.TabPermissions;

                Perm.Add(infPerm, true); // true if checkForDuplicates

                // Save the permissions
                permProv.SaveTabPermissions(oTab);


                return string.Format("Page {0}: role {1} View Rights Set <br />", oTab.TabName, RoleName);
            }
            else
                return string.Format("Role {0} does not exist <br />", RoleName);
        }

        public string AddWithTrailingString(string original, string addition, string trailing)
        {
            if (original.Length == 0)
                original += trailing;

            return (original + addition);
        }

    }
}