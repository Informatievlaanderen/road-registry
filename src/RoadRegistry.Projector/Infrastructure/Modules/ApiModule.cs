namespace RoadRegistry.Projector.Infrastructure.Modules
{
    using System;
    using System.Collections.Generic;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;
    using Wfs.Schema;
    using Wms.Schema;
    using Module = Autofac.Module;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<ProjectionDetail, Func<DbContext>> _listOfProjections = new();
        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new DataDogModule(_configuration));
            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();
            _services.AddSingleton<IStreamStore>(sp =>
                new MsSqlStreamStoreV3(
                    new MsSqlStreamStoreV3Settings(
                        sp
                            .GetService<IConfiguration>()
                            .GetConnectionString(WellknownConnectionNames.Events)
                    ) {Schema = WellknownSchemas.EventSchema}));
            RegisterProjections();
            _services.AddSingleton(_listOfProjections);
            builder.Populate(_services);
        }

        private void RegisterProjection<TContext>(ProjectionDetail projectionDetail) where TContext : DbContext
        {
            var connection = _configuration.GetConnectionString(projectionDetail.WellKnownConnectionName);
            var dbContextOptions= new DbContextOptionsBuilder<TContext>()
                .UseSqlServer(connection,o => o
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite())
                .Options;

            var ctxFactory = (Func<DbContext>) (() =>
                (DbContext) Activator.CreateInstance(typeof(TContext), dbContextOptions)!);

            _listOfProjections.Add(projectionDetail, ctxFactory);
        }

        private void RegisterProjections()
        {
            RegisterProjection<WmsContext>(new ProjectionDetail
            {
                Id = "roadregistry-wms-projectionhost",
                Description = "",
                Name = "WMS Wegenregister",
                WellKnownConnectionName = WellknownConnectionNames.WmsProjections
            });
            RegisterProjection<WfsContext>(new ProjectionDetail
            {
                Id = "roadregistry-wfs-projectionhost",
                Description = "",
                Name = "WFS Wegenregister",
                WellKnownConnectionName = WellknownConnectionNames.WfsProjections
            });
        }
    }
}
