namespace RoadRegistry.Tests;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;

/// <summary>
/// Minimal <see cref="IAdvancedSql"/> stand-in for the in-memory document store. Only the single-result
/// <c>QueryAsync&lt;long&gt;</c> path is implemented, which is enough for the projection-progress queries used by
/// <c>WaitForNonStaleProjection</c> (high water mark + projection sequence id both resolve to a non-stale value).
/// </summary>
public sealed class FakeAdvancedSql : IAdvancedSql
{
    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken token, params object[] parameters)
    {
        if (typeof(T) == typeof(long))
        {
            return Task.FromResult((IReadOnlyList<T>)(object)new List<long> { 1L });
        }

        return Task.FromResult((IReadOnlyList<T>)new List<T>());
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => QueryAsync<T>(sql, token, parameters);

    public Task<IReadOnlyList<(T1, T2)>> QueryAsync<T1, T2>(string sql, CancellationToken token, params object[] parameters)
        => Task.FromResult((IReadOnlyList<(T1, T2)>)new List<(T1, T2)>());

    public Task<IReadOnlyList<(T1, T2)>> QueryAsync<T1, T2>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => Task.FromResult((IReadOnlyList<(T1, T2)>)new List<(T1, T2)>());

    public Task<IReadOnlyList<(T1, T2, T3)>> QueryAsync<T1, T2, T3>(string sql, CancellationToken token, params object[] parameters)
        => Task.FromResult((IReadOnlyList<(T1, T2, T3)>)new List<(T1, T2, T3)>());

    public Task<IReadOnlyList<(T1, T2, T3)>> QueryAsync<T1, T2, T3>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => Task.FromResult((IReadOnlyList<(T1, T2, T3)>)new List<(T1, T2, T3)>());

    public IAsyncEnumerable<T> StreamAsync<T>(string sql, CancellationToken token, params object[] parameters)
        => Empty<T>();

    public IAsyncEnumerable<T> StreamAsync<T>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => Empty<T>();

    public IAsyncEnumerable<(T1, T2)> StreamAsync<T1, T2>(string sql, CancellationToken token, params object[] parameters)
        => Empty<(T1, T2)>();

    public IAsyncEnumerable<(T1, T2)> StreamAsync<T1, T2>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => Empty<(T1, T2)>();

    public IAsyncEnumerable<(T1, T2, T3)> StreamAsync<T1, T2, T3>(string sql, CancellationToken token, params object[] parameters)
        => Empty<(T1, T2, T3)>();

    public IAsyncEnumerable<(T1, T2, T3)> StreamAsync<T1, T2, T3>(char placeholder, string sql, CancellationToken token, params object[] parameters)
        => Empty<(T1, T2, T3)>();

#pragma warning disable CS1998
    private static async IAsyncEnumerable<T> Empty<T>()
    {
        yield break;
    }
#pragma warning restore CS1998
}
