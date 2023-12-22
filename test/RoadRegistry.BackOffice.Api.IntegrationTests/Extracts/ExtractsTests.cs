namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts
{
    using Xunit;
    using Xunit.Abstractions;

    public partial class ExtractsTests : IClassFixture<IntegrationTestFixture>
    {
        protected IntegrationTestFixture Fixture { get; }
        protected ITestOutputHelper TestOutputHelper { get; }

        public ExtractsTests(IntegrationTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            TestOutputHelper = testOutputHelper;
        }
    }
}
