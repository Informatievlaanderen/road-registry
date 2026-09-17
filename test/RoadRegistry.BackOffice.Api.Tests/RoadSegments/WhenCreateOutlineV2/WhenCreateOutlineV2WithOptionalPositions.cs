namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.ValueObjects;

public class WhenCreateOutlineV2WithOptionalPositions
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());
    private readonly Mock<IMediator> _mediator = new();
    private CreateRoadSegmentOutlineV2SqsRequest _capturedSqsRequest;

    public WhenCreateOutlineV2WithOptionalPositions()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<CreateRoadSegmentOutlineV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (CreateRoadSegmentOutlineV2SqsRequest)r)
            .ReturnsAsync(new RoadNetworkTestData().ObjectProvider.Create<LocationResult>());
    }

    [Fact]
    public async Task WhenNoPositionsAreGiven_ThenEveryAttributeCoversTheWholeSegment()
    {
        var parameters = CreateOutlineV2Parameters.Valid();

        var validation = await _validator.ValidateAsync(parameters);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Errors.Select(x => x.ErrorCode)));

        var result = await Act(parameters);

        Assert.IsType<AcceptedResult>(result);
        var zero = new RoadSegmentPositionV2(0);
        var length = new RoadSegmentPositionV2(CreateOutlineV2Parameters.GeometryLength);
        Assert.All(_capturedSqsRequest.Morphology, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.SurfaceType, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.AccessRestriction, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.StreetNameId, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.MaintenanceAuthorityId, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.Category, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.CarTrafficDirection, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.BikeTrafficDirection, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
        Assert.All(_capturedSqsRequest.PedestrianTrafficDirection, x => Assert.Equal((zero, length), (x.FromPosition, x.ToPosition)));
    }

    [Fact]
    public async Task WhenOnlyTheInnerPositionsAreGiven_ThenTheOuterOnesAreFilledIn()
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Morfologie =
            [
                new MorfologieParameters { VanPositie = null, TotPositie = 6, Morfologie = RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan.ToDutchString() },
                new MorfologieParameters { VanPositie = 6, TotPositie = null, Morfologie = RoadSegmentMorphologyV2.Pad.ToDutchString() }
            ]
        };

        var validation = await _validator.ValidateAsync(parameters);
        Assert.True(validation.IsValid, string.Join(Environment.NewLine, validation.Errors.Select(x => x.ErrorCode)));

        await Act(parameters);

        Assert.Equal(
            [
                (new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(6)),
                (new RoadSegmentPositionV2(6), new RoadSegmentPositionV2(CreateOutlineV2Parameters.GeometryLength))
            ],
            _capturedSqsRequest.Morphology.Select(x => (x.FromPosition, x.ToPosition)).ToArray());
    }

    [Theory]
    [InlineData("niet van toepassing")]
    [InlineData("Niet van toepassing")]
    public async Task WhenAStreetNameIsNotApplicable_ThenTheNotApplicableLocalIdIsSentAlong(string identificator)
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "links", Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "rechts", Identificator = identificator }
            ]
        };

        await Act(parameters);

        Assert.Equal(new StreetNameLocalId(71671), _capturedSqsRequest.StreetNameId.Single(x => x.Side == RoadSegmentAttributeSide.Links).StreetNameId);
        Assert.Equal(StreetNameLocalId.NotApplicable, _capturedSqsRequest.StreetNameId.Single(x => x.Side == RoadSegmentAttributeSide.Rechts).StreetNameId);
    }

    private Task<IActionResult> Act(CreateOutlinedRoadSegmentV2Parameters parameters)
    {
        var controller = new RoadSegmentsController(
            new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()),
            _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller.CreateOutlinedRoadSegmentV2(_validator, new Mock<IRoadNetworkIdGenerator>().Object, parameters, CancellationToken.None);
    }
}
