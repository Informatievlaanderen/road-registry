namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Modules;

using Autofac;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoadRegistry.Hosts;
using RoadRegistry.Syndication.Schema;

public class SyndicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context =>
        {
            var configuration = context.Resolve<IConfiguration>();

            return new TraceDbConnection<SyndicationContext>(
                new SqlConnection(configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections)), configuration["DataDog:ServiceName"]);
        });
        
        builder.Register<Func<SyndicationContext>>(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var loggerFactory = context.Resolve<ILoggerFactory>();

                return () =>
                    new SyndicationContext(
                        new DbContextOptionsBuilder<SyndicationContext>()
                            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(
                                configuration.GetConnectionString(WellknownConnectionNames.SyndicationProjections),
                                options => options
                                    .EnableRetryOnFailure()
                            )
                            .Options);
            }
        ).SingleInstance();
    }
}
