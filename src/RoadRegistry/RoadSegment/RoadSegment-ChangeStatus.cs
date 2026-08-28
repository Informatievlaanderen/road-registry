namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // Records that this segment is now knotted into the network and realized. The geometry is the one it ended up
    // with after snapping, and the attributes are already remapped onto it by the caller. Which event says so is the
    // status change's business - the status a segment takes on is the event itself.
    public Problems ChangeStatusToConnected(
        RoadSegmentStatusChange statusChange,
        RoadSegmentGeometry geometry,
        RoadSegmentAttributes attributes,
        ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        problems += geometry.ValidateRoadSegmentGeometryDomainV2();
        problems += new RoadSegmentAttributesValidator().Validate(attributes, geometry.Value.Length);

        // A realized segment hangs off a road node at either end. They are resolved from whatever sits on the
        // endpoints, so the nodes have to be in place before this is called.
        var startEndNodes = context.RoadNetwork.FindStartEndNodes(geometry);
        problems += startEndNodes.Problems;

        if (problems.HasError())
        {
            return problems;
        }

        ApplyStatusChangeToConnected(statusChange.BuildEvent(new RoadSegmentConnectedChangeData
        {
            RoadSegmentId = RoadSegmentId,
            Geometry = geometry,
            StartNodeId = startEndNodes.NodeIds.Start!.Value,
            EndNodeId = startEndNodes.NodeIds.End!.Value,
            Attributes = attributes,
            Provenance = new ProvenanceData(context.Provenance)
        }));

        return problems;
    }

    // Records that this segment is no longer realized and has come loose from the road nodes it hung off. The caller
    // has already established that it is realized, which is what guarantees both node identifiers are there.
    public Problems ChangeStatusFromConnected(RoadSegmentStatusChange statusChange, ScopedRoadNetworkChangeContext context)
    {
        ApplyStatusChangeFromConnected(statusChange.BuildEvent(new RoadSegmentDisconnectedChangeData
        {
            RoadSegmentId = RoadSegmentId,
            PreviousStartNodeId = StartNodeId!.Value,
            PreviousEndNodeId = EndNodeId!.Value,
            Provenance = new ProvenanceData(context.Provenance)
        }));

        return Problems.None;
    }

    // Records a status change between two statuses that both leave the segment outside the network. It carried no
    // road nodes before and carries none after, so there is nothing to record but the change itself.
    public Problems ChangeStatusWhileUnconnected(RoadSegmentStatusChange statusChange, ScopedRoadNetworkChangeContext context)
    {
        ApplyStatusChangeWhileUnconnected(statusChange.BuildEvent(new RoadSegmentUnconnectedChangeData
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(context.Provenance)
        }));

        return Problems.None;
    }
}
