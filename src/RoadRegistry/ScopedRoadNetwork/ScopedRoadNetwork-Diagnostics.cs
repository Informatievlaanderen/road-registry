namespace RoadRegistry.ScopedRoadNetwork;

using System.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode;

// TEMPORARY - diagnostics for GAWR-7827 follow-up (inwinning upload reporting a missing start/end road node and a
// removed road segment after a neighbouring inwinning removed both). Not meant to be merged as-is: it logs per
// change and walks every road node in scope to find the nearest one.
public partial class ScopedRoadNetwork
{
    private ILogger? _diagnosticLogger;

    public void AttachDiagnosticLogger(ILogger logger)
    {
        _diagnosticLogger = logger;
    }

    // What sits at a coordinate the lookup came back empty on. FindRoadNode matches exactly and only sees road nodes
    // that are not removed, so this says which of the two it was: nothing near at all, or something near that the
    // lookup is blind to.
    private void LogRoadNodeLookupMiss(string which, Coordinate coordinate)
    {
        if (_diagnosticLogger is null)
        {
            return;
        }

        var nearest = FindNearestRoadNodeIncludingRemoved(coordinate);
        if (nearest is null)
        {
            _diagnosticLogger.LogWarning(
                "DIAG {Which} road node not found at ({X}, {Y}) and there is no road node at all in scope",
                which, coordinate.X, coordinate.Y);
            return;
        }

        var (roadNode, distance) = nearest.Value;

        _diagnosticLogger.LogWarning(
            "DIAG {Which} road node not found at ({X}, {Y}); nearest is {RoadNodeId} at ({NodeX}, {NodeY}), distance {Distance}, IsRemoved {IsRemoved}, IsRemovedBecauseOfMigration {IsRemovedBecauseOfMigration}",
            which, coordinate.X, coordinate.Y,
            roadNode.RoadNodeId.ToInt32(),
            roadNode.Geometry.Value.Coordinate.X, roadNode.Geometry.Value.Coordinate.Y,
            distance,
            roadNode.IsRemoved,
            roadNode.IsRemovedBecauseOfMigration);
    }

    private (RoadNode RoadNode, double Distance)? FindNearestRoadNodeIncludingRemoved(Coordinate coordinate)
    {
        return _roadNodes.Values
            .Select(roadNode => (RoadNode: roadNode, Distance: roadNode.Geometry.Value.Coordinate.Distance(coordinate)))
            .OrderBy(x => x.Distance)
            .Cast<(RoadNode RoadNode, double Distance)?>()
            .FirstOrDefault();
    }

    private void LogDiagnostic(string message, params object?[] args)
    {
        _diagnosticLogger?.LogWarning("DIAG " + message, args);
    }
}
