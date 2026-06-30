namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using System.Collections.Generic;
using System.Linq;
using Api.Infrastructure.Controllers;
using Api.RoadSegments;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Editor.Schema;
using Editor.Schema.Organizations;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework.Projections;

public abstract class ChangeDynamicAttributesTestBase
{
    protected readonly RoadNetworkTestData TestData;
    protected IFixture ObjectProvider => TestData.ObjectProvider;

    private readonly Mock<IMediator> _mediator;
    private readonly EditorContext _editorContext;

    protected ChangeDynamicAttributesTestBase()
    {
        TestData = new RoadNetworkTestData();
        ObjectProvider.CustomizeRoadSegmentOutlineLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentOutlineWidth();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentsDynamicAttributesSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ObjectProvider.Create<LocationResult>());

        _editorContext = new FakeEditorContextFactory().CreateDbContext();
    }

    protected async Task GivenRoadNetwork()
    {
        await _editorContext.AddOrganization(TestData.ChangedByOrganization, TestData.ChangedByOrganizationName);

        var message = ObjectProvider
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(TestData.Segment1Added);

        await _editorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, message.When));
        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }

    protected async Task<IActionResult> GetResultAsync(ChangeRoadSegmentsDynamicAttributesParameters parameters)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return await controller.ChangeDynamicAttributes(
            parameters,
            new ChangeRoadSegmentsDynamicAttributesParametersValidator(_editorContext),
            CancellationToken.None
        );
    }

    protected async Task ItShouldHaveExpectedError(
        ChangeRoadSegmentsDynamicAttributesParameters request,
        string expectedErrorCode,
        string expectedErrorMessagePrefix = null)
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
        return err.TranslateToDutch(WellKnownProblemTranslators.Default);
    }
}
