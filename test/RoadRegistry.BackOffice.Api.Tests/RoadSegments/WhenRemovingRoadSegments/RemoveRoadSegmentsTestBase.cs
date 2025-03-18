namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenRemovingRoadSegments;

using Abstractions.RoadSegments;
using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Editor.Schema;
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

public abstract class RemoveRoadSegmentsTestBase
{
    protected readonly RoadNetworkTestData TestData;
    protected IFixture Fixture => TestData.ObjectProvider;

    private readonly Mock<IMediator> _mediator;
    private readonly EditorContext _editorContext;

    protected RemoveRoadSegmentsTestBase()
    {
        TestData = new RoadNetworkTestData();

        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<DeleteRoadSegmentsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<DeleteRoadSegmentsResponse>());
        _editorContext = new FakeEditorContextFactory().CreateDbContext();
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

    protected async Task<IActionResult> GetResultAsync(DeleteRoadSegmentsParameters parameters)
    {
        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.Delete(
            parameters,
            new DeleteRoadSegmentsParametersValidator(_editorContext),
            CancellationToken.None
        );
    }
}
