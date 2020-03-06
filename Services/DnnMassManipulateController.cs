using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace FortyFingers.DnnMassManipulate.Services
{
    // The name of the controller MUST end with "Controller"
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class DnnMassManipulateController : DnnApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage HelloWorld()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World!");
        }

        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage HelloYou(string name)
        {
            return Request.CreateResponse(HttpStatusCode.OK, String.Format("Hello {0}!", name));
        }

    }
}