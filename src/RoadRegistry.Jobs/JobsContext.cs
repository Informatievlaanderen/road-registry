namespace RoadRegistry.Jobs
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class JobsContext : DbContext
    {
        public const string Schema = "RoadRegistryJobs";
        public const string MigrationsTableName = "__EFMigrationsHistoryRoadRegistryJobs";

        public DbSet<Job> Jobs => Set<Job>();

        public JobsContext()
        {
        }

        public JobsContext(DbContextOptions<JobsContext> options)
            : base(options)
        {
        }

        public async Task<Job?> FindJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await Jobs.FindAsync(new object[] { jobId }, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsContext).Assembly);
        }
        
        public static void ConfigureOptions(IServiceProvider sp, DbContextOptionsBuilder options)
        {
            options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseSqlServer(
                    sp.GetRequiredService<TraceDbConnection<JobsContext>>(),
                    sqlOptions => sqlOptions
                        .EnableRetryOnFailure()
                        .MigrationsHistoryTable(MigrationsTableName, Schema));
        }
    }

    public class JobsContextMigratorFactory : DbContextMigratorFactory<JobsContext>
    {
        public JobsContextMigratorFactory()
            : base(WellKnownConnectionNames.JobsAdmin, new MigrationHistoryConfiguration
            {
                Schema = JobsContext.Schema,
                Table = JobsContext.MigrationsTableName
            })
        {
        }

        protected override JobsContext CreateContext(DbContextOptions<JobsContext> migrationContextOptions)
        {
            return new JobsContext(migrationContextOptions);
        }
    }
}
