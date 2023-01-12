using System;
using System.Collections;
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
using System.Net;
using System.Text;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using FortyFingers.CacheManager.Components.BaseClasses;
using DotNetNuke.Web.Client.ClientResourceManagement;
using FortyFingers.CacheManager.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FortyFingers.CacheManager.Services
{
    [SupportedModules("40F CacheManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class CacheManagerController : ApiControllerBase
    {
        private const string TaskTypeName = "FortyFingers.CacheManager.Components.ConditionalCacheClearerTask";
        private const string TaskAssembly = "40Fingers.CacheManager";

        public CacheManagerController() { }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCache()
        {
            DataCache.ClearCache();
            ClientResourceManager.ClearCache();

            return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCacheKeys()
        {
            var setting = PortalController.GetPortalSetting(Constants.PsCacheKeysToClear(PortalSettings.PortalId), PortalSettings.PortalId, null);
            var cacheKeysModel = new CacheKeysModel();
            if (!string.IsNullOrEmpty(setting))
            {
                try
                {
                    cacheKeysModel.Keys = JsonConvert.DeserializeObject<List<CacheKeyModel>>(setting);
                }
                catch
                {
                    cacheKeysModel.Keys = new List<CacheKeyModel>();
                }
            }

            foreach (var cachedKey in cacheKeysModel.Keys)
            {
                DataCache.RemoveCache(cachedKey.Key);
            }
            return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ActionName("SaveKeys")]
        public HttpResponseMessage SaveKeys()
        {
            try
            {
                var keys = Request.GetHttpContext().Request["Keys"];
                var model = new CacheKeysModel();
                model.Keys = JsonConvert.DeserializeObject<List<CacheKeyModel>>(keys);
                var taskEnabled = bool.Parse(Request.GetHttpContext().Request["TaskEnabled"]);

                PortalController.Instance.UpdatePortalSetting(PortalSettings.PortalId, Constants.PsCacheKeysToClear(PortalSettings.PortalId), keys, true, null, false);

                if (SchedulerHelper.IsScheduleItemEnabled(TaskTypeName, TaskAssembly) != taskEnabled)
                {
                    if (taskEnabled)
                        SchedulerHelper.EnableScheduleItem(TaskTypeName, TaskAssembly);
                    else
                        SchedulerHelper.DisableScheduleItem(TaskTypeName, TaskAssembly);
                }

                return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Json");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ActionName("MatchKeys")]
        public HttpResponseMessage MatchKeys()
        {
            try
            {
                var keys = Request.GetHttpContext().Request["Keys"];
                var model = new CacheKeysModel();
                model.Keys = JsonConvert.DeserializeObject<List<CacheKeyModel>>(keys);

                var currentKeys = Common.GetCurrentCachedKeys();
                var retval = new List<string>();
                foreach (var cacheKeyModel in model.Keys)
                {
                    retval.Add($"BEGIN KEY {cacheKeyModel.Key}");
                    retval.AddRange(Common.GetMatchingCachedKeys(currentKeys, cacheKeyModel));
                    retval.Add($"END KEY");
                    retval.Add("");
                }
                
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Json");
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ActionName("GetSetting")]
        public HttpResponseMessage GetSetting()
        {
            try
            {
                var setting = PortalController.GetPortalSetting(Constants.PsCacheKeysToClear(PortalSettings.PortalId), PortalSettings.PortalId, null);
                var retval = new CacheKeysModel();
                if (!string.IsNullOrEmpty(setting))
                {
                    try
                    {
                        retval.Keys = JsonConvert.DeserializeObject<List<CacheKeyModel>>(setting);
                    }
                    catch
                    {
                        retval.Keys = new List<CacheKeyModel>();
                    }
                }
                retval.TaskEnabled = SchedulerHelper.IsScheduleItemEnabled(TaskTypeName, TaskAssembly);
                if (!retval.Keys.Any())
                {
                    retval = new CacheKeysModel();
                    retval.Keys.Add(new CacheKeyModel() { Key = $"DNN_Folders{PortalSettings.PortalId}", Query = "SELECT CASE WHEN EXISTS(SELECT * FROM Folders WHERE LastModifiedOnDate > [LASTCLEARED]) THEN 1 ELSE 0 END" });
                    retval.Keys.Add(new CacheKeyModel() { Key = $"DNN_Folders\\d*", IsRegex = true, Query = "SELECT CASE WHEN EXISTS(SELECT * FROM Folders WHERE LastModifiedOnDate > [LASTCLEARED]) THEN 1 ELSE 0 END" });
                }

                var sLastCleared = PortalController.GetPortalSetting(Constants.PsLastCacheCleared(PortalSettings.PortalId), PortalSettings.PortalId, "");
                if (sLastCleared == "")
                {
                    PortalController.UpdatePortalSetting(PortalSettings.PortalId,
                        Constants.PsLastCacheCleared(PortalSettings.PortalId),
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error occurred");
            }
        }
        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ActionName("GetKeys")]
        public HttpResponseMessage GetKeys()
        {
            try
            {
                var retval = new CurrentKeysModel();
                retval.Keys = Common.GetCurrentCachedKeys();
                retval.Keys = retval.Keys.OrderBy(model => model.Key).ToList();
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error occurred");
            }
        }


        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        [ActionName("GetKey")]
        public HttpResponseMessage GetKey(string key)
        {
            try
            {
                var retval = new CurrentKeyModel();
                var cp = CachingProvider.Instance();
                IDictionaryEnumerator cacheEnum = cp.GetEnumerator();
                var o = cp.GetItem(key);
                retval.Key = key;
                retval.Type = o?.GetType().FullName;
                try
                {
                    retval.Contents = JsonConvert.SerializeObject(o, Formatting.Indented);
                }
                catch (Exception e)
                {
                    retval.Contents = "Can't show contents of this thing";
                }
                return Request.CreateResponse(System.Net.HttpStatusCode.OK, retval);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error occurred");
            }
        }
    }
}