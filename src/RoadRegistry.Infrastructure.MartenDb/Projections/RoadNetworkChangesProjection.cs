namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public sealed record RoadNetworkChangesProjectionProgression(string Id, string ProjectionName, long LastSequenceId);

public abstract class RoadNetworkChangesProjection : IProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
    }

    public void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNetworkChangesProjectionProgression>()
            .DatabaseSchemaName("eventstore")
            .DocumentAlias("roadnetworkchangesprojection_progression")
            .Identity(x => x.Id)
            .Index(x => x.ProjectionName);
    }
    protected virtual void ConfigureSchema(StoreOptions options)
    {
    }

    //TODO-pr current in table bijhouden welke correlationids zijn verwerkt (incl hun hoogste eventposition + projection name)
    //wanneer batchsize gelijk is aan max, dan aparte query'en voor nog meer data op te halen
    //tbd: indien te traag dan dit enkel doen wanneer de projectie dichtbij het einde is (bvb -50k)
    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var eventsPerCorrelationId = events
            .GroupBy(x => x.CorrelationId!)
            .ToList();

        foreach (var eventsGrouping in eventsPerCorrelationId.Select((g, i) => (CorrelationId: g.Key, Events: g.ToArray(), IsLast: i == eventsPerCorrelationId.Count - 1)))
        {
            // if (eventsGrouping.IsLast)
            // {
            //     var eventProcessed = await operations.LoadAsync<RoadNetworkChangesProjectionProgression>(eventIdentifier, token);
            //     if (eventProcessed is not null)
            //     {
            //         return;
            //     }
            //
            //     var processEvents = operations.Events.QueryAllRawEvents()
            //         .Where(x => x.CorrelationId == correlationId) //TODO-pr add index on correlationId
            //         .ToList()
            //         .AsReadOnly();
            //     foreach (var projection in _projections)
            //     {
            //         await projection.Project(events, operations, cancellation).ConfigureAwait(false);
            //     }
            //
            //
            //     //session.Insert(new MigratedEvent(eventIdentifier));
            // }
            // else
            {
                foreach (var projection in _projections)
                {
                    await projection.Project(eventsGrouping.Events, operations, cancellation).ConfigureAwait(false);
                }
            }
        }
    }
}
