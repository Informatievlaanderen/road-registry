namespace RoadRegistry.Projector.Infrastructure.Modules;

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
using Editor.Schema;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Schema;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using SqlStreamStore;
using Syndication.Schema;
using Wfs.Schema;
using Wms.Schema;

public class ApiModule : Module
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<ProjectionDetail, Func<DbContext>> _listOfProjections = new();
    private readonly IServiceCollection _services;

    public ApiModule(
        IConfiguration configuration,
        IServiceCollection services)
    {
        _configuration = configuration;
        _services = services;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule(new DataDogModule(_configuration));
        builder
            .RegisterType<ProblemDetailsHelper>()
            .AsSelf();
        _services.AddStreamStore();
        RegisterProjections();
        _services.AddSingleton(_listOfProjections);
        builder.Populate(_services);
    }

    private void RegisterProjection<TContext>(ProjectionDetail projectionDetail) where TContext : DbContext
    {
        var connection = _configuration.GetConnectionString(projectionDetail.WellKnownConnectionName);
        var dbContextOptions = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(connection, o => o
                .EnableRetryOnFailure()
                .UseNetTopologySuite())
            .Options;

        var ctxFactory = (Func<DbContext>)(() =>
            (DbContext)Activator.CreateInstance(typeof(TContext), dbContextOptions)!);

        _listOfProjections.Add(projectionDetail, ctxFactory);
    }

    private void RegisterProjections()
    {
        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadNode.RoadNodeProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadnode-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadNode",
            WellKnownConnectionName = WellknownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadSegment.RoadSegmentProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadsegment-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadSegment",
            WellKnownConnectionName = WellknownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadSegmentSurface.RoadSegmentSurfaceProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadsegmentsurface-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadSegmentSurface",
            WellKnownConnectionName = WellknownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.GradeSeparatedJunction.GradeSeparatedJunctionProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-gradeseparatedjunction-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - GradeSeparatedJunction",
            WellKnownConnectionName = WellknownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.NationalRoad.NationalRoadProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-nationalroad-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - NationalRoad",
            WellKnownConnectionName = WellknownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<ProductContext>(new ProjectionDetail
        {
            Id = "roadregistry-product-projectionhost",
            Description = "",
            Name = "Product",
            WellKnownConnectionName = WellknownConnectionNames.ProductProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
        RegisterProjection<EditorContext>(new ProjectionDetail
        {
            Id = "roadregistry-editor-projectionhost",
            Description = "",
            Name = "Editor",
            WellKnownConnectionName = WellknownConnectionNames.EditorProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
        RegisterProjection<WmsContext>(new ProjectionDetail
        {
            Id = "roadregistry-wms-projectionhost",
            Description = "Projectie die de wegen data voor het WMS wegenregister voorziet.",
            Name = "WMS Wegen",
            WellKnownConnectionName = WellknownConnectionNames.WmsProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
        RegisterProjection<WfsContext>(new ProjectionDetail
        {
            Id = "roadregistry-wfs-projectionhost",
            Description = "Projectie die de wegen data voor het WFS wegenregister voorziet.",
            Name = "WFS Wegen",
            WellKnownConnectionName = WellknownConnectionNames.WfsProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<SyndicationContext>(new ProjectionDetail
        {
            Id = "roadregistry-syndication-projectionhost-Gemeente",
            Name = "municipality",
            WellKnownConnectionName = WellknownConnectionNames.SyndicationProjections,
            IsSyndication = true
        });

        RegisterProjection<SyndicationContext>(new ProjectionDetail
        {
            Id = "roadregistry-syndication-projectionhost-StraatNaam",
            Name = "streetName",
            WellKnownConnectionName = WellknownConnectionNames.SyndicationProjections,
            IsSyndication = true
        });
    }
}
