namespace RoadRegistry.Tests.BackOffice;

using Xunit;

public class NumberedRoadNumbersTests
{
    [Fact]
    public void AllReturnsExpectedResult()
    {
        Assert.Equal(
            5807,
            NumberedRoadNumbers.All.Length);
    }
}
