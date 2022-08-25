namespace RoadRegistry.Tests.BackOffice;

using Xunit;

public class NationalRoadNumbersTests
{
    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            747,
            NationalRoadNumbers.All.Length);
    }
}
