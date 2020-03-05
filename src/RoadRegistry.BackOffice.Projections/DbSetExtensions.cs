// namespace RoadRegistry.BackOffice.Projections
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using Microsoft.EntityFrameworkCore;
//
//     internal static class DbSetExtensions
//     {
//         public static void Synchronize<TIdentity, TEntity>(this DbSet<TEntity> allSet,
//             IReadOnlyDictionary<TIdentity, TEntity> current,
//             IReadOnlyDictionary<TIdentity, TEntity> next,
//             Action<TEntity, TEntity> patch) where TEntity : class
//         {
//             foreach (var key in next.Select(a => a.Key).Concat(current.Select(a => a.Key)))
//             {
//                 if (current.ContainsKey(key) && next.ContainsKey(key))
//                 {
//                     // modify
//                     patch(current[key], next[key]);
//                 } else if (next.ContainsKey(key))
//                 {
//                     // add
//                     allSet.Add(next[key]);
//                 } else if (current.ContainsKey(key))
//                 {
//                     // remove
//                     allSet.Remove(current[key]);
//                 }
//             }
//         }
//     }
// }
