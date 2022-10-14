using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.ZipArchiveWriters.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;

    public static class DbSetExtensions
    {
        public static async Task<List<T>> ToListWithPolygonials<T, TKey>(this DbSet<T> dbSet, IPolygonal contour, Func<DbSet<T>, IPolygonal, IQueryable<T>> insideContour, Func<T, TKey> keySelector, CancellationToken cancellationToken)
            where T : class
        {
            var polygonials = contour.GetPolygonals();
            var items = new List<T>();
            foreach (var polygon in polygonials)
            {
                items.AddRange(await insideContour(dbSet, polygon).ToListAsync(cancellationToken));
            }
            return items.DistinctBy(keySelector).ToList();
        }
    }
}
