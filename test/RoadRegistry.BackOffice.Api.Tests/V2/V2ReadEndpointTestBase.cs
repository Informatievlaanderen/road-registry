namespace RoadRegistry.BackOffice.Api.Tests.V2;

using AutoFixture;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

/// <summary>
/// Base for the V2 read-endpoint controller tests. Provides an in-memory Marten document store
/// (configured with the read-model document mappings), a fixture for building V2 events and a
/// helper to seed read items directly - no Postgres/Marten infrastructure is required.
/// </summary>
public abstract class V2ReadEndpointTestBase
{
    protected InMemoryDocumentStoreSession Store { get; }
    protected ApiOptions ApiOptions { get; }
    protected Fixture Fixture { get; }

    protected V2ReadEndpointTestBase()
    {
        var options = new StoreOptions();
        options.ConfigureRoad();
        options.ConfigureReadDocuments();

        Store = new InMemoryDocumentStoreSession(options);
        ApiOptions = new ApiOptions { BaseUrl = "https://test.basisregisters.vlaanderen" };
        Fixture = new RoadNetworkTestDataV2().Fixture;
    }

    protected void Seed<T>(T item)
        where T : notnull
    {
        var session = Store.LightweightSession();
        session.Store(item);
    }

    protected static BackofficeApiControllerContext CreateControllerContext()
    {
        return new BackofficeApiControllerContext(
            new FakeTicketingOptions(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
    }

    protected static void SetHttpContext(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}
