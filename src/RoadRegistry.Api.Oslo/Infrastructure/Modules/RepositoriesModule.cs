namespace RoadRegistry.Api.Oslo.Infrastructure.Modules
{
    using Autofac;
    using RoadRegistry.Road;
    using Road;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            containerBuilder
                .RegisterType<Roads>()
                .As<IRoads>();
        }
    }
}
