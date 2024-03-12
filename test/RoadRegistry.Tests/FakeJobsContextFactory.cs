namespace RoadRegistry.Tests;

using Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

public class FakeJobsContextFactory : IDesignTimeDbContextFactory<FakeJobsContext>
{
    private readonly bool _canBeDisposed;
    private readonly string _databaseName;

    public FakeJobsContextFactory(
        string databaseName = null,
        bool canBeDisposed = true)
    {
        _canBeDisposed = canBeDisposed;
        _databaseName = databaseName ?? Guid.NewGuid().ToString();
    }

    public FakeJobsContext CreateDbContext(params string[] args)
    {
        var builder = new DbContextOptionsBuilder<JobsContext>().UseInMemoryDatabase(_databaseName);
        return new FakeJobsContext(builder.Options, _canBeDisposed);
    }
}
