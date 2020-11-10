namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Schema.RoadSegments;

    public static class RoadSegmentLaneAttributeRecordDbSetExtensions
    {
        public static async Task Synchronize(this DbSet<RoadSegmentLaneAttributeRecord> source,
            Dictionary<int, RoadSegmentLaneAttributeRecord> currentSet,
            Dictionary<int, RoadSegmentLaneAttributeRecord> nextSet,
            Action<RoadSegmentLaneAttributeRecord, RoadSegmentLaneAttributeRecord> action)
        {
            var newRecordIds = nextSet.Keys.Where(key => !currentSet.ContainsKey(key));
            var removedRecordIds = currentSet.Keys.Where(key => !nextSet.ContainsKey(key));
            var modifiedRecordIds = nextSet.Keys.Where(key => currentSet.ContainsKey(key));

            foreach (var removedRecordId in removedRecordIds)
            {
                var toRemove = currentSet[removedRecordId];
                source.Remove(toRemove);
                source.Local.Remove(toRemove);
            }

            foreach (var newRecordId in newRecordIds)
            {
                await source.AddAsync(nextSet[newRecordId]);
            }

            modifiedRecordIds
                .ToList()
                .ForEach(x => action(currentSet[x], nextSet[x]));
        }
    }
}
