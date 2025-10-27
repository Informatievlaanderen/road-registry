namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using Autofac;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork;
using Core;
using Handlers.RoadNetwork;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Requests.RoadNetwork;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using Sqs.RoadNetwork;
using Tests.BackOffice;
using Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

[Collection("DockerFixtureCollection")]
public class WithValidRequest : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly Mock<ITicketing> _ticketingMock = new();
    private readonly Mock<IExtractRequests> _extractRequestsMock = new();

    public WithValidRequest(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        //TODO-pr seed marten topology projection met data + start lambda function met command + verify ticketing status + verify data in marten

        // Arrange
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", _databaseFixture.ConnectionString)
            ])
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(_ticketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(_extractRequestsMock.Object);

        services
            .AddMartenRoadNetworkRepository()
            .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddChangeRoadNetworkCommandHandler()
            .AddScoped<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();

        var sp = services.BuildServiceProvider();

        var sqsRequest = new ChangeRoadNetworkCommandSqsRequest
        {
            Request = new ChangeRoadNetworkCommand(),
            ProvenanceData = new RoadRegistryProvenanceData()
        };

        // Act
        var handler = sp.GetRequiredService<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkCommandSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        // Assert

    }
}
