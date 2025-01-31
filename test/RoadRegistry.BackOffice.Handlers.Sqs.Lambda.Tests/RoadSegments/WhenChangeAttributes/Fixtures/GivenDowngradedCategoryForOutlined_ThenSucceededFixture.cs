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
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using AcceptedChange = Messages.AcceptedChange;

public class GivenDowngradedCategoryForOutlined_ThenSucceededFixture : WhenChangeAttributesFixture
{
    public GivenDowngradedCategoryForOutlined_ThenSucceededFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
    }

    protected override void CustomizeTestData(Fixture fixture)
    {
        base.CustomizeTestData(fixture);

        fixture.CustomizeRoadSegmentOutline();
    }

    protected override ChangeRoadSegmentAttributesRequest Request => new ChangeRoadSegmentAttributesRequest()
        .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
        {
            change.Category = RoadSegmentCategory.MainRoad;
        });

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.Category = RoadSegmentCategory.EuropeanMainRoad;

        await Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(TestData.Segment1Added.Id)), new RoadNetworkChangesAccepted
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
        var attributesModified = Assert.Single(command.Changes).RoadSegmentAttributesModified;

        var attributesModifiedIsCorrect = attributesModified.Id == roadSegmentId
                                          && attributesModified.Category == change.Category;

        return attributesModifiedIsCorrect;
    }
}
