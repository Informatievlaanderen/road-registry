namespace Microsoft.EntityFrameworkCore;

public static class EntityFrameworkExtensions
{
    public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>(
        this IQueryable<TEntity> source, bool ignoreQueryFilters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);

        if (ignoreQueryFilters)
        {
            return source.IgnoreQueryFilters();
        }

        return source;
    }
}
