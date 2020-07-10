using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Common;
using DotNetNuke.Web.Api;
using DotNetNuke.Security;
using System.Threading;
using DotNetNuke.UI.Modules;
using DotNetNuke.Common.Utilities;
using System.Collections.Generic;
using FortyFingers.CacheManager.Components.BaseClasses;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json.Linq;

namespace FortyFingers.CacheManager.Services
{
    [SupportedModules("40F CacheManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class CacheManagerController : ApiControllerBase
    {
        public CacheManagerController() { }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCache()
        {
            DataCache.ClearCache();
            ClientResourceManager.ClearCache();

            return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
        }
    }
}