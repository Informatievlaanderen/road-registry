namespace RoadRegistry.Tests.BackOffice
{
    using RoadRegistry.BackOffice.Extensions;

    public class FormattingExtensionsTests
    {
        [Theory]
        [InlineData("0", 0)]
        [InlineData("0.1", 0.1)]
        [InlineData("0.12", 0.12)]
        [InlineData("0.123", 0.123)]
        [InlineData("0.123", 0.1234)]
        [InlineData("0.124", 0.1235)]
        [InlineData("0.124", 0.1236)]
        public void ToRoundedMeasurementStringReturnsExpectedResult(string expected, double value)
        {
            Assert.Equal(expected, value.ToRoundedMeasurementString());
        }
    }
}
