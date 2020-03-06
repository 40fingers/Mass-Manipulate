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
using FortyFingers.DnnMassManipulate.ManipulatorModules.GenerateTabs;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Sample;
using FortyFingers.Library;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    public class SampleController : DnnApiController
    {
        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage Do(SamplePostModel model)
        {
            string ret = $"Your Input: \"{model.SampleInput}\"";
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

    }
}