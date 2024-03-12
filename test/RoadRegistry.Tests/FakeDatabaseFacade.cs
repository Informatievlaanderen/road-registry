namespace RoadRegistry.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

public class FakeDatabaseFacade : DatabaseFacade
{
    private IDbContextTransaction _currentTransaction;

    public FakeDatabaseFacade(DbContext context) : base(context)
    { }

    public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = new())
    {
        IDbContextTransaction transaction = new FakeDbContextTransaction();
        _currentTransaction = transaction;

        return Task.FromResult(transaction);
    }

    public override IDbContextTransaction? CurrentTransaction => _currentTransaction;
}
