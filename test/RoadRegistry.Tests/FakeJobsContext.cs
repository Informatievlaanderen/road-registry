namespace RoadRegistry.Tests;

using Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

public sealed class FakeJobsContext : JobsContext
{
    private readonly bool _canBeDisposed;
    public FakeDatabaseFacade FakeDatabase;

    public FakeJobsContext(DbContextOptions<JobsContext> options, bool canBeDisposed)
        : base(options)
    {
        _canBeDisposed = canBeDisposed;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        base.OnConfiguring(optionsBuilder);
    }

    public override DatabaseFacade Database => FakeDatabase ??= new FakeDatabaseFacade(this);

    public override void Dispose()
    {
        if (_canBeDisposed)
        {
            base.Dispose();
        }
    }

    public override ValueTask DisposeAsync()
    {
        if (_canBeDisposed)
            return base.DisposeAsync();

        return ValueTask.CompletedTask;
    }
}
