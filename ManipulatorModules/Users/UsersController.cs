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
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.Components;
using FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Users;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class UsersController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage FindUsers(UsersPostModel model)
        {
            var retval = "";

            if (model.TemplateType == "REGEX")
            {
                retval = HandleUsersRegex(model.UsersInput, "Find", 50000);
                return Request.CreateResponse(HttpStatusCode.OK, retval);
            }

            var range = new Range(model.UsersInput, "");

            if (range.FromValue >= 0)
            {
                var sNumber = range.GetRangeFormat(0, 3);
                for (int i = range.FromValue; i <= range.ToValue; i++)
                {
                    var username = String.Format(range.RestString + sNumber, i);
                    var status = UserExists(username);

                    switch (status)
                    {
                        case -1:
                            break;
                        case 0:
                            retval += $"User: <del>{username}</del><br/>";
                            break;
                        case 1:
                            retval += $"User: {username}<br/>";
                            break;
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }
        [HttpPost]
        public HttpResponseMessage FindUsersDeleted(UsersPostModel model)
        {
            var retval = "";

            if (model.TemplateType == "REGEX")
            {
                retval = HandleUsersRegex(model.UsersInput, "FindDeleted", 50000);
                return Request.CreateResponse(HttpStatusCode.OK, retval);
            }

            var range = new Range(model.UsersInput, "");

            if (range.FromValue >= 0)
            {
                var sNumber = range.GetRangeFormat(0, 3);
                for (int i = range.FromValue; i <= range.ToValue; i++)
                {
                    var username = String.Format(range.RestString + sNumber, i);
                    var status = UserExists(username);

                    switch (status)
                    {
                        case -1:
                            break;
                        case 0:
                            retval += $"User: <del>{username}</del><br/>";
                            break;
                        case 1:
                            //retval += $"User: {username}<br/>";
                            break;
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }

        [HttpPost]
        public HttpResponseMessage GenerateUsers(UsersPostModel model)
        {
            var retval = "";

            var range = new Range(model.UsersInput, "");

            if (range.FromValue >= 0)
            {
                var sNumber = range.GetRangeFormat(0, 3);
                for (int i = range.FromValue; i <= range.ToValue; i++)
                {
                    var username = String.Format(range.RestString + sNumber, i);
                    var userError = CreateUser(username, model.UsersFolders);

                    retval += string.IsNullOrEmpty(userError)
                        ? $"User created: {username}<br />"
                        : $"Error creating User: {username} // {userError}<br />";
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }

        [HttpPost]
        public HttpResponseMessage DeleteUsers(UsersPostModel model)
        {
            var retval = "";

            if (model.TemplateType == "REGEX")
            {
                var mode = model.HardDelete ? "HardDelete" : "SoftDelete";
                retval = HandleUsersRegex(model.UsersInput, mode, 50000);
                return Request.CreateResponse(HttpStatusCode.OK, retval);
            }

            var range = new Range(model.UsersInput, "");

            if (range.FromValue >= 0)
            {
                var sNumber = range.GetRangeFormat(0, 3);
                for (int i = range.FromValue; i <= range.ToValue; i++)
                {
                    var username = String.Format(range.RestString + sNumber, i);
                    var status = DeleteUser(username, model.HardDelete);

                    switch (status)
                    {
                        case -1:
                            retval += $"User doesn't exist: {username}<br />";
                            break;
                        case 0:
                            retval += $"Cannot delete Site Admininistrator: {username}<br />";
                            break;
                        case 1:
                            retval += $"User deleted: {username}<br />";
                            break;
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, retval);
        }

        private int UserExists(string username)
        {
            var user = UserController.GetUserByName(PortalSettings.PortalId, username);

            if (user == null) return -1;

            return user.IsDeleted ? 0 : 1;
        }

        public string CreateUser(string Username, bool CreateUserFolder = false)
        {
            UserInfo oUser = new DotNetNuke.Entities.Users.UserInfo();

            oUser.AffiliateID = Null.NullInteger;
            oUser.Email = "user@email.com";
            oUser.FirstName = "FirstName";
            oUser.IsSuperUser = false;
            // I bet you will not create SuperUsers in bulk ;)

            oUser.LastName = "LastName";
            oUser.PortalID = PortalSettings.PortalId;
            oUser.Username = Username;
            oUser.DisplayName = Username;
            // Usually here FirstName+LastName

            oUser.Membership.Password = "test_password";
            // please check in web.config requirements for your password (length, letters, etc)
            oUser.Membership.Approved = true;
            oUser.Email = "user@email.com";
            oUser.Username = oUser.Username;
            oUser.Membership.UpdatePassword = false;

            oUser.Profile.Country = "Netherlands";
            oUser.Profile.Street = "Street";
            oUser.Profile.City = "City";
            oUser.Profile.Region = "Region";
            oUser.Profile.PostalCode = "PostalCode";
            oUser.Profile.Unit = "Unit";
            oUser.Profile.Telephone = "Telephone";
            oUser.Profile.FirstName = oUser.FirstName;
            oUser.Profile.LastName = oUser.LastName;

            // Actually create the user
            DotNetNuke.Security.Membership.UserCreateStatus objCreateStatus = UserController.CreateUser(ref oUser);
            // everything fine
            if (objCreateStatus == DotNetNuke.Security.Membership.UserCreateStatus.Success)
            {
                if (CreateUserFolder)
                    FolderManager.Instance.GetUserFolder(oUser);

                return "";
            }
            else
                return objCreateStatus.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="hardDelete"></param>
        /// <returns>0 = error, 1 = removed, -1 = does not exist</returns>
        public int DeleteUser(string username, bool hardDelete)
        {
            // Permanently removes a user
            // Returns: 0 = error, 1 = removed, -1 = does not exist

            UserInfo oUser = UserController.GetUserByName(PortalSettings.PortalId, username);

            if (oUser == null)
                return -1;

            if (oUser.UserID != PortalSettings.AdministratorId)
            {
                UserController.DeleteUser(ref oUser, false, false);
                if (hardDelete == true)
                    UserController.RemoveUser(oUser);
                return (1);
            }
            else
                return (0);
        }

        private string HandleUsersRegex(string UserRegex, string Mode, int MaxUsers)
        {
            string sOut = string.Empty;

            int int_TotalRecords = 0;
            int int_GetRecords = MaxUsers;
            var int_HandledRecords = 0;

            ArrayList oUsers = UserController.GetUsers(PortalSettings.PortalId, 0, int_GetRecords, ref int_TotalRecords, true, false);

            foreach (UserInfo oUser in oUsers)
            {
                if (Regex.IsMatch(oUser.Username, UserRegex, RegexOptions.IgnoreCase))
                {
                    switch (Mode)
                    {
                        case "Find":
                            {
                                sOut += oUser.IsDeleted ? $"User: <del>{oUser.Username}</del><br/>" : $"User: {oUser.Username}<br/>";

                                int_HandledRecords += 1;
                                break;
                            }

                        case "FindDeleted":
                            {
                                sOut += oUser.IsDeleted ? $"User: <del>{oUser.Username}</del><br/>" : $"";

                                int_HandledRecords += 1;
                                break;
                            }

                        case "SoftDelete":
                            {
                                if (DeleteUser(oUser.Username, false) == 1)
                                {
                                    sOut += string.Format("User {0}: Deleted<br />", oUser.Username);
                                    int_HandledRecords += 1;
                                }
                                else
                                    sOut += string.Format("User {0}: Cannot Delete default Administrator<br />", oUser.Username);
                                break;
                            }

                        case "HardDelete":
                            {
                                if (DeleteUser(oUser.Username, true) == 1)
                                {
                                    sOut += string.Format("User {0}: Removed<br />", oUser.Username);
                                    int_HandledRecords += 1;
                                }
                                else
                                    sOut += string.Format("User {0}: Cannot Remove default Administrator<br />", oUser.Username);
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

            return sOut;
        }
    }
}