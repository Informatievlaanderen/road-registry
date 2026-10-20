namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using System.Security.Claims;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.ValueObjects;

// The endpoint takes the 'geschetste weg' scope as well as the 'ingemeten weg' scope. An outlined road segment is
// 'ingeschetst' by definition, so it is the command host that turns away a caller holding only 'ingemeten weg' - the
// endpoint just hands their entitlement along.
public class WhenCreateOutlineV2WithRoadScopes
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());
    private readonly Mock<IMediator> _mediator = new();
    private CreateRoadSegmentOutlineV2SqsRequest _capturedSqsRequest;

    public WhenCreateOutlineV2WithRoadScopes()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<CreateRoadSegmentOutlineV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (CreateRoadSegmentOutlineV2SqsRequest)r)
            .ReturnsAsync(new RoadNetworkTestData().ObjectProvider.Create<LocationResult>());
    }

    [Theory]
    [InlineData(true, false, Scopes.DvWrGeschetsteWegBeheer)]
    [InlineData(false, true, Scopes.DvWrIngemetenWegBeheer)]
    [InlineData(true, true, Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer)]
    public async Task GivenACaller_ThenTheirScopesAreSentAlong(bool ingeschetst, bool ingemeten, params string[] scopes)
    {
        var result = await Act(CreateOutlineV2Parameters.Valid(), scopes);

        result.Should().BeOfType<AcceptedResult>();
        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest.EditableGeometryDrawMethods.Should().Be(new EditableGeometryDrawMethods(ingeschetst, ingemeten));
    }

    private Task<IActionResult> Act(CreateOutlinedRoadSegmentV2Parameters parameters, string[] scopes)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(scopes.Select(x => new Claim(AcmIdmClaimTypes.Scope, x)), "Test"))
        };

        var controller = new RoadSegmentsController(
            new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()),
            _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller.CreateOutlinedRoadSegmentV2(_validator, new Mock<IRoadNetworkIdGenerator>().Object, parameters, CancellationToken.None);
    }
}
