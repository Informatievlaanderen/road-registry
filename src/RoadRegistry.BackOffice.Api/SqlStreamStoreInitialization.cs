namespace RoadRegistry.Api
{
    using System;
    using System.Threading.Tasks;
    using AspNetCore.AsyncInitialization;
    using SqlStreamStore;

    public class SqlStreamStoreInitialization : IAsyncInitializer
    {
        private readonly IStreamStore _store;

        public SqlStreamStoreInitialization(IStreamStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Task InitializeAsync()
        {
            if (_store is MsSqlStreamStore store)
            {
                return store.CreateSchema();
            }
            return Task.CompletedTask;
        }
    }
}