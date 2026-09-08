namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-2 of the junction editing actions: the identifier names no grade separated junction at all. Deliberately not the
// shared GradeSeparatedJunctionNotFound, whose message is about the road network topology rather than about a caller
// naming something that is not there.
public class GradeSeparatedJunctionDoesNotExist : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeSeparatedJunction.DoesNotExist;

    public GradeSeparatedJunctionDoesNotExist(GradeSeparatedJunctionId identifier)
        : base(ProblemCode,
            new ProblemParameter("OngelijkGrondseKruisingId", identifier.ToInt32().ToString()))
    {
    }
}
