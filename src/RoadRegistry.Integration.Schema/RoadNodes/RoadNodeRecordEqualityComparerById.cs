// namespace RoadRegistry.Integration.Schema.RoadNodes;
//
// using System.Collections.Generic;
//
// public class RoadNodeRecordEqualityComparerById : IEqualityComparer<RoadNodeRecord>
// {
//     public bool Equals(RoadNodeRecord x, RoadNodeRecord y)
//     {
//         if (ReferenceEquals(x, y)) return true;
//         if (ReferenceEquals(x, null)) return false;
//         if (ReferenceEquals(y, null)) return false;
//         if (x.GetType() != y.GetType()) return false;
//         return x.Id == y.Id;
//     }
//
//     public int GetHashCode(RoadNodeRecord obj)
//     {
//         return obj.Id;
//     }
// }
