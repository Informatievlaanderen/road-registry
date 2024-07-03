namespace RoadRegistry.Projector.Infrastructure.Modules;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Editor.Schema;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using Product.Schema;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Syndication.Schema;
using Sync.StreetNameRegistry;
using System;
using System.Collections.Generic;
using Integration.Schema;
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
        builder
            .RegisterType<ProblemDetailsHelper>()
            .AsSelf();
        _services.AddStreamStore();
        RegisterProjections();
        _services.AddSingleton(_listOfProjections);
        builder.Populate(_services);
    }

    private void RegisterProjections()
    {
        var projectionOptions = _configuration.GetOptions<ProjectionOptions>("Projections");

        if (projectionOptions.ProducerSnapshot.Enabled)
        {
            RegisterProducerSnapshotProjection();
        }

        if (projectionOptions.Product.Enabled)
        {
            RegisterProductProjection();
        }

        if (projectionOptions.Editor.Enabled)
        {
            RegisterEditorProjections();
        }

        if (projectionOptions.Syndication.Enabled)
        {
            RegisterSyndicationProjections();
        }

        if (projectionOptions.Wms.Enabled)
        {
            RegisterWmsProjections();
        }

        if (projectionOptions.Wfs.Enabled)
        {
            RegisterWfsProjections();
        }

        if (projectionOptions.BackOfficeProcessors.Enabled)
        {
            RegisterBackOfficeProcessors();
        }

        if (projectionOptions.Integration.Enabled)
        {
            RegisterIntegrationProjections();
        }

        if (projectionOptions.StreetNameSync.Enabled)
        {
            RegisterStreetNameProjection();
        }
    }

    private void RegisterProducerSnapshotProjection()
    {
        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadNode.RoadNodeProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadnode-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadNode",
            WellKnownConnectionName = WellKnownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadSegment.RoadSegmentProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadsegment-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadSegment",
            WellKnownConnectionName = WellKnownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.RoadSegmentSurface.RoadSegmentSurfaceProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-roadsegmentsurface-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - RoadSegmentSurface",
            WellKnownConnectionName = WellKnownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.GradeSeparatedJunction.GradeSeparatedJunctionProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-gradeseparatedjunction-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - GradeSeparatedJunction",
            WellKnownConnectionName = WellKnownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<Producer.Snapshot.ProjectionHost.NationalRoad.NationalRoadProducerSnapshotContext>(new ProjectionDetail
        {
            Id = "roadregistry-producer-nationalroad-snapshot-projectionhost",
            Description = "",
            Name = "Producer Snapshot - NationalRoad",
            WellKnownConnectionName = WellKnownConnectionNames.ProducerSnapshotProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterProductProjection()
    {
        RegisterProjection<ProductContext>(new ProjectionDetail
        {
            Id = "roadregistry-product-projectionhost",
            Description = "",
            Name = "Product",
            WellKnownConnectionName = WellKnownConnectionNames.ProductProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterEditorProjections()
    {
        RegisterProjection<EditorContext>(new ProjectionDetail
        {
            Id = "roadregistry-editor-projectionhost",
            Description = "",
            Name = "Editor",
            WellKnownConnectionName = WellKnownConnectionNames.EditorProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterWmsProjections()
    {
        RegisterProjection<WmsContext>(new ProjectionDetail
        {
            Id = "roadregistry-wms-projectionhost",
            Description = "Projectie die de wegen data voor het WMS wegenregister voorziet.",
            Name = "WMS Wegen",
            WellKnownConnectionName = WellKnownConnectionNames.WmsProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterWfsProjections()
    {
        RegisterProjection<WfsContext>(new ProjectionDetail
        {
            Id = "roadregistry-wfs-projectionhost",
            Description = "Projectie die de wegen data voor het WFS wegenregister voorziet.",
            Name = "WFS Wegen",
            WellKnownConnectionName = WellKnownConnectionNames.WfsProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterBackOfficeProcessors()
    {
        RegisterProjection<BackOfficeProcessorDbContext>(new ProjectionDetail
        {
            Id = WellKnownQueues.EventQueue,
            Description = "Verwerker van events",
            Name = "BackOffice Processor - Event",
            WellKnownConnectionName = WellKnownConnectionNames.Events,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<BackOfficeProcessorDbContext>(new ProjectionDetail
        {
            Id = WellKnownQueues.ExtractQueue,
            Description = "Verwerker van extract events",
            Name = "BackOffice Processor - Extract Event",
            WellKnownConnectionName = WellKnownConnectionNames.Events,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<BackOfficeProcessorDbContext>(new ProjectionDetail
        {
            Id = WellKnownQueues.CommandQueue,
            Description = "Verwerker van commando's",
            Name = "BackOffice Processor - Command",
            WellKnownConnectionName = WellKnownConnectionNames.Events,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<BackOfficeProcessorDbContext>(new ProjectionDetail
        {
            Id = WellKnownQueues.ExtractCommandQueue,
            Description = "Verwerker van extract commando's",
            Name = "BackOffice Processor - Extract Command",
            WellKnownConnectionName = WellKnownConnectionNames.Events,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterSyndicationProjections()
    {
        RegisterProjection<SyndicationContext>(new ProjectionDetail
        {
            Id = "roadregistry-syndication-projectionhost-Gemeente",
            Name = "municipality",
            WellKnownConnectionName = WellKnownConnectionNames.SyndicationProjections,
            IsSyndication = true
        });
    }

    private void RegisterIntegrationProjections()
    {
        RegisterProjection<IntegrationContext>(new ProjectionDetail
        {
            Id = "roadregistry-integration-organization-latestitem-projectionhost",
            Description = "",
            Name = "Integration - Organization Latest Item",
            WellKnownConnectionName = WellKnownConnectionNames.IntegrationProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<IntegrationContext>(new ProjectionDetail
        {
            Id = "roadregistry-integration-organization-version-projectionhost",
            Description = "",
            Name = "Integration - Organization Version",
            WellKnownConnectionName = WellKnownConnectionNames.IntegrationProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<IntegrationContext>(new ProjectionDetail
        {
            Id = "roadregistry-integration-roadnetwork-latestitem-projectionhost",
            Description = "",
            Name = "Integration - RoadNetwork Latest Item",
            WellKnownConnectionName = WellKnownConnectionNames.IntegrationProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });

        RegisterProjection<IntegrationContext>(new ProjectionDetail
        {
            Id = "roadregistry-integration-roadnetwork-version-projectionhost",
            Description = "",
            Name = "Integration - RoadNetwork Version",
            WellKnownConnectionName = WellKnownConnectionNames.IntegrationProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterStreetNameProjection()
    {
        RegisterProjection<StreetNameSnapshotProjectionContext>(new ProjectionDetail
        {
            Id = "roadregistry-sync-streetnameprojection",
            Description = "",
            Name = "Sync - Street Name Snapshot",
            WellKnownConnectionName = WellKnownConnectionNames.StreetNameProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
        RegisterProjection<StreetNameEventProjectionContext>(new ProjectionDetail
        {
            Id = "roadregistry-sync-streetnameeventprojection",
            Description = "",
            Name = "Sync - Street Name Event",
            WellKnownConnectionName = WellKnownConnectionNames.StreetNameProjections,
            FallbackDesiredState = "subscribed",
            IsSyndication = false
        });
    }

    private void RegisterProjection<TContext>(ProjectionDetail projectionDetail) where TContext : DbContext
    {
        var connectionString = _configuration.GetRequiredConnectionString(projectionDetail.WellKnownConnectionName);

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TContext>();
        if (connectionString.Contains("host=", StringComparison.InvariantCultureIgnoreCase))
        {
            dbContextOptionsBuilder
                .UseNpgsql(connectionString, o => o
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite());
        }
        else
        {
            dbContextOptionsBuilder
                .UseSqlServer(connectionString, o => o
                    .EnableRetryOnFailure()
                    .UseNetTopologySuite());
        }

        var ctxFactory = (Func<DbContext>)(() =>
            (DbContext)Activator.CreateInstance(typeof(TContext), dbContextOptionsBuilder.Options)!);

        _listOfProjections.Add(projectionDetail, ctxFactory);
    }
}
