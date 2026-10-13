namespace RoadRegistry.Tests;

using System.Collections;
using System.Linq.Expressions;
using Marten.Linq;
using Marten.Linq.Includes;

// The IMartenQueryable<T> that InMemoryDocumentStoreSession.Query<T>() hands out: a plain Linq-to-Objects queryable
// wearing Marten's interface. Everything that only needs IQueryable - Where, OrderBy, Select, enumeration - works and
// is evaluated in memory; Marten's server-side extras (Include, WhereSub) have no in-memory equivalent and say so.
//
// Note that Provider is the inner Linq-to-Objects provider, so the first operator applied returns an ordinary
// EnumerableQuery. That is deliberate: it is how SessionExtensions.ToReadOnlyListAsync recognizes a query that
// cannot go through Marten's async pipeline.
public sealed class InMemoryMartenQueryable<T> : IMartenQueryable<T>
    where T : notnull
{
    private readonly IQueryable<T> _queryable;

    public InMemoryMartenQueryable(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    public Type ElementType => _queryable.ElementType;
    public Expression Expression => _queryable.Expression;
    public IQueryProvider Provider => _queryable.Provider;

    public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback, Expression<Func<TInclude, bool>> filter) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list, Expression<Func<TInclude, bool>> filter) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource, IDictionary<TKey, TInclude> dictionary) where TInclude : notnull where TKey : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource, IDictionary<TKey, TInclude> dictionary, Expression<Func<TInclude, bool>> filter) where TInclude : notnull where TKey : notnull => throw new NotSupportedException();
    public IMartenQueryableIncludeBuilder<T, TInclude> Include<TInclude>(Action<TInclude> callback) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryableIncludeBuilder<T, TInclude> Include<TInclude>(IList<TInclude> list) where TInclude : notnull => throw new NotSupportedException();
    public IMartenQueryableIncludeBuilder<T, TKey, TInclude> Include<TKey, TInclude>(IDictionary<TKey, TInclude> dictionary) where TInclude : notnull where TKey : notnull => throw new NotSupportedException();
    public IMartenQueryableIncludeBuilder<T, TKey, TInclude> Include<TKey, TInclude>(IDictionary<TKey, IList<TInclude>> dictionary) where TInclude : notnull where TKey : notnull => throw new NotSupportedException();
    public IMartenQueryableIncludeBuilder<T, TKey, TInclude> Include<TKey, TInclude>(IDictionary<TKey, List<TInclude>> dictionary) where TInclude : notnull where TKey : notnull => throw new NotSupportedException();
    public IMartenQueryable<T> WhereSub<TSub>(Expression<Func<TSub, bool>> predicate) where TSub : T => throw new NotSupportedException();
}
