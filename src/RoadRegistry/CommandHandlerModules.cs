namespace RoadRegistry
{
    using System;
    using Aiv.Vbr.CommandHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Road;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterSqlStreamStoreCommandHandler<RoadCommandHandlerModule>(
                    c => handler =>
                        new RoadCommandHandlerModule(
                            c.Resolve<Func<IRoads>>(),
                            handler));
        }
    }
}
