namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Hosts;
using Messages;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Extracts;
using AcceptedChange = Messages.AcceptedChange;

public class WhenChangeAttributesWithOnlyEuropeanRoadsFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithOnlyEuropeanRoadsFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.EuropeanRoads = ObjectProvider.CreateMany<EuropeanRoadNumber>(1).ToArray();
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
            Changes = new[]
            {
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
                }
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
;
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

        VerifyThatTicketHasCompleted(new ChangeRoadSegmentAttributesResponse());

        var change = Request.ChangeRequests.Single();

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        Assert.Equal(2, command.Changes.Length);
        
        var europeanRoadsIsCorrect = command.Changes[0].RoadSegmentAddedToEuropeanRoad.Number == change.EuropeanRoads!.Single()
                                     && command.Changes[1].RoadSegmentRemovedFromEuropeanRoad.SegmentId == change.Id;
        
        return europeanRoadsIsCorrect;
    }
}
