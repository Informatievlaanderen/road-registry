namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using GradeSeparatedJunction;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Projections;
using RoadNode;
using RoadSegment;

public static class SetupExtensions
{
    public static void ConfigureRoadNetworkRepository(this StoreOptions options)
    {
        options.Events.MetadataConfig.CausationIdEnabled = true;

        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline);
        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        // options.Schema.For<Wegsegment>().Metadata(opts =>
        // {
        //     opts.Version.MapTo(x => x.Version);
        // });
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);
        // options.Schema.For<Wegknoop>().Metadata(opts =>
        // {
        //     opts.Version.MapTo(x => x.Version);
        // });
    }
}
