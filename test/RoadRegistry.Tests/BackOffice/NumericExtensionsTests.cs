namespace RoadRegistry.Tests.BackOffice
{
    public class NumericExtensionsTests
    {
        private const decimal TOLERANCE = 0.001M;

        [Theory]
        [InlineData(false, 0, -0.0011)]
        [InlineData(true, 0, -0.001)]
        [InlineData(true, 0, -0.0001)]
        [InlineData(true, 0, 0)]
        [InlineData(true, 0, 0.0001)]
        [InlineData(true, 0, 0.001)]
        [InlineData(false, 0, 0.0011)]
        public void IsReasonablyEqualTo(bool expected, decimal value1, decimal value2)
        {
            Assert.Equal(expected, value1.IsReasonablyEqualTo(value2, TOLERANCE));
        }

        [Theory]
        [InlineData(false, 0, 0)]
        [InlineData(false, 0, 0.0001)]
        [InlineData(false, 0, 0.001)]
        [InlineData(true, 0, 0.0011)]
        public void IsReasonablyLessThan(bool expected, decimal value1, decimal value2)
        {
            Assert.Equal(expected, value1.IsReasonablyLessThan(value2, TOLERANCE));
        }
    }
}
