using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Scheduling;

namespace FortyFingers.CacheManager.Components
{
    public static class SchedulerHelper
    {
        public static ScheduleItem GetScheduleItem(string typeName, string assembly)
        {
            var fullTypeName = typeName + ", " + assembly;
            return GetScheduleItem(fullTypeName);
        }
        public static ScheduleItem GetScheduleItem(string fullTypeName)
        {
            ScheduleItem retval = null;

            var provider = SchedulingProvider.Instance();

            var list = new List<ScheduleItem>();
            list.AddRange(provider.GetSchedule().OfType<ScheduleItem>());

            if (list.Any(s => s.TypeFullName == fullTypeName))
                retval = list.First(s => s.TypeFullName == fullTypeName);

            return retval;
        }

        public static bool IsScheduleItemEnabled(string typeName, string assembly)
        {
            var item = GetScheduleItem(typeName, assembly);
            return item != null && item.Enabled;
        }
        public static ScheduleItem EnableScheduleItem(string typeName, string assembly)
        {
            var fullTypeName = typeName + ", " + assembly;
            var provider = SchedulingProvider.Instance();
            var item = GetScheduleItem(fullTypeName);

            if (item == null)
            {
                item = new ScheduleItem();
                item.CatchUpEnabled = false;
                item.FriendlyName = typeName;
                item.RetainHistoryNum = 50;
                item.RetryTimeLapse = -1;
                item.RetryTimeLapseMeasurement = "s";
                item.TimeLapse = 5;
                item.TimeLapseMeasurement = "m";
                item.TypeFullName = fullTypeName;

                item.ScheduleID = provider.AddSchedule(item);
            }

            item.Enabled = true;
            provider.UpdateSchedule(item);

            return item;
        }

        public static void UpdateScheduleItem(ScheduleItem item)
        {
            var provider = SchedulingProvider.Instance();
            provider.UpdateSchedule(item);
        }

        public static void DisableScheduleItem(string typeName, string assembly)
        {
            var fullTypeName = typeName + ", " + assembly;
            DisableScheduleItem(fullTypeName);
        }
        public static void DisableScheduleItem(string fullTypeName)
        {
            var provider = SchedulingProvider.Instance();
            var item = GetScheduleItem(fullTypeName);

            if (item != null)
            {
                item.Enabled = false;
                provider.UpdateSchedule(item);
            }
        }
    }
}