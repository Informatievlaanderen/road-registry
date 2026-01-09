namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Security.Claims;
using Api.Infrastructure.Controllers;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.Extracten;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public partial class ExtractsControllerTests : IAsyncLifetime
{
    private readonly DbContextBuilder _dbContextBuilder;
    private ExtractsDbContext _extractsDbContext;
    protected IFixture Fixture { get; }
    protected Mock<IMediator> Mediator { get; }
    protected ExtractenController Controller { get; }
    protected string TestOrgCode { get; }

    public ExtractsControllerTests(
        DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder.ThrowIfNull();
        Fixture = new RoadNetworkTestData().ObjectProvider;
        Mediator = new Mock<IMediator>();
        TestOrgCode = Fixture.Create<OrganizationOvoCode>();
        Controller = BuildController();
    }

    private ExtractenController BuildController()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("vo_orgcode", TestOrgCode)]))
        };

        return new ExtractenController(new BackofficeApiControllerContext(
            new FakeTicketingOptions(),
            new HttpContextAccessor
            {
                HttpContext = httpContext
            }), Mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    public async Task DisposeAsync()
    {
        await _extractsDbContext.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
    }
}
