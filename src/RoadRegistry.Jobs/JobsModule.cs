namespace RoadRegistry.Jobs
{
    using BackOffice;
    using BackOffice.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    public static class JobsModule
    {
        public static IServiceCollection AddJobsContext(this IServiceCollection services)
        {
            return services
                .AddTraceDbConnection<JobsContext>(WellKnownConnectionNames.Jobs)
                .AddDbContext<JobsContext>(JobsContext.ConfigureOptions)
                .AddDbContextFactory<JobsContext>(JobsContext.ConfigureOptions)
                ;
        }
    }
}
