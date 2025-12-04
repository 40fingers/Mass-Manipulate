using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Search;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Settings;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Modules;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class SettingsController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Save(SettingsPostModel model)
        {
            var ret = "";
            try
            {
                foreach (var module in model.Modules)
                {
                    ModuleController.Instance.UpdateModuleSetting(
                        ActiveModule.ModuleID,
                        $"Tab_{module.Module}",
                        module.Enabled.ToString());
                }
                ret = "Settings saved";
            }
            catch (Exception e)
            {
                ret = $"Exception: <pre>{JsonConvert.SerializeObject(e, Formatting.Indented)}</pre>";
            }
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

    }
}