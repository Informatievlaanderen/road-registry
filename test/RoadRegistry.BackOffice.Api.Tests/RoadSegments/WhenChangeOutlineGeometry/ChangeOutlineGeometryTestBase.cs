namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

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

public abstract class ChangeOutlineGeometryTestBase
{
    protected readonly RoadNetworkTestData TestData;
    protected IFixture ObjectProvider => TestData.ObjectProvider;

    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IRoadSegmentRepository> _defaultRoadSegmentRepository;

    protected ChangeOutlineGeometryTestBase()
    {
        TestData = new RoadNetworkTestData();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentOutlineGeometrySqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ObjectProvider.Create<LocationResult>());

        _defaultRoadSegmentRepository = new Mock<IRoadSegmentRepository>();
        _defaultRoadSegmentRepository
            .Setup(x => x.FindAsync(It.IsAny<RoadSegmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoadSegmentRecord(new RoadSegmentId(TestData.Segment1Added.Id), RoadSegmentGeometryDrawMethod.Outlined, "hash"));
    }

    protected async Task<IActionResult> GetResultAsync(PostChangeOutlineGeometryParameters request, IRoadSegmentRepository roadSegmentRepository = null)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return await controller.ChangeOutlineGeometry(
            new RoadSegmentIdValidator(),
            new PostChangeOutlineGeometryParametersValidator(),
            roadSegmentRepository ?? _defaultRoadSegmentRepository.Object,
            request,
            TestData.Segment1Added.Id,
            CancellationToken.None
        );
    }

    protected async Task ItShouldHaveExpectedError(
        PostChangeOutlineGeometryParameters request,
        string expectedErrorCode,
        string expectedErrorMessagePrefix = null,
        IRoadSegmentRepository roadSegmentRepository = null)
    {
        Exception exception;
        try
        {
            await GetResultAsync(request, roadSegmentRepository);
            exception = null;
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var errors = ItShouldHaveValidationException(exception).ToArray();

        if (expectedErrorCode is not null)
        {
            Assert.Contains(expectedErrorCode, errors.Select(x => x.ErrorCode));
        }

        if (expectedErrorMessagePrefix is not null)
        {
            Assert.True(errors != null && errors.Any(x => x.ErrorMessage.StartsWith(expectedErrorMessagePrefix)));
        }
    }

    private static IEnumerable<ValidationFailure> ItShouldHaveValidationException(Exception exception)
    {
        var ex = Assert.IsType<ValidationException>(exception);
        var err = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(ex.Errors);
        return err.TranslateToDutch(WellKnownProblemTranslators.Default);
    }
}
