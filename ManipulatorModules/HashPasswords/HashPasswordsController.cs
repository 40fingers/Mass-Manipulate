using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.Components._40FingersLib;
using FortyFingers.DnnMassManipulate.ManipulatorModules.HashPasswords;
using Newtonsoft.Json.Linq;

namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class HashPasswordsController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage GetEncryptedPasswords(JObject model)
        {
            var log = new StringBuilder();
            var cntAlreadyHashed = 0;
            var cntRetrieved = 0;
            var cntAdmins = 0;

            var retval = new HashPasswordsModel();

            try
            {
                var allUsers = UserController.GetUsers(PortalSettings.PortalId).ToList<UserInfo>();
                for (int i = 0; i < allUsers.Count - 1; i++)
                {
                    var user = allUsers[i];
                    if (user.IsSuperUser || user.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        cntAdmins++;
                        continue;
                    }

                    try
                    {
                        var pw = UserController.GetPassword(ref user, "");
                        cntRetrieved++;

                        retval.Passwords.Add(new PasswordModel()
                        {
                            UserId = user.UserID, Username = user.Username, Password = pw
                        });
                    }
                    catch
                    {
                        cntAlreadyHashed++;
                    }
                }

                log.AppendLine();
                log.AppendLine($"Passwords for admin/superusers skipped: {cntAdmins}");
                log.AppendLine($"Passwords retrieved to be hashed: {cntRetrieved}");
                log.AppendLine($"Passwords already hashed: {cntAlreadyHashed}");

            }
            catch (Exception e)
            {
                log.AppendLine($"Error occurred: {e.Message}");
                log.AppendLine($"Error stacktrace: {e.StackTrace}");
            }

            retval.Log = $"<pre>{log.ToString()}</pre>";
            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }
        [HttpPost]
        public HttpResponseMessage HashEncryptedPasswords(List<PasswordModel> model)
        {
            var retval = new StringBuilder();
            var cntAlreadyHashed = 0;
            var cntChanged = 0;
            var cntFailed = 0;
            var cntAdmins = 0;

            try
            {
                foreach (var passwordModel in model)
                {
                    var user = UserController.Instance.GetUser(PortalSettings.PortalId, passwordModel.UserId);
                    if (user == null)
                    {
                        continue;
                    }
                    if (user.IsSuperUser || user.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        cntAdmins++;
                        continue;
                    }

                    try
                    {
                        var changed = UserController.ChangePassword(user, passwordModel.Password, passwordModel.Password);

                        if (changed)
                            cntChanged++;
                        else
                            cntFailed++;
                    }
                    catch
                    {
                        cntAlreadyHashed++;
                    }
                }
                retval.AppendLine($"Passwords for admin/superusers skipped: {cntAdmins}");
                retval.AppendLine($"Passwords hashed: {cntChanged}");
                retval.AppendLine($"Passwords failed: {cntFailed}");
                retval.AppendLine($"Passwords already hashed: {cntAlreadyHashed}");

            }
            catch (Exception e)
            {
                retval.AppendLine($"Error occurred: {e.Message}");
                retval.AppendLine($"Error stacktrace: {e.StackTrace}");
            }

            return Request.CreateResponse(HttpStatusCode.OK, $"<pre>{retval.ToString()}</pre>");
        }
    }
}