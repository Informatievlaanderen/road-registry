namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

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

public abstract class CreateOutlineTestBase
{
    protected readonly RoadNetworkTestData TestData;
    protected IFixture ObjectProvider => TestData.ObjectProvider;

    private readonly Mock<IMediator> _mediator;

    protected CreateOutlineTestBase()
    {
        TestData = new RoadNetworkTestData();
        ObjectProvider.CustomizeRoadSegmentOutline();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<CreateRoadSegmentOutlineSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ObjectProvider.Create<LocationResult>());
    }

    protected PostRoadSegmentOutlineParameters CreateValidRequest()
    {
        return new PostRoadSegmentOutlineParameters
        {
            MiddellijnGeometrie = GeometryTranslatorTestCases.ValidGmlLineString,
            Wegsegmentstatus = ObjectProvider.Create<RoadSegmentStatus>().ToDutchString(),
            MorfologischeWegklasse = ObjectProvider.Create<RoadSegmentMorphology>().ToDutchString(),
            Toegangsbeperking = ObjectProvider.Create<RoadSegmentAccessRestriction>().ToDutchString(),
            Wegbeheerder = "TEST",
            Wegverharding = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString(),
            Wegbreedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString(),
            AantalRijstroken = new RoadSegmentLaneParameters
            {
                Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
            }
        };
    }

    protected async Task<IActionResult> GetResultAsync(PostRoadSegmentOutlineParameters request, IOrganizationCache organizationCache = null)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return await controller.CreateOutline(
            new PostRoadSegmentOutlineParametersValidator(organizationCache ?? new FakeOrganizationCache()),
            request,
            CancellationToken.None
        );
    }

    protected async Task ItShouldHaveExpectedError(
        PostRoadSegmentOutlineParameters request,
        string expectedErrorCode,
        string expectedErrorMessagePrefix = null,
        IOrganizationCache organizationCache = null)
    {
        Exception exception;
        try
        {
            await GetResultAsync(request, organizationCache);
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
