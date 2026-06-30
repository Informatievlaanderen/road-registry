namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline;

using System.Collections.Generic;
using System.Linq;
using Api.Infrastructure.Controllers;
using Api.RoadSegments;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class DeleteOutlineTests
{
    private readonly RoadNetworkTestData _testData;
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IRoadSegmentRepository> _roadSegmentRepository;

    public DeleteOutlineTests()
    {
        _testData = new RoadNetworkTestData();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<DeleteRoadSegmentOutlineSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testData.ObjectProvider.Create<LocationResult>());

        _roadSegmentRepository = new Mock<IRoadSegmentRepository>();
        _roadSegmentRepository
            .Setup(x => x.FindAsync(It.IsAny<RoadSegmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoadSegmentRecord(new RoadSegmentId(_testData.Segment1Added.Id), RoadSegmentGeometryDrawMethod.Outlined, "hash"));
    }

    private async Task<IActionResult> GetResultAsync(int roadSegmentId)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return await controller.DeleteOutline(
            new RoadSegmentIdValidator(),
            _roadSegmentRepository.Object,
            roadSegmentId,
            CancellationToken.None
        );
    }

    [Fact]
    public async Task ValidRequest_AcceptedResult()
    {
        var result = await GetResultAsync(_testData.Segment1Added.Id);
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task InvalidId_IncorrectObjectId()
    {
        Exception exception;
        try
        {
            await GetResultAsync(0);
            exception = null;
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var validationEx = Assert.IsType<ValidationException>(exception);
        var errors = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(validationEx.Errors)
            .TranslateToDutch(WellKnownProblemTranslators.Default)
            .ToArray();

        Assert.Contains("IncorrectObjectId", errors.Select(x => x.ErrorCode));
        Assert.Contains(errors, x => x.ErrorMessage.StartsWith("De waarde '0' is ongeldig."));
    }
}
