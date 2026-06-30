namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Core;
using FluentAssertions;
using Messages;
using NodaTime.Text;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Extracts;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;
using AddRoadSegmentToEuropeanRoad = BackOffice.Uploads.AddRoadSegmentToEuropeanRoad;
using AddRoadSegmentToNationalRoad = BackOffice.Uploads.AddRoadSegmentToNationalRoad;
using AddRoadSegmentToNumberedRoad = BackOffice.Uploads.AddRoadSegmentToNumberedRoad;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using RemoveRoadSegmentFromEuropeanRoad = BackOffice.Uploads.RemoveRoadSegmentFromEuropeanRoad;
using RemoveRoadSegmentFromNationalRoad = BackOffice.Uploads.RemoveRoadSegmentFromNationalRoad;
using RemoveRoadSegmentFromNumberedRoad = BackOffice.Uploads.RemoveRoadSegmentFromNumberedRoad;

public class GivenInwinningRoadSegment : WhenChangeAttributesTestBase
{
    public GivenInwinningRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenError()
    {
        // Arrange
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(roadSegmentId, change =>
            {
                change.AccessRestriction = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentAccessRestriction.Parse(TestData.Segment1Added.AccessRestriction));
                change.Category = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentCategory.Parse(TestData.Segment1Added.Category));
                change.MaintenanceAuthority = TestData.ChangedByOrganization;
                change.Morphology = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentMorphology.Parse(TestData.Segment1Added.Morphology));
                change.Status = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentStatus.Parse(TestData.Segment1Added.Status));
                change.LeftSideStreetNameId = new StreetNameLocalId(WellKnownStreetNameIds.Proposed);
                change.RightSideStreetNameId = StreetNameLocalId.NotApplicable;

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
        var change = request.ChangeRequests.Single();

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.LeftSide.StreetNameId = null;
        TestData.Segment1Added.RightSide.StreetNameId = null;

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.EuropeanRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.NationalRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = change.NumberedRoads!.Single().Number,
                        Direction = ObjectProvider.CreateWhichIsDifferentThan(change.NumberedRoads!.Single().Direction),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 2,
                        TemporaryAttributeId = 2,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.NumberedRoads!.Single().Number),
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();

        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            Completed = ObjectProvider.Create<bool>()
        });
        await extractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request, extractsDbContext: extractsDbContext);

        // Assert
        VerifyThatTicketHasError("RoadSegmentIsInInwinning", $"Het wegsegment met id {roadSegmentId} heeft de inwinningsstatus 'locked' of 'compleet'.");
    }
}
