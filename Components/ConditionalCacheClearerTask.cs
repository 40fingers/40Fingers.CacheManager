using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Services.Exceptions;

namespace FortyFingers.CacheManager.Components
{
    public class ConditionalCacheClearerTask : SchedulerClient
    {
        public ConditionalCacheClearerTask(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                var cntCheck = 0;
                var cntClear = 0;
                foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                {
                    var sLastCleared = PortalController.GetPortalSetting(Constants.PsLastCacheCleared(portal.PortalID), portal.PortalID, "");
                    if (sLastCleared == "")
                    {
                        // no cachemanager here
                        continue;
                    }

                    var sKeysToClear = PortalController.GetPortalSetting(Constants.PsCacheKeysToClear(portal.PortalID), portal.PortalID, "");
                    var keysModel = Json.Deserialize<List<CacheKeyModel>>(sKeysToClear);
                    if (keysModel != null)
                    {
                        // change lastCleared to prevent double clearings
                        PortalController.UpdatePortalSetting(portal.PortalID,
                            Constants.PsLastCacheCleared(portal.PortalID),
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        foreach (CacheKeyModel keyModel in keysModel)
                        {
                            // we make the query work regardless of the LASTCLEARED token being quoted
                            var sql = keyModel.Query.Replace("'[LASTCLEARED]'", "[LASTCLEARED]").Replace("[LASTCLEARED]", $"'{sLastCleared}'");
                            if (DataContext.Instance().ExecuteScalar<bool>(CommandType.Text, sql))
                            {
                                DataCache.RemoveCache(keyModel.Key);
                                cntClear++;
                            }

                            cntCheck++;
                            Progressing();
                        }
                    }

                    Progressing();

                    ScheduleHistoryItem.AddLogNote($"{cntCheck} Cache items checked, cleared {cntClear}");
                    ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception e)
            {
                Exceptions.ProcessSchedulerException(e);
                ScheduleHistoryItem.Succeeded = false;
                Errored(ref e);
            }
        }
    }
}