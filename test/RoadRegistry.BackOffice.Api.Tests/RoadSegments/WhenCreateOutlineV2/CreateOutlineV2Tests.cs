namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using Api.Infrastructure.Controllers;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.BackOffice;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class CreateOutlineV2Tests
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IRoadNetworkIdGenerator> _roadNetworkIdGenerator;
    private readonly RoadNetworkTestData _testData;

    public CreateOutlineV2Tests()
    {
        _testData = new RoadNetworkTestData();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<CreateRoadSegmentOutlineV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testData.ObjectProvider.Create<LocationResult>());

        _roadNetworkIdGenerator = new Mock<IRoadNetworkIdGenerator>();
    }

    private async Task<IActionResult> GetResultAsync(CreateOutlinedRoadSegmentV2Parameters request)
    {
        var controller = new RoadSegmentsController(
            new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()),
            _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var validator = new CreateOutlinedRoadSegmentV2ParametersValidator(new FakeOrganizationCache());
        return await controller.CreateOutlinedRoadSegmentV2(validator, _roadNetworkIdGenerator.Object, request, CancellationToken.None);
    }

    [Fact]
    public async Task ValidRequest_AcceptedResult()
    {
        const double length = 10;

        var result = await GetResultAsync(new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Wegsegmentstatus = RoadSegmentStatusV2.Gepland.ToDutchString(),
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ],
            Wegverharding =
            [
                new WegsegmentWegverhardingAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString()
                }
            ],
            Toegang =
            [
                new WegsegmentToegangAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Toegang = RoadSegmentAccessRestrictionV2.OpenbareWeg.ToDutchString()
                }
            ],
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = WegsegmentKant.Beide,
                    VanPositie = 0,
                    TotPositie = length,
                    Identificator = "https://data.vlaanderen.be/id/straatnaam/71671"
                }
            ],
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = WegsegmentKant.Beide,
                    VanPositie = 0,
                    TotPositie = length,
                    Wegbeheerder = "AWV"
                }
            ],
            Wegcategorie =
            [
                new WegsegmentWegcategorieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Wegcategorie = RoadSegmentCategoryV2.RegionaleWeg.ToDutchString()
                }
            ],
            VerkeerstypeAuto =
            [
                new WegsegmentVerkeerstypeAutoAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.Forward.ToDutchString()
                }
            ],
            VerkeerstypeFiets =
            [
                new WegsegmentVerkeerstypeFietsAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.Both.ToDutchString()
                }
            ],
            VerkeerstypeVoetganger =
            [
                new WegsegmentVerkeerstypeVoetgangerAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = length,
                    Richting = RoadSegmentTrafficDirection.None.ToDutchString()
                }
            ]
        });

        Assert.IsType<AcceptedResult>(result);
    }
}
