namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts
{
    using IntegrationTests;
    using Xunit;
    using Xunit.Abstractions;

    public partial class ExtractsTests : IClassFixture<ApiClientTestFixture>
    {
        protected ApiClientTestFixture Fixture { get; }
        protected ITestOutputHelper TestOutputHelper { get; }

        public ExtractsTests(ApiClientTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            TestOutputHelper = testOutputHelper;
        }
    }
}
