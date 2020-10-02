namespace RoadRegistry.BackOffice.Scenarios
{
    public class PendingScenarios
    {
        // - Each road node id should only appear in one road node requested change (AddRoadNode, ModifyRoadNode, RemoveRoadNode)
        // - Each road segment id should only appear in one road segment requested change (AddRoadSegment, ModifyRoadSegment, RemoveRoadSegment)
        // - Each grade separated junction id should only appear in one grade separated junction requested change (AddGradeSeparatedJunction, ModifyGradeSeparatedJunction, RemoveGradeSeparatedJunction)

        // Order of evaluating requested changes matters (done)
        // Messages.AddRoadNode
        // Messages.AddRoadSegment
        // Messages.AddRoadSegmentToEuropeanRoad
        // Messages.AddRoadSegmentToNationalRoad
        // Messages.AddRoadSegmentToNumberedRoad
        // Messages.AddGradeSeparatedJunction
        // Messages.ModifyRoadNode
        // Messages.ModifyRoadSegment
        // Messages.ModifyGradeSeparatedJunction
        // Messages.RemoveRoadSegmentFromEuropeanRoad
        // Messages.RemoveRoadSegmentFromNationalRoad
        // Messages.RemoveRoadSegmentFromNumberedRoad
        // Messages.RemoveGradeSeparatedJunction
        // Messages.RemoveRoadSegment
        // Messages.RemoveRoadNod

        // Modifying or removing a road node: the node needs to exist
        // Modifying or removing a road segment: the segment needs to exist
        // Modifying or removing a grade separated junction: the grade separated junction needs to exist

        // Modifying a road segment to a new start and / or end node needs to cause the old nodes to become disconnected (unless they are removed)

        // After all changes have been applied
        // - for modified nodes: node type needs be valid
        // - for modified segments: the connected and / or disconnected start and /or end nodes need to have a valid node type (unless the disconnected nodes are removed)
        // - for removed segments: the disconnected start and end node need to have a valid node type unless removed themselves
        // - for removed nodes: the segments that used to be connected must no longer be connected to any of these nodes
    }
}
