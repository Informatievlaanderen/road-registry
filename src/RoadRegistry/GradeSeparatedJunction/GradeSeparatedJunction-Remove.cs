namespace RoadRegistry.GradeSeparatedJunction;

using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Remove()
    {
        Apply(new GradeSeparatedJunctionRemoved
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId
        });

        return Problems.None;
    }
}
