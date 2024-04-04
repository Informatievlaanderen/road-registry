namespace RoadRegistry.Jobs
{
    using Microsoft.Extensions.DependencyInjection;

    public static class JobsModule
    {
        public static IServiceCollection AddJobsContext(this IServiceCollection services)
        {
            return services
                .AddDbContext<JobsContext>(JobsContext.ConfigureOptions)
                .AddDbContextFactory<JobsContext>(JobsContext.ConfigureOptions)
                ;
        }
    }
}
