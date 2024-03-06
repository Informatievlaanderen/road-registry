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
        //public DbSet<JobRecord> JobRecords => Set<JobRecord>();

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

        //public async Task<JobRecord?> FindJobRecord(long jobRecordId, CancellationToken cancellationToken)
        //{
        //    return await JobRecords.FindAsync(new object[] { jobRecordId }, cancellationToken);
        //}

        //public IQueryable<JobRecord> GetJobRecordsArchive(Guid jobId)
        //{
        //    return JobRecords
        //        .FromSqlRaw($"select * from [{Schema}].[JobRecordsArchive] where {nameof(JobRecord.JobId)} = @jobId", new SqlParameter("@jobId", jobId))
        //        .AsNoTracking();
        //}

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


    //public class JobsContextFactory : IDesignTimeDbContextFactory<JobsContext>
    //{
    //    public JobsContext CreateDbContext(string[] args)
    //    {
    //        var migrationConnectionStringName = WellKnownConnectionNames.JobsAdmin;

    //        var configuration = new ConfigurationBuilder()
    //            .SetBasePath(Directory.GetCurrentDirectory())
    //            .AddJsonFile("appsettings.json")
    //            .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
    //            .AddEnvironmentVariables()
    //            .Build();

    //        var builder = new DbContextOptionsBuilder<JobsContext>();

    //        var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
    //        if (string.IsNullOrEmpty(connectionString))
    //            throw new InvalidOperationException(
    //                $"Could not find a connection string with name '{migrationConnectionStringName}'");

    //        builder
    //            .UseSqlServer(connectionString, sqlServerOptions =>
    //            {
    //                sqlServerOptions.EnableRetryOnFailure();
    //                sqlServerOptions.MigrationsHistoryTable(JobsContext.MigrationsTableName,
    //                    JobsContext.Schema);
    //                sqlServerOptions.UseNetTopologySuite();
    //            });

    //        return new JobsContext(builder.Options);
    //    }
    //}
}
