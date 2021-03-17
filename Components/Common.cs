using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Services.Cache;

namespace FortyFingers.CacheManager.Components
{
    public class Common
    {
        public static List<CurrentKeyModel> GetCurrentCachedKeys()
        {
            var retval = new List<CurrentKeyModel>();
            var cp = CachingProvider.Instance();
            IDictionaryEnumerator cacheEnum = cp.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                var o = cp.GetItem(cacheEnum.Key.ToString());
                retval.Add(new CurrentKeyModel() {Key = cacheEnum.Key.ToString(), Size = 0, Type = o.GetType().Name});
            }
            return retval;
        }

        public static List<string> GetMatchingCachedKeys(List<CurrentKeyModel> currentKeys, CacheKeyModel cacheKey)
        {
            var retval = new List<string>();

            if (cacheKey.IsRegex)
            {
                foreach (var currentKey in currentKeys)
                {
                    if(Regex.IsMatch(currentKey.Key, cacheKey.Key)) retval.Add(currentKey.Key);
                }
            }
            else
            {
                if(currentKeys.Any(o => o.Key == cacheKey.Key)) retval.Add(cacheKey.Key);
            }

            return retval;
        }
    }
}