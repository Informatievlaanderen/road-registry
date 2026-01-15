namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public abstract class RoadNetworkChangesProjection : IProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
    }

    public virtual void Configure(StoreOptions options)
    {
    }

    //TODO-pr in table bijhouden welke correlationids zijn verwerkt (incl hun hoogste eventposition + projection name)
    //wanneer batchsize gelijk is aan max, dan aparte query'en voor nog meer data op te halen
    //tbd: indien te traag dan dit enkel doen wanneer de projectie dichtbij het einde is (bvb -50k)
    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        foreach (var projection in _projections)
        {
            await projection.Project(events, operations, cancellation).ConfigureAwait(false);
        }
    }
}
