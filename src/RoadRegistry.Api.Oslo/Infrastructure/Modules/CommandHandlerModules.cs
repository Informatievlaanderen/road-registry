namespace RoadRegistry.Api.Oslo.Infrastructure.Modules
{
    using System;
    using Aiv.Vbr.CommandHandling.SqlStreamStore.Autofac;
    using Autofac;
    using RoadRegistry.Road;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            // TODO: Kunnen we dit via discovery doen? Zodat ge niet per commandhandlermodule zoiets moet typen?
            // TODO: Zou dit niet in domein assembly beter zitten? bij de commandhandlermodules zelf
            containerBuilder
                .RegisterSqlStreamStoreCommandHandler<RoadCommandHandlerModule>(
                    c => handler => new RoadCommandHandlerModule(c.Resolve<Func<IRoads>>(), handler));
        }
    }
}
