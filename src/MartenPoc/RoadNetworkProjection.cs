namespace MartenPoc;

using System;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public class RoadNetworkProjection : EventProjection
{
    public RoadNetworkSegment Create(IEvent<WegsegmentWerdToegevoegd> input)
    {
        return new RoadNetworkSegment(input.Data.Id, input.Data.Geometry, input.Data.StartNodeId, input.Data.EndNodeId, input.CausationId);
    }

    public RoadNetworkSegment Create(IEvent<WegsegmentWerdGewijzigd> input)
    {
        return new RoadNetworkSegment(input.Data.Id, input.Data.Geometry, input.Data.StartNodeId, input.Data.EndNodeId, input.CausationId);
    }

    public void Apply(IDocumentOperations operations, IDocumentSession session, object @event)
    {
        if (@event is IHasGeometry geometry)
        {
            var wkt = geometry.Geometry;
            var id = ((IHasId)@event).Id;

            operations.QueueSqlCommand("""
                                           UPDATE mt_doc_roadnetworksegment
                                           SET geometry = ST_GeomFromText(:wkt, 0)
                                           WHERE id = :id
                                       """, new { id, wkt });
        }
    }
}

public record RoadNetworkSegment(Guid Id, string Geometry, Guid StartNodeId, Guid EndNodeId, string? CausationId)
{
    public static RoadNetworkSegment Create(WegsegmentWerdToegevoegd werdToegevoegd) =>
        new(
            werdToegevoegd.Id,
            werdToegevoegd.Geometry,
            werdToegevoegd.StartNodeId,
            werdToegevoegd.EndNodeId,
            null); //TODO-pr causationId?

    public RoadNetworkSegment Apply(WegsegmentWerdGewijzigd werdGewijzigd) =>
        this with
        {
            Geometry = werdGewijzigd.Geometry,
            StartNodeId = werdGewijzigd.StartNodeId,
            EndNodeId = werdGewijzigd.EndNodeId
        };
}
