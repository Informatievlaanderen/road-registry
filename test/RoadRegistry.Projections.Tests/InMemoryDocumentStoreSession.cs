namespace RoadRegistry.Projections.Tests;

using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Transactions;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using Marten;
using Marten.Events;
using Marten.Internal.Operations;
using Marten.Linq;
using Marten.Schema;
using Marten.Services;
using Marten.Services.BatchQuerying;
using Marten.Storage;
using Marten.Storage.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql;
using IsolationLevel = System.Data.IsolationLevel;

public class InMemoryDocumentStoreSession : IDocumentStore, IDocumentSession
{
    private readonly StoreOptions _storeOptions;
    private readonly Dictionary<(object Id, Type Type), string> _storedEntities = [];
    private readonly List<StreamAction> _streamActions = new();

    public InMemoryDocumentStoreSession(StoreOptions options)
    {
        _storeOptions = options;
        _eventsStoreOperations = new InMemoryEventStoreOperations();
    }

    public object[] AllRecords()
    {
        var serializer = _storeOptions.Serializer();
        return _storedEntities.Select(x =>
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(x.Value));
            return serializer.FromJson(x.Key.Type, jsonStream);
        }).ToArray();
    }

    public object[] AllEvents()
    {
        return _streamActions
            .SelectMany(x => x.Events)
            .Select(x => x.Data)
            .ToArray();
    }

    private void StoreEntities<T>(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            var type = entity.GetType();
            var id = GetIdProperty(entity).GetValue(entity)
                     ?? throw new InvalidOperationException($"Entity of type '{type}' has a null ID.");

            _storedEntities[(id, type)] = _storeOptions.Serializer().ToJson(entity);
        }
    }

    private PropertyInfo GetIdProperty(object entity)
    {
        var type = entity.GetType();
        var mapping = new DocumentMapping(type, _storeOptions);
        return type.GetProperty(mapping.IdMember.Name)
                 ?? throw new InvalidOperationException($"No ID mapping found for entity of type '{type}'.");
    }

    private async Task<T?> LoadByIdAsync<T>(object id) where T : notnull
    {
        var result = await LoadManyByIdAsync<T, object>([id]);
        return result.SingleOrDefault();
    }
    private T? LoadById<T>(object id) where T : notnull
    {
        var result = LoadManyById<T, object>([id]);
        return result.SingleOrDefault();
    }
    private async Task<IReadOnlyList<T>> LoadManyByIdAsync<T, TId>(IEnumerable<TId> ids) where T : notnull
    {
        return LoadManyById<T, TId>(ids);
    }
    private IReadOnlyList<T> LoadManyById<T, TId>(IEnumerable<TId> ids) where T : notnull
    {
        var serializer = _storeOptions.Serializer();
        var entities = _storedEntities
            .Where(x => ids.Cast<object>().Contains(x.Key.Id))
            .Select(x =>
            {
                using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(x.Value));
                return (T)serializer.FromJson(x.Key.Type, jsonStream);
            })
            .ToArray();

        return entities;
    }

    private void DeleteById<T>(object id) where T : notnull
    {
        if(!_storedEntities.ContainsKey((id, typeof(T))))
        {
            throw new InvalidOperationException($"No entity of type '{typeof(T).Name}' with Id '{id}' found to delete.");
        }

        _storedEntities.Remove((id, typeof(T)));
    }

    public void Delete<T>(T entity) where T : notnull
    {
        var id = GetIdProperty(entity).GetValue(entity)
                 ?? throw new InvalidOperationException($"Entity of type '{typeof(T)}' has a null ID.");

        DeleteById<T>(id);
    }

    public IDocumentSession LightweightSession(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return this;
    }

    public Task<T?> LoadAsync<T>(string id, CancellationToken token = new CancellationToken()) where T : notnull
    {
        return LoadByIdAsync<T>(id);
    }

    public Task<T?> LoadAsync<T>(object id, CancellationToken token = new CancellationToken()) where T : notnull
    {
        return LoadByIdAsync<T>(id);
    }

    public Task<T?> LoadAsync<T>(int id, CancellationToken token = new CancellationToken()) where T : notnull
    {
        return LoadByIdAsync<T>(id);
    }

    public Task<T?> LoadAsync<T>(long id, CancellationToken token = new CancellationToken()) where T : notnull
    {
        return LoadByIdAsync<T>(id);
    }

    public Task<T?> LoadAsync<T>(Guid id, CancellationToken token = new CancellationToken()) where T : notnull
    {
        return LoadByIdAsync<T>(id);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params string[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, string>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<string> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, string>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params Guid[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, Guid>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<Guid> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, Guid>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params int[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, int>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<int> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, int>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(params long[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, long>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(IEnumerable<long> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, long>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params string[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, string>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<string> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, string>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params Guid[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, Guid>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<Guid> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, Guid>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params int[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, int>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<int> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, int>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, params long[] ids) where T : notnull
    {
        return LoadManyByIdAsync<T, long>(ids);
    }

    public Task<IReadOnlyList<T>> LoadManyAsync<T>(CancellationToken token, IEnumerable<long> ids) where T : notnull
    {
        return LoadManyByIdAsync<T, long>(ids);
    }

    public void SetHeader(string key, object value)
    {
    }

    public void Delete<T>(int id) where T : notnull
    {
        DeleteById<T>(id);
    }

    public void Delete<T>(long id) where T : notnull
    {
        DeleteById<T>(id);
    }

    public void Delete<T>(object id) where T : notnull
    {
        DeleteById<T>(id);
    }

    public void Delete<T>(Guid id) where T : notnull
    {
        DeleteById<T>(id);
    }

    public void Delete<T>(string id) where T : notnull
    {
        DeleteById<T>(id);
    }

    public void Insert<T>(IEnumerable<T> entities) where T : notnull
    {
        Insert(entities.ToArray());
    }

    public void Insert<T>(params T[] entities) where T : notnull
    {
        foreach (var entity in entities)
        {
            var type = entity.GetType();
            var id = GetIdProperty(entity).GetValue(entity)
                     ?? throw new InvalidOperationException($"Entity of type '{type}' has a null ID.");

            var existingItem = LoadById<T>(id);
            if (existingItem is not null)
            {
                throw new InvalidOperationException($"Entity of type '{type}' with ID '{id}' already exists.");
            }

            StoreEntities([entity]);
        }
    }

    public Task SaveChangesAsync(CancellationToken token = new CancellationToken())
    {
        _streamActions.AddRange(_eventsStoreOperations.GetAndClearStreamActions());
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #region NotImplemented

    public Task BulkInsertEnlistTransactionAsync<T>(IReadOnlyCollection<T> documents, Transaction transaction, BulkInsertMode mode = BulkInsertMode.InsertsOnly, int batchSize = 1000, CancellationToken cancellation = new CancellationToken()) where T : notnull
    {
        throw new NotImplementedException();
    }

    public Task BulkInsertAsync<T>(string tenantId, IReadOnlyCollection<T> documents, BulkInsertMode mode = BulkInsertMode.InsertsOnly, int batchSize = 1000, CancellationToken cancellation = new CancellationToken()) where T : notnull
    {
        throw new NotImplementedException();
    }

    public Task BulkInsertAsync<T>(IReadOnlyCollection<T> documents, BulkInsertMode mode = BulkInsertMode.InsertsOnly, int batchSize = 1000, CancellationToken cancellation = new CancellationToken()) where T : notnull
    {
        throw new NotImplementedException();
    }

    public IDocumentSession OpenSession(SessionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> OpenSerializableSessionAsync(SessionOptions options, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IDocumentSession LightweightSession(string tenantId, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public IDocumentSession LightweightSession(SessionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> LightweightSerializableSessionAsync(CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> LightweightSerializableSessionAsync(string tenantId, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> LightweightSerializableSessionAsync(SessionOptions options, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IDocumentSession IdentitySession(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public IDocumentSession IdentitySession(string tenantId, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public IDocumentSession IdentitySession(SessionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> IdentitySerializableSessionAsync(CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> IdentitySerializableSessionAsync(string tenantId, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> IdentitySerializableSessionAsync(SessionOptions options, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IDocumentSession DirtyTrackedSession(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public IDocumentSession DirtyTrackedSession(string tenantId, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        throw new NotImplementedException();
    }

    public IDocumentSession DirtyTrackedSession(SessionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> DirtyTrackedSerializableSessionAsync(CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> DirtyTrackedSerializableSessionAsync(string tenantId, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IDocumentSession> DirtyTrackedSerializableSessionAsync(SessionOptions options, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IQuerySession QuerySession()
    {
        throw new NotImplementedException();
    }

    public IQuerySession QuerySession(string tenantId)
    {
        throw new NotImplementedException();
    }

    public IQuerySession QuerySession(SessionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task<IQuerySession> QuerySerializableSessionAsync(CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IQuerySession> QuerySerializableSessionAsync(string tenantId, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IQuerySession> QuerySerializableSessionAsync(SessionOptions options, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task BulkInsertDocumentsAsync(IEnumerable<object> documents, BulkInsertMode mode = BulkInsertMode.InsertsOnly, int batchSize = 1000, CancellationToken cancellation = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task BulkInsertDocumentsAsync(string tenantId, IEnumerable<object> documents, BulkInsertMode mode = BulkInsertMode.InsertsOnly, int batchSize = 1000, CancellationToken cancellation = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public ValueTask<IProjectionDaemon> BuildProjectionDaemonAsync(string? tenantIdOrDatabaseIdentifier = null, ILogger? logger = null)
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Query<T>() where T : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> QueryForNonStaleData<T>(TimeSpan timeout) where T : notnull
    {
        throw new NotImplementedException();
    }

    public Task<int> StreamJson<T>(Stream destination, CancellationToken token, string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<int> StreamJson<T>(Stream destination, CancellationToken token, char placeholder, string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<int> StreamJson<T>(Stream destination, string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<int> StreamJson<T>(Stream destination, char placeholder, string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, CancellationToken token, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(char placeholder, string sql, CancellationToken token, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(char placeholder, string sql, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public IBatchedQuery CreateBatchQuery()
    {
        throw new NotImplementedException();
    }

    public Task<TOut> QueryAsync<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = new CancellationToken()) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Task<bool> StreamJsonOne<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, Stream destination, CancellationToken token = new CancellationToken()) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Task<int> StreamJsonMany<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, Stream destination, CancellationToken token = new CancellationToken()) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Task<string?> ToJsonOne<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = new CancellationToken()) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Task<string> ToJsonMany<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CancellationToken token = new CancellationToken()) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Guid? VersionFor<TDoc>(TDoc entity) where TDoc : notnull
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TDoc>> SearchAsync<TDoc>(string queryText, string regConfig = "english", CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TDoc>> PlainTextSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TDoc>> PhraseSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TDoc>> WebStyleSearchAsync<TDoc>(string searchTerm, string regConfig = "english", CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<DocumentMetadata> MetadataForAsync<T>(T entity, CancellationToken token = new CancellationToken()) where T : notnull
    {
        throw new NotImplementedException();
    }

    ITenantOperations IDocumentSession.ForTenant(string tenantId)
    {
        throw new NotImplementedException();
    }

    public void EjectAllPendingChanges()
    {
        throw new NotImplementedException();
    }

    public IUnitOfWork PendingChanges { get; }

    public void BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public ValueTask BeginTransactionAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Eject<T>(T document) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void EjectAllOfType(Type type)
    {
        throw new NotImplementedException();
    }

    public object? GetHeader(string key)
    {
        throw new NotImplementedException();
    }

    ITenantOperations IDocumentOperations.ForTenant(string tenantId)
    {
        throw new NotImplementedException();
    }

    public void DeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void DeleteObjects(IEnumerable<object> documents)
    {
        throw new NotImplementedException();
    }

    void IDocumentOperations.Store<T>(IEnumerable<T> entities)
    {
        StoreEntities(entities);
    }

    void IDocumentOperations.Store<T>(params T[] entities)
    {
        StoreEntities(entities);
    }

    public void UpdateExpectedVersion<T>(T entity, Guid version) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void UpdateRevision<T>(T entity, int revision) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void TryUpdateRevision<T>(T entity, int revision) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void StoreObjects(IEnumerable<object> documents)
    {
        throw new NotImplementedException();
    }

    public void QueueOperation(IStorageOperation storageOperation)
    {
        throw new NotImplementedException();
    }

    public void Update<T>(IEnumerable<T> entities) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void Update<T>(params T[] entities) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void InsertObjects(IEnumerable<object> documents)
    {
        throw new NotImplementedException();
    }

    public void HardDelete<T>(T entity) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void HardDelete<T>(int id) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void HardDelete<T>(long id) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void HardDelete<T>(Guid id) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void HardDelete<T>(string id) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void HardDeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void UndoDeleteWhere<T>(Expression<Func<T, bool>> expression) where T : notnull
    {
        throw new NotImplementedException();
    }

    public void QueueSqlCommand(string sql, params object[] parameterValues)
    {
        throw new NotImplementedException();
    }

    public void QueueSqlCommand(char placeholder, string sql, params object[] parameterValues)
    {
        throw new NotImplementedException();
    }

    public void UseIdentityMapFor<T>() where T : notnull
    {
        throw new NotImplementedException();
    }

    ITenantQueryOperations IQuerySession.ForTenant(string tenantId)
    {
        throw new NotImplementedException();
    }

    public Task<int> ExecuteAsync(NpgsqlCommand command, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<T> QueryByPlanAsync<T>(IQueryPlan<T> plan, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IProjectionStorage<TDoc, TId>> FetchProjectionStorageAsync<TDoc, TId>(string tenantId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IProjectionStorage<TDoc, TId>> FetchProjectionStorageAsync<TDoc, TId>(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IMessageSink> GetOrStartMessageSink()
    {
        throw new NotImplementedException();
    }

    private IQueryEventStore _events;
    private InMemoryEventStoreOperations _eventsStoreOperations;
    IEventStoreOperations IDocumentSession.Events => _eventsStoreOperations;
    public ConcurrencyChecks Concurrency { get; }
    public IList<IDocumentSessionListener> Listeners { get; }
    public string? LastModifiedBy { get; set; }
    IEventStoreOperations IDocumentOperations.Events => _eventsStoreOperations;
    public IReadOnlyStoreOptions Options { get; }
    public IMartenStorage Storage { get; }
    public AdvancedOperations Advanced { get; }
    public IDiagnostics Diagnostics { get; }
    public bool EnableSideEffectsOnInlineProjections { get; }
    public IMartenDatabase Database { get; }
    public NpgsqlConnection Connection { get; }
    public IMartenSessionLogger Logger { get; set; }
    public int RequestCount { get; }
    public IDocumentStore DocumentStore { get; }
    IQueryEventStore IQuerySession.Events => _events;
    public IJsonLoader Json { get; }
    public string? CausationId { get; set; }
    public string? CorrelationId { get; set; }
    public string TenantId { get; }
    public IAdvancedSql AdvancedSql { get; }

    #endregion
}
