namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using System.Collections.Generic;
using System.Linq;
using Abstractions.RoadSegments;
using Api.Infrastructure.Controllers;
using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Editor.Schema;
using Editor.Schema.Organizations;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework.Projections;

public abstract class ChangeAttributesTestBase
{
    protected readonly FakeOrganizationCache OrganizationCache;
    protected readonly RoadNetworkTestData TestData;
    protected IFixture Fixture => TestData.ObjectProvider;

    private readonly Mock<IMediator> _mediator;
    private readonly EditorContext _editorContext;

    protected ChangeAttributesTestBase()
    {
        TestData = new RoadNetworkTestData(x =>
        {
            x.CustomizeRoadSegmentOutlineMorphology();
            x.CustomizeRoadSegmentOutlineStatus();
        });

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentAttributesSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());
        _editorContext = new FakeEditorContextFactory().CreateDbContext();
        OrganizationCache = new FakeOrganizationCache();
    }

    protected async Task ItShouldHaveExpectedError(
        ChangeRoadSegmentAttributesParameters request,
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

    protected async Task GivenRoadNetwork()
    {
        await _editorContext.AddOrganization(TestData.ChangedByOrganization, TestData.ChangedByOrganizationName);

        var roadNetworkChangesAccepted = Fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(TestData.Segment1Added);

        await _editorContext.RoadSegments
            .AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, roadNetworkChangesAccepted.When));
        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }

    protected async Task<IActionResult> GetResultAsync(ChangeRoadSegmentAttributesParameters parameters)
    {
        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.ChangeAttributes(
            parameters,
            new ChangeRoadSegmentAttributesParametersValidator(),
            new ChangeRoadSegmentAttributesParametersWrapperValidator(_editorContext, OrganizationCache),
            CancellationToken.None
        );
    }
}
