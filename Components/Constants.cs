using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FortyFingers.CacheManager.Components
{
    public static class Constants
    {
        public static string PsLastCacheCleared(int portalId) => $"40F_LastCacheCleared_{portalId}";
        public static string PsCacheKeysToClear(int portalId) => $"40F_CacheKeysToClear_{portalId}";
    }
}