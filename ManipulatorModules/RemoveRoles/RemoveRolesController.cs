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
using FortyFingers.DnnMassManipulate.ManipulatorModules.RemoveRoles;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class RemoveRolesController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Do(RemoveRolesPostModel model)
        {
            string sOut = HandleRolesRegex(model.RolesRegex, model.DoRemove ? "Remove" : "Find");

            return Request.CreateResponse(HttpStatusCode.OK, new { message = sOut });
        }

        private string GetDeletedLink(string Name, bool IsDeleted)
        {
            return $"Role:  {(IsDeleted ? "<del>" : "")}{Name}{(IsDeleted ? "</del>" : "")}<br />";
        }

        private string HandleRolesRegex(string RoleRegex, string Mode)
        {
            string sOut = string.Empty;

            ArrayList oRoles = RoleProvider.Instance().GetRoles(PortalSettings.PortalId);

            foreach (RoleInfo oRole in oRoles)
            {
                if (Regex.IsMatch(oRole.RoleName, RoleRegex, RegexOptions.IgnoreCase))
                {
                    switch (Mode)
                    {
                        case "Find":
                        {
                            sOut += GetDeletedLink(oRole.RoleName, false);
                            break;
                        }

                        case "Remove":
                        {
                            if (DeleteRole(oRole.RoleName) == true)
                                sOut += $"Role {oRole.RoleName}: Removed<br />";
                            else
                                sOut += $"Role {oRole.RoleName}: Cannot Remove Role<br />";
                            break;
                        }

                        default:
                        {
                            sOut += string.Format("No mode Selected");
                            break;
                        }
                    }
                }
            }

            if (sOut == string.Empty)
                sOut = "No Roles Found";

            return (sOut);
        }
        private bool DeleteRole(string RoleName)
        {
            RoleController oDnnRoleController = new RoleController();

            RoleInfo oRole1 = oDnnRoleController.GetRoleByName(PortalSettings.PortalId, RoleName);

            if (oRole1 != null)
            {
                // Role does exist

                // Check not to remove Admin / registered users & Subscribers
                if (oRole1.RoleType == RoleType.Administrator || oRole1.RoleType == RoleType.RegisteredUser || oRole1.RoleType == RoleType.Subscriber)
                    return false;
                else
                {
                    RoleController.Instance.DeleteRole(oRole1);
                    return true;
                }
            }
            else
                // Role already exists
                return false;
        }

    }
}