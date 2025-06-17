namespace MartenPoc.Projections;

using System;
using Domain;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public sealed record RoadSegment(
    Guid Id,
    int Version,
    string Geometry,
    Guid StartNodeId,
    Guid EndNodeId,
    string CausationId);

public class RoadSegmentProjection : MultiStreamProjection<RoadSegment, string>
{
    //TODO-pr afwerken
    private const string TableName = "road.segments";

    public RoadSegmentProjection()
    {
        Identity<IEvent>(e => e.CausationId ?? throw new ArgumentException("CausationId must be present"));

        // var table = new Table(TableName);
        // table.AddColumn<Guid>(nameof(RoadSegment.Id)).AsPrimaryKey();
        // table.AddColumn<int>(nameof(RoadSegment.Version));
        // table.AddColumn(nameof(RoadSegment.Geometry), "geometry");
        // table.AddColumn<Guid>(nameof(RoadSegment.StartNodeId));
        // table.AddColumn<Guid>(nameof(RoadSegment.EndNodeId));
        // table.AddColumn<string>(nameof(RoadSegment.CausationId));
        // SchemaObjects.Add(table);

        // Options.DeleteDataInTableOnTeardown(table.Identifier.QualifiedName);

        // ProjectEvent<IEvent>((view, evt) =>
        // {
        //     view.Events.Add(evt);
        //     view.LastUpdated = evt.Timestamp.UtcDateTime;
        // });
    }

    public void Project(IEvent<WegsegmentWerdToegevoegd> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"insert into {TableName} ({nameof(RoadSegment.Id)}, {nameof(RoadSegment.Version)}, {nameof(RoadSegment.Geometry)}, {nameof(RoadSegment.StartNodeId)}, {nameof(RoadSegment.EndNodeId)}, {nameof(RoadSegment.CausationId)}) values (?, ?, ST_GeomFromText(?, 0), ?, ?, ?)",
            e.Data.Id, e.Version, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId
        );
    }

    public void Project(IEvent<WegsegmentWerdGewijzigd> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($@"
UPDATE {TableName}
SET
    {nameof(RoadSegment.Version)} = ?,
    {nameof(RoadSegment.Geometry)} = ST_GeomFromText(?, 0),
    {nameof(RoadSegment.StartNodeId)} = ?,
    {nameof(RoadSegment.EndNodeId)} = ?,
    {nameof(RoadSegment.CausationId)} = ?
WHERE {nameof(RoadSegment.Id)} = ?",
            e.Version, e.Data.Geometry, e.Data.StartNodeId, e.Data.EndNodeId, e.CausationId, e.Data.Id
        );
    }
}

