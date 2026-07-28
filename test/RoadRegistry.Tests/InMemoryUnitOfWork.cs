namespace RoadRegistry.Tests;

using JasperFx.Events;
using Marten.Internal.Operations;
using Marten.Services;

// Minimal IUnitOfWork used by InMemoryDocumentStoreSession. Only Streams() is implemented - it surfaces the pending
// (not-yet-saved) event stream actions so DocumentStoreExtensions.IdempotentSession can stamp change-ordinal headers
// before SaveChanges, exactly like production Marten.
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly InMemoryEventStoreOperations _eventStoreOperations;

    public InMemoryUnitOfWork(InMemoryEventStoreOperations eventStoreOperations)
    {
        _eventStoreOperations = eventStoreOperations;
    }

    public IList<StreamAction> Streams() => _eventStoreOperations.PendingStreamActions;

    public IEnumerable<IDeletion> Deletions() => throw new NotImplementedException();
    public IEnumerable<IDeletion> DeletionsFor<T>() => throw new NotImplementedException();
    public IEnumerable<IDeletion> DeletionsFor(Type documentType) => throw new NotImplementedException();
    public IEnumerable<object> Updates() => throw new NotImplementedException();
    public IEnumerable<object> Inserts() => throw new NotImplementedException();
    public IEnumerable<T> UpdatesFor<T>() => throw new NotImplementedException();
    public IEnumerable<T> InsertsFor<T>() => throw new NotImplementedException();
    public IEnumerable<T> AllChangedFor<T>() => throw new NotImplementedException();
    public IEnumerable<IStorageOperation> Operations() => throw new NotImplementedException();
    public IEnumerable<IStorageOperation> OperationsFor<T>() => throw new NotImplementedException();
    public IEnumerable<IStorageOperation> OperationsFor(Type documentType) => throw new NotImplementedException();
}
