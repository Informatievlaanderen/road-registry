namespace RoadRegistry.BackOffice.ZipArchiveWriters.Extensions;

using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;

public static class DbSetExtensions
{
    public static async Task<IReadOnlyList<T>> ToListWithPolygonials<T, TKey>(this DbSet<T> dbSet, IPolygonal contour, Func<DbSet<T>, IPolygonal, IQueryable<T>> insideContour, Func<T, TKey> keySelector, CancellationToken cancellationToken)
        where T : class
    {
        var polygonials = contour.GetPolygonals();
        var items = new List<T>();
        foreach (var polygon in polygonials)
        {
            var subRange = insideContour(dbSet, polygon);
            items.AddRange(await subRange.ToListAsync(cancellationToken));
        }

        return items.DistinctBy(keySelector).ToList();
    }
}
