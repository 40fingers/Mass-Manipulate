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
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateRoles;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class GenerateRolesController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Do(GenerateRolesPostModel model)
        {
            string sOut = string.Empty;
            var strTemplate = model.RolesTemplate;

            Range oRange = new Range(strTemplate, "");

            if (oRange.FromValue > -1)
            {
                string strNumber = oRange.GetRangeFormat(0, 3);

                for (int i = oRange.FromValue; i <= oRange.ToValue; i++)
                {
                    string strRoleName = string.Format(oRange.RestString + strNumber, i);


                    if (CreateRole(strRoleName))
                        sOut += string.Format("Role created: {0}<br />", strRoleName);
                    else
                        sOut += string.Format("Role already Exists: {0}<br />", strRoleName);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { message = sOut });
        }
        private bool CreateRole(string RoleName)
        {
            RoleController oDnnRoleController = new RoleController();

            RoleInfo oRole1 = oDnnRoleController.GetRoleByName(PortalSettings.PortalId, RoleName);

            if (oRole1 == null)
            {
                // Role does not exist yet
                var oRole = new RoleInfo()
                {
                    PortalID = PortalSettings.PortalId,
                    RoleName = RoleName,
                    IsPublic = false,
                    AutoAssignment = false,
                    RoleGroupID = Null.NullInteger,
                    Description = "Auto generated Role",
                    Status = RoleStatus.Approved,
                };
                oDnnRoleController.AddRole(oRole);
                return (true);
            }
            else
                // Role already exists
                return false;
        }

    }
}