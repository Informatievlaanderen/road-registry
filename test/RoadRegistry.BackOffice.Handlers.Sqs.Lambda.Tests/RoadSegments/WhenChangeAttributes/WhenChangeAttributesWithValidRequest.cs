namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using Editor.Schema;
using Messages;
using Moq;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using TicketingService.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WhenChangeAttributesWithValidRequest //: WhenChangeAttributes<WhenChangeAttributesWithValidRequestFixture>
{
    private readonly RoadNetworkTestData _testData;
    private readonly Mock<ITicketing> _ticketingMock;
    private readonly IStreamStore _store;
    private Fixture ObjectProvider => _testData.ObjectProvider;
    private readonly FakeEditorContext _editorContext = new FakeEditorContextFactory().CreateDbContext();
    private Organisation Organisation { get; }

    public WhenChangeAttributesWithValidRequest()
    {
        _testData = new RoadNetworkTestData();
        _ticketingMock = new Mock<ITicketing>();
        _store = new InMemoryStreamStore();

        Organisation = ObjectProvider.Create<Organisation>();
    }

    [Fact]
    public async Task ItShouldSucceed()
    {
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(_testData.Segment1Added.Id), change =>
            {
                change.AccessRestriction = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentAccessRestriction.Parse(_testData.Segment1Added.AccessRestriction));
                change.Category = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentCategory.Parse(_testData.Segment1Added.Category));
                change.MaintenanceAuthority = _testData.ChangedByOrganization;
                change.Morphology = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentMorphology.Parse(_testData.Segment1Added.Morphology));
                change.Status = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentStatus.Parse(_testData.Segment1Added.Status));

                change.EuropeanRoads = ObjectProvider.CreateMany<EuropeanRoadNumber>(1).ToArray();
                change.NationalRoads = ObjectProvider.CreateMany<NationalRoadNumber>(1).ToArray();
                change.NumberedRoads = ObjectProvider.CreateMany<NumberedRoadNumber>(1)
                    .Select(number => new ChangeRoadSegmentNumberedRoadAttribute
                    {
                        Number = number,
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    })
                    .ToArray();
            });

        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(new FakeClock(NodaConstants.UnixEpoch).GetCurrentInstant())
        });

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = _testData.RequestId,
            Reason = _testData.ReasonForChange,
            Operator = _testData.ChangedByOperator,
            OrganizationId = _testData.ChangedByOrganization,
            Organization = _testData.ChangedByOrganizationName,
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = _testData.Segment1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = _testData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(request.ChangeRequests.Single().EuropeanRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = _testData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(request.ChangeRequests.Single().NationalRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = _testData.Segment1Added.Id,
                        Number = request.ChangeRequests.Single().NumberedRoads!.Single().Number,
                        Direction = ObjectProvider.CreateWhichIsDifferentThan(request.ChangeRequests.Single().NumberedRoads!.Single().Direction),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 2,
                        TemporaryAttributeId = 2,
                        SegmentId = _testData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(request.ChangeRequests.Single().NumberedRoads!.Single().Number),
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                }
            ],
            When = InstantPattern.ExtendedIso.Format(new FakeClock(NodaConstants.UnixEpoch).GetCurrentInstant())
        });

        await _editorContext.RoadSegments.AddAsync(_testData.Segment1Added.ToRoadSegmentRecord(_testData.ChangedByOrganization, new FakeClock(NodaConstants.UnixEpoch)));
        await _editorContext.SaveChangesAsync();

        var result = await VerifyTicketAsync(request);

        Assert.True(result);
    }

    protected async Task<bool> VerifyTicketAsync(ChangeRoadSegmentAttributesRequest request)
    {
        var rejectCommand = await _store.GetLastMessageIfTypeIs<RoadNetworkChangesRejected>();
        if (rejectCommand != null)
        {
            var problems = rejectCommand.Changes.SelectMany(change => change.Problems).ToArray();
            if (problems.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, problems.Select(x => x.ToString())));
            }
        }

        var roadSegmentId = new RoadSegmentId(_testData.Segment1Added.Id);

        VerifyThatTicketHasCompleted(new ChangeRoadSegmentAttributesResponse());

        var change = request.ChangeRequests.Single();

        var command = await _store.GetLastMessage<RoadNetworkChangesAccepted>();
        Assert.Equal(8, command.Changes.Length);

        var attributesModified = command.Changes[0].RoadSegmentAttributesModified;
        var attributesModifiedIsCorrect = attributesModified.Id == roadSegmentId
                                          && attributesModified.AccessRestriction == change.AccessRestriction
                                          && attributesModified.Category == change.Category
                                          && attributesModified.MaintenanceAuthority?.Code == change.MaintenanceAuthority
                                          && attributesModified.Morphology == change.Morphology
                                          && attributesModified.Status == change.Status;

        var europeanRoadsIsCorrect = command.Changes[1].RoadSegmentAddedToEuropeanRoad.Number == change.EuropeanRoads!.Single()
                                     && command.Changes[4].RoadSegmentRemovedFromEuropeanRoad.SegmentId == change.Id;
        var nationalRoadsIsCorrect = command.Changes[2].RoadSegmentAddedToNationalRoad.Number == change.NationalRoads!.Single()
                                     && command.Changes[5].RoadSegmentRemovedFromNationalRoad.SegmentId == change.Id;
        var numberedRoadsIsCorrect = command.Changes[3].RoadSegmentAddedToNumberedRoad.Number == change.NumberedRoads!.Single().Number
                                     && command.Changes[6].RoadSegmentRemovedFromNumberedRoad.SegmentId == change.Id
                                     && command.Changes[7].RoadSegmentRemovedFromNumberedRoad.SegmentId == change.Id;

        return attributesModifiedIsCorrect
            && europeanRoadsIsCorrect
            && nationalRoadsIsCorrect
            && numberedRoadsIsCorrect;
    }

    private void VerifyThatTicketHasCompleted(object response)
    {
        _ticketingMock.Verify(x =>
            x.Complete(
                It.IsAny<Guid>(),
                new TicketResult(response),
                CancellationToken.None
            )
        );
    }


    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings Settings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private static readonly StreamNameConverter StreamNameConverter = StreamNameConversions.PassThru;

    private Task Given(StreamName streamName, params object[] events)
    {
        return _store.Given(Mapping, Settings, StreamNameConverter, streamName, events);
    }
}
