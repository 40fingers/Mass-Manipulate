using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using DotNetNuke.Web.Api;

namespace FortyFingers.DnnMassManipulate.Services
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            // instruct DNN about the characteristics of the URL of your service
            mapRouteManager.MapHttpRoute("40FDMM",
                                         "default",
                                         "{controller}/{action}/{output}",
                                         new RouteValueDictionary { { "output", "default" } },
                                         new RouteValueDictionary { { "output", "xml|json|rss|atom|default" } },
                                         new[] { "FortyFingers.DnnMassManipulate.Services" });
        }
    }
}