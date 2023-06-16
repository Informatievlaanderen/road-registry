namespace RoadRegistry.Tests.BackOffice
{
    using RoadRegistry.BackOffice;

    public class UIDNTests
    {
        [Theory]
        [InlineData("0_0", 0, 0)]
        [InlineData("1_2", 1, 2)]
        [InlineData(" 1_2", 1, 2)]
        [InlineData("1_2 ", 1, 2)]
        public void ParseShouldSucceed(string value, int expectedId, int expectedVersion)
        {
            var uidn = UIDN.Parse(value);
            Assert.Equal(expectedId, uidn.Id);
            Assert.Equal(expectedVersion, uidn.Version);
            Assert.Equal(value, uidn);
        }

        [Theory]
        [InlineData((string)null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("a_b")]
        [InlineData("1")]
        [InlineData("1_b")]
        [InlineData("a_2")]
        [InlineData("1_2_3")]
        public void ParseShouldFail(string value)
        {
            Assert.ThrowsAny<Exception>(() => UIDN.Parse(value));
        }
    }
}
