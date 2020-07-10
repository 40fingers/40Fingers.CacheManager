using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Web.Api;

namespace FortyFingers.CacheManager.Components.BaseClasses
{
    public class ApiControllerBase : DnnApiController
    {
        private const string DBCONTEXT_KEY = "CacheManagerContext_Instance";

    }
}