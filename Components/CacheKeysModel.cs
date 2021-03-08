using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace FortyFingers.CacheManager.Components
{
    public class CacheKeysModel
    {
        public List<CacheKeyModel> Keys { get; set; } = new List<CacheKeyModel>();

        public bool TaskEnabled { get; set; }
    }

    public class CacheKeyModel
    {
        public string Key { get; set; }
        public string Query { get; set; }
    }

    public class CurrentKeysModel
    {
        public List<CurrentKeyModel> Keys { get; set; } = new List<CurrentKeyModel>();
    }

    public class CurrentKeyModel
    {
        public string Key { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Contents { get; set; }
    }
}