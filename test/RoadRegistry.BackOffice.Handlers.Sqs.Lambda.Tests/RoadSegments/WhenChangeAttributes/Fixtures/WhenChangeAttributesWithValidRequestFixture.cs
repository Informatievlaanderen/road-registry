namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Messages;
using Microsoft.Extensions.Configuration;
using Moq;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Extracts;
using TicketingService.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WhenChangeAttributesWithValidRequestFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithValidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.AccessRestriction = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentAccessRestriction.Parse(TestData.Segment1Added.AccessRestriction));
                change.Category = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentCategory.Parse(TestData.Segment1Added.Category));
                change.MaintenanceAuthority = TestData.ChangedByOrganization;
                change.Morphology = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentMorphology.Parse(TestData.Segment1Added.Morphology));
                change.Status = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentStatus.Parse(TestData.Segment1Added.Status));

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
    }

    protected override ChangeRoadSegmentAttributesRequest Request { get; }

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

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
                        Number = ObjectProvider.CreateWhichIsDifferentThan(Request.ChangeRequests.Single().EuropeanRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(Request.ChangeRequests.Single().NationalRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = Request.ChangeRequests.Single().NumberedRoads!.Single().Number,
                        Direction = ObjectProvider.CreateWhichIsDifferentThan(Request.ChangeRequests.Single().NumberedRoads!.Single().Direction),
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
                        Number = ObjectProvider.CreateWhichIsDifferentThan(Request.ChangeRequests.Single().NumberedRoads!.Single().Number),
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();
    }

    protected override async Task<bool> VerifyTicketAsync()
    {
        var rejectCommand = await Store.GetLastMessageIfTypeIs<RoadNetworkChangesRejected>();
        if (rejectCommand != null)
        {
            var problems = rejectCommand.Changes.SelectMany(change => change.Problems).ToArray();
            if (problems.Any())
            {
                throw new Exception(string.Join(Environment.NewLine, problems.Select(x => x.ToString())));
            }
        }

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        VerifyThatTicketHasCompleted(new ChangeRoadSegmentAttributesResponse());

        var change = Request.ChangeRequests.Single();

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
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
}
