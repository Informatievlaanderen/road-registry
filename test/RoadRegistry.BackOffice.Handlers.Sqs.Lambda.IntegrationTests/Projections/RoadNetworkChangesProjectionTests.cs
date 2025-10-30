namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections;

using JasperFx.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;

//TODO-pr nog te verplaatsen, welke assembly? dit is geen lambda, enkel Marten

[Collection("DockerFixtureCollection")]
public class RoadNetworkChangesProjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public RoadNetworkChangesProjectionTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public void TestProjection()
    {
        var sp = BuildServiceProvider();

        //TODO-pr register events + run projection + check result in marten documents


    }

    private IServiceProvider BuildServiceProvider()
    {
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
            .AddMartenRoad(options =>
            {
                RoadNodeProjection.Configure(options);
                RoadSegmentProjection.Configure(options);
                options.AddRoadNetworkChangesProjection("projection_roadnetworkchanges", [new RoadNodeProjection(), new RoadSegmentProjection()], ProjectionLifecycle.Inline);
            })
            ;

        var sp = services.BuildServiceProvider();
        return sp;
    }
}
