namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Extensions;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

public partial class ScopedRoadNetwork
{
    // A correction, not an operation an operator can ask for.
    //
    // Before CanAddGradeJunction, a crossing whose segments were only half migrated read as carrying no traffic at all
    // - a segment that is still V1 has no traffic attributes - so a gelijkgrondse kruising was recorded next to the
    // ongelijkgrondse one that already described that very crossing. A segment pair never holds both kinds of kruising
    // at once, so those gelijkgrondse kruisingen are removed here.
    //
    // Nothing is persisted by this call; the caller decides whether to save, which is what makes a dry run possible.
    public RoadNetworkChangeResult RemoveGradeJunctionsSupersededByGradeSeparatedJunctions(Provenance provenance, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var problems = Problems.None;

        foreach (var gradeJunction in GetGradeJunctionsSupersededByGradeSeparatedJunctions())
        {
            problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context);
        }

        if (!problems.HasError() && context.Summary.GradeJunctions.Removed.Count > 0)
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(problems, context.Summary);
    }

    // The gelijkgrondse kruisingen of this scope whose segment pair also carries an ongelijkgrondse kruising. Public so
    // a caller can report on them without having to repeat the rule.
    public IReadOnlyCollection<GradeJunction> GetGradeJunctionsSupersededByGradeSeparatedJunctions()
    {
        var gradeSeparatedJunctionKeys = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .Select(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId))
            .ToHashSet();

        return _gradeJunctions.Values
            .Where(x => !x.IsRemoved && gradeSeparatedJunctionKeys.Contains(CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2)))
            .ToArray();
    }

    // The ongelijkgrondse kruising that supersedes the given gelijkgrondse one, for reporting.
    public GradeSeparatedJunction? FindGradeSeparatedJunctionFor(GradeJunction gradeJunction)
    {
        var key = CreateCombinationKey(gradeJunction.RoadSegmentId1, gradeJunction.RoadSegmentId2);

        return _gradeSeparatedJunctions.Values
            .FirstOrDefault(x => !x.IsRemoved && CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId) == key);
    }
}
