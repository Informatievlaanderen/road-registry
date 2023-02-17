namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.WhenDeleteOutline.Fixtures;

using Abstractions.Fixtures;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice.Core;
using SqlStreamStore;
using AcceptedChange = Messages.AcceptedChange;

public class WhenDeleteOutlineWithValidRequestFixture : WhenDeleteOutlineFixture
{
    public WhenDeleteOutlineWithValidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IStreamStore streamStore,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, streamStore, roadNetworkCommandQueue, clock, options)
    {
        TestData = RoadNetworkTestHelpers.Create();
    }

    protected RoadNetworkTestHelpers TestData { get; }

    protected override DeleteRoadSegmentOutlineRequest Request => new(
        TestData.Segment1Added.Id
    );

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(new OrganizationId(Organisation.ToString())), new ImportedOrganization
        {
            Code = Organisation.ToString(),
            Name = Organisation.ToString(),
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined.ToString();
        TestData.Segment1Added.StartNodeId = 0;
        TestData.Segment1Added.EndNodeId = 0;

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

        await VerifyThatTicketHasCompleted(roadSegmentId);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        return command.Changes.Single().RoadSegmentRemoved.Id == roadSegmentId;
    }
}
