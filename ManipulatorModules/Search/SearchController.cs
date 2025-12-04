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
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.Api;
using FortyFingers.DnnMassManipulate.ManipulatorModules.Search;
using Newtonsoft.Json;

// Leave the ApiController in this namespace to avoid the need for a custom routemapper
namespace FortyFingers.DnnMassManipulate.Services
{
    [DnnModuleAuthorize]
    [SupportedModules("40Fingers.DnnMassManipulate")] // can be comma separated list of supported module
    public class SearchController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage Do(SearchPostModel model)
        {
            var s = InternalSearchController.Instance.GetSearchStatistics();
            var searchTypes = SearchHelper.Instance.GetSearchTypes().Select(st => st.SearchTypeId).ToArray();

            var searchQuery = new SearchQuery()
            {
                KeyWords = model.SearchInput,
                SearchTypeIds = searchTypes,
                PortalIds = new[] { PortalSettings.PortalId },
                PageSize = model.PageSize,
                PageIndex = model.PageIndex,
                WildCardSearch = true,
                AllowLeadingWildcard = true
            };
            if (!string.IsNullOrEmpty(model.SearchTags))
            {
                searchQuery.Tags = model.SearchTags?.Replace("\r\n", "\n").Split('\n');
            }

            var ret = "";
            try
            {
                var res = DotNetNuke.Services.Search.Controllers.SearchController.Instance.SiteSearch(searchQuery);
                ret = $"Your result: <pre>{JsonConvert.SerializeObject(res, Formatting.Indented)}</pre>";
            }
            catch (Exception e)
            {
                ret = $"Exception: <pre>{JsonConvert.SerializeObject(e, Formatting.Indented)}</pre>";
            }
            return Request.CreateResponse(HttpStatusCode.OK, ret);
        }

    }
}