namespace RoadRegistry.BackOffice.EventHost
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SqlStreamStore;

    public static class StreamStoreExtensions
    {
        public static async Task WaitUntilAvailable(this IStreamStore store, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (store is MsSqlStreamStore)
            {
                var exit = false;
                while(!exit)
                {
                    try
                    {
                        await store.ReadHeadPosition(cancellationToken);
                        exit = true;

                    }
                    catch (Exception)
                    {
                        // swallowed
                    }
                }
            }
        }
    }
}
