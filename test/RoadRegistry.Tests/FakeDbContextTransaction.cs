namespace RoadRegistry.Tests;

using Microsoft.EntityFrameworkCore.Storage;

public class FakeDbContextTransaction : IDbContextTransaction
{
    public TransactionStatus Status { get; private set; } = TransactionStatus.Started;

    public enum TransactionStatus
    {
        Started,
        Committed,
        Rolledback
    }

    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Commit()
    {
        Status = TransactionStatus.Committed;
    }

    public Task CommitAsync(CancellationToken cancellationToken = new())
    {
        Status = TransactionStatus.Committed;
        return Task.CompletedTask;
    }

    public void Rollback()
    {
        Status = TransactionStatus.Rolledback;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = new())
    {
        Status = TransactionStatus.Rolledback;
        return Task.CompletedTask;
    }

    public Guid TransactionId { get; }
}
