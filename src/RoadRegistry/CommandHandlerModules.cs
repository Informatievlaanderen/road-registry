namespace RoadRegistry
{
    using Autofac;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
//            containerBuilder
//                .RegisterSqlStreamStoreCommandHandler<RoadCommandHandlerModule>(
//                    c => handler =>
//                        new RoadCommandHandlerModule(
//                            c.Resolve<Func<IRoads>>(),
//                            handler));
        }
    }
}
