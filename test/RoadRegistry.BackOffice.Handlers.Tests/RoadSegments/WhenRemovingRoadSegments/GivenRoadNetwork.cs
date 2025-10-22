namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments.WhenRemovingRoadSegments;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Framework;
using Editor.Schema;
using Editor.Schema.RoadSegments;
using FluentAssertions;
using Handlers.RoadSegments;
using Messages;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.ValueObjects;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class GivenRoadNetwork: RoadNetworkTestBase
{
    public GivenRoadNetwork(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenChangeRoadNetworkCommand()
    {
        // Arrange
        var request = new DeleteRoadSegmentsRequest([
            new RoadSegmentId(TestData.Segment1Added.Id), new RoadSegmentId(TestData.Segment2Added.Id), new RoadSegmentId(TestData.Segment3Added.Id)
        ])
        {
            ProvenanceData = new RoadRegistryProvenanceData()
        };

        var editorContext = BuildEditorContext();
        editorContext.RoadSegments.Add(TestData.Segment1Added.ToEditorRoadSegmentRecord(r =>
        {
            r.MethodId = RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier;
        }));
        editorContext.RoadSegments.Add(TestData.Segment2Added.ToEditorRoadSegmentRecord(r =>
        {
            r.MethodId = RoadSegmentGeometryDrawMethod.Measured.Translation.Identifier;
        }));
        editorContext.RoadSegments.Add(TestData.Segment3Added.ToEditorRoadSegmentRecord(r =>
        {
            r.MethodId = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
        }));
        await editorContext.SaveChangesAsync();

        var commandQueueMock = new Mock<IRoadNetworkCommandQueue>();
        var ticketingUrlMock = new Mock<ITicketingUrl>();
        ticketingUrlMock
            .Setup(x => x.For(It.IsAny<Guid>()))
            .Returns(new Uri("http://ticketing/1"));

        // Act
        var sut = new DeleteRoadSegmentsRequestHandler(
            commandQueueMock.Object,
            editorContext,
            new FakeOrganizationCache(),
            Mock.Of<ITicketing>(),
            ticketingUrlMock.Object,
            LoggerFactory
        );
        var response = await sut.Handle(request, CancellationToken.None);

        // Assert
        commandQueueMock.Verify(x =>
            x.WriteAsync(It.Is<Command>(c => c.Body is ChangeRoadNetwork
            && ((ChangeRoadNetwork)c.Body).Changes.Length == 2), It.IsAny<CancellationToken>()), Times.Once);

        response.TicketUrl.Should().Be("http://ticketing/1");
    }

    [Fact]
    public async Task WithNonExistingWegsegmenten_ThenException()
    {
        // Arrange
        var request = new DeleteRoadSegmentsRequest([new RoadSegmentId(TestData.Segment1Added.Id)]);

        var editorContext = BuildEditorContext();

        // Act
        var sut = new DeleteRoadSegmentsRequestHandler(
            Mock.Of<IRoadNetworkCommandQueue>(),
            editorContext,
            new FakeOrganizationCache(),
            Mock.Of<ITicketing>(),
            Mock.Of<ITicketingUrl>(),
            LoggerFactory
        );
        var act = () => sut.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<RoadSegmentsNotFoundException>();
        ex.And.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(TestData.Segment1Added.Id)]);
    }

    private EditorContext BuildEditorContext()
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EditorContext(options);
    }
}

internal static class EditorExtensions
{
    public static RoadSegmentRecord ToEditorRoadSegmentRecord(this RoadSegmentAdded roadSegmentAdded, Action<RoadSegmentRecord> configure = null )
    {
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

        var record = new RoadSegmentRecord
            {
                Id = roadSegmentAdded.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                Geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry),
                Version = roadSegmentAdded.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = roadSegmentAdded.MaintenanceAuthority.Name,
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,
                DbaseRecord = [],
                ShapeRecordContent = []
            };
        configure?.Invoke(record);

        return record;
    }
}
