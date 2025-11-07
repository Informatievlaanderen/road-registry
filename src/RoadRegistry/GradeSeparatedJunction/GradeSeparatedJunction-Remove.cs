namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using Events;

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
