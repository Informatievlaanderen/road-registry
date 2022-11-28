namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Core;
using MediatR;
using Messages;
using Newtonsoft.Json;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework.Testing;
using SqlStreamStore.Streams;
using System.Runtime;
using SqlStreamStore;
using Problem = Messages.Problem;
using ProblemParameter = Messages.ProblemParameter;
using RejectedChange = Messages.RejectedChange;

public class LinkRoadSegmentToStreetNameRequestHandlerTests : RoadNetworkScenarios
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
        AddSegment1.LeftSideStreetNameId = null;

        await SetInitialStoreState(new[]
        {
            new RecordedEvent(Organizations.ToStreamName(ChangedByOrganization), new ImportedOrganization
            {
                Code = ChangedByOrganization,
                Name = ChangedByOrganizationName,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }),
            new RecordedEvent(RoadNetworks.Stream, TheOperator.ChangesTheRoadNetwork(
                RequestId, ReasonForChange, ChangedByOperator, ChangedByOrganization,
                new RequestedChange
                {
                    AddRoadNode = AddStartNode1
                },
                new RequestedChange
                {
                    AddRoadNode = AddEndNode1
                },
                new RequestedChange
                {
                    AddRoadSegment = AddSegment1
                }
            ).Body)
        });

        //TODO-rik execute streamstore handlers
        
        var request = new LinkRoadSegmentToStreetNameRequest(AddSegment1.TemporaryId, 1, 0);

        var response = await _mediator.Send(request, CancellationToken.None);
    }
}
