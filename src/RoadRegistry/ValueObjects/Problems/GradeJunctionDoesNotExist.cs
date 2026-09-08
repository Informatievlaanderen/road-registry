namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-2 of the junction editing actions: the identifier names no grade junction at all. Deliberately not the shared
// GradeJunctionNotFound, whose message is about the road network topology rather than about a caller naming something
// that is not there.
public class GradeJunctionDoesNotExist : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeJunction.DoesNotExist;

    public GradeJunctionDoesNotExist(GradeJunctionId identifier)
        : base(ProblemCode,
            new ProblemParameter("GelijkGrondseKruisingId", identifier.ToInt32().ToString()))
    {
    }
}
