namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using System.Security.Claims;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.Inwinning;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public partial class InwinningsstatusControllerTests : IAsyncLifetime
{
    private readonly DbContextBuilder _dbContextBuilder;
    private ExtractsDbContext _extractsDbContext;
    protected IFixture Fixture { get; }
    protected InwinningsstatusController Controller { get; }
    protected string TestOrgCode { get; }

    public InwinningsstatusControllerTests(
        DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder.ThrowIfNull();
        Fixture = new RoadNetworkTestData().ObjectProvider;
        TestOrgCode = Fixture.Create<OrganizationOvoCode>();
        Controller = BuildController();
    }

    private InwinningsstatusController BuildController()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("vo_orgcode", TestOrgCode)]))
        };

        return new InwinningsstatusController(new BackofficeApiControllerContext(
            new FakeTicketingOptions(),
            new HttpContextAccessor
            {
                HttpContext = httpContext
            }))
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
