namespace RoadRegistry.RoadNetwork;

using System.Linq;
using BackOffice;
using BackOffice.Core;
using BackOffice.Core.ProblemCodes;
using Changes;
using NetTopologySuite.Geometries;
using RoadSegment;
using ValueObjects;

public partial class RoadNetwork
{
    private Problems AddRoadSegment(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        // ************ vervanging van de huidige Core classes per command
        var roadSegment = new RoadSegment();
        var problems = roadSegment.Add(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        RoadSegments.Add(roadSegment.Id, roadSegment);
        return problems;
    }

    private Problems ModifyRoadSegment(ModifyRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        // ************ vervanging van de huidige Core classes per command

        // verify before (guard)

        // generate + apply events

        // verify after

        // return events
        return Problems.None;
    }
}
