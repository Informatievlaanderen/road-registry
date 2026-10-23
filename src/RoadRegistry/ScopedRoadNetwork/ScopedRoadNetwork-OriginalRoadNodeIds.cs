namespace RoadRegistry.ScopedRoadNetwork;

using System.Globalization;
using System.Linq;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // A municipality uploads road nodes under the WK_OIDN it knows them by, but the feature compare matches them on
    // geometry: the road node it ends up pointing at can carry a different identifier, the one from the extract. Every
    // problem raised about that road node then names an identifier the uploader cannot find anything by.
    //
    // So both are reported. The identifier is read off the upload, where it is known, and carried as far as the
    // identifier translator - which is also where a road node that is newly added keeps the one it was uploaded with.

    private const string RoadNodeIdParameterName = "WegknoopId";

    private static void RegisterOriginalRoadNodeIds(RoadNetworkChanges changes, ScopedRoadNetworkChangeContext context)
    {
        foreach (var change in changes)
        {
            switch (change)
            {
                case ModifyRoadNodeChange { OriginalId: not null } modify:
                    context.IdTranslator.RegisterOriginalId(modify.OriginalId.Value, modify.RoadNodeId);
                    break;
            }
        }
    }

    // Problems are raised deep in the road network, where only the identifier it uses is at hand, so the one the
    // uploader used is added afterwards - to all of them at once, whichever action raised them.
    private static Problems WithOriginalRoadNodeIds(Problems problems, ScopedRoadNetworkChangeContext context)
    {
        foreach (var problem in problems)
        {
            var roadNodeId = problem.Parameters.FirstOrDefault(x => x.Name == RoadNodeIdParameterName);
            if (roadNodeId is null || !int.TryParse(roadNodeId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                continue;
            }

            var originalId = context.IdTranslator.TranslateToOriginalId(new RoadNodeId(id));
            if (originalId is null)
            {
                continue;
            }

            problem.WithContext(ProblemContext.ForOriginalRoadNodeId(originalId.Value));
        }

        return problems;
    }
}
