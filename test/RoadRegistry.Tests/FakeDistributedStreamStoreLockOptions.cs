namespace RoadRegistry.Tests;

using Hosts;

public class FakeDistributedStreamStoreLockOptions : DistributedStreamStoreLockOptions
{
    public FakeDistributedStreamStoreLockOptions()
    {
        Enabled = false;
    }
}