using System;

namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts
{
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
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
