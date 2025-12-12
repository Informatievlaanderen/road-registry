namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenRemovingRoadSegments;

using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadNetwork;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.RoadSegments;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.CommandHandling;
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class RemoveRoadSegmentsTestBase
{
    protected readonly RoadNetworkTestData TestData;
    protected IFixture Fixture => TestData.ObjectProvider;

    protected readonly Mock<IMediator> Mediator;

    protected RemoveRoadSegmentsTestBase()
    {
        TestData = new RoadNetworkTestData();

        Mediator = new Mock<IMediator>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RemoveRoadSegmentsSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());
    }

    protected async Task ItShouldHaveExpectedError(
        DeleteRoadSegmentsParameters request,
        string expectedErrorCode,
        string expectedErrorMessagePrefix)
    {
        Exception exception;
        try
        {
            await GetResultAsync(request);
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
        return err.TranslateToDutch();
    }

    protected async Task<IActionResult> GetResultAsync(DeleteRoadSegmentsParameters parameters)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), Mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.DeleteRoadSegments(
            parameters,
            new DeleteRoadSegmentsParametersValidator(),
            new UseDomainV2FeatureToggle(true),
            CancellationToken.None
        );
    }
}
