namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.Tests;
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
                }
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
    }

    protected override async Task<bool> VerifyTicketAsync()
    {
        var rejectCommand = await Store.GetLastCommandIfTypeIs<RoadNetworkChangesRejected>();
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

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        var @event = command.Changes.Single().RoadSegmentAttributesModified;
        var change = Request.ChangeRequests.Single();
        return @event.Id == roadSegmentId
               && @event.AccessRestriction == change.AccessRestriction
               && @event.Category == change.Category
               && @event.MaintenanceAuthority.Code == change.MaintenanceAuthority
               && @event.Morphology == change.Morphology
               && @event.Status == change.Status
               ;
    }
}
