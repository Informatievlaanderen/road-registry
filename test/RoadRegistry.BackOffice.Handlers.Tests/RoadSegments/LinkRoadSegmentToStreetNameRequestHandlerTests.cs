namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Framework;
using Core;
using MediatR;
using Messages;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using AcceptedChange = Messages.AcceptedChange;

public class LinkRoadSegmentToStreetNameRequestHandlerTests : RoadNetworkFixture
{
    private readonly IMediator _mediator;

    public LinkRoadSegmentToStreetNameRequestHandlerTests(IMediator mediator, IStreamStore store)
        : base(store)
    {
        _mediator = mediator;
    }

    [Fact]
    public async Task LinkRoadSegmentToStreetName_Succeeded()
    {
        Segment1Added.LeftSide.StreetNameId = null;

        await Given(Organizations.ToStreamName(ChangedByOrganization), new ImportedOrganization
        {
            Code = ChangedByOrganization,
            Name = ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = RequestId,
            Reason = ReasonForChange,
            Operator = ChangedByOperator,
            OrganizationId = ChangedByOrganization,
            Organization = ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = Segment1Added
                }
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        var request = new LinkRoadSegmentToStreetNameRequest(1, 1, 0);
        await _mediator.Send(request, CancellationToken.None);

        var command = await Store.GetLastCommand<ChangeRoadNetwork>();

        Assert.Equal(1, command!.Changes.Single().ModifyRoadSegment.LeftSideStreetNameId);
    }
}
