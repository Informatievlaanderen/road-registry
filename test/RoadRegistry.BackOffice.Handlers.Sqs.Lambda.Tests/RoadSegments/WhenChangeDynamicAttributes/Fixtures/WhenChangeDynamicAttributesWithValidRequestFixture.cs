namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes.Fixtures;

using Abstractions.Fixtures;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice.Extracts;
using AcceptedChange = Messages.AcceptedChange;

public class WhenChangeDynamicAttributesWithValidRequestFixture : WhenChangeDynamicAttributesFixture
{
    public WhenChangeDynamicAttributesWithValidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Request = new ChangeRoadSegmentsDynamicAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                var geometryLength = GeometryTranslator.Translate(TestData.Segment1Added.Geometry).Length;
                var lastToPosition = RoadSegmentPosition.FromDouble(Math.Round(geometryLength, 3));

                var lanePositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Lanes = lanePositions
                    .Select((x, index) => new ChangeRoadSegmentLaneAttributeRequest
                    {
                        FromPosition = x.From,
                        ToPosition = index != lanePositions.Length - 1 ? x.To : lastToPosition,
                        Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
                    }).ToArray();

                var surfacePositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Surfaces = surfacePositions
                    .Select((x, index) => new ChangeRoadSegmentSurfaceAttributeRequest
                    {
                        FromPosition = x.From,
                        ToPosition = index != surfacePositions.Length - 1 ? x.To : lastToPosition,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
                    }).ToArray();

                var widthPositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Widths = widthPositions
                    .Select((x, index) => new ChangeRoadSegmentWidthAttributeRequest
                    {
                        FromPosition = x.From,
                        ToPosition = index != lanePositions.Length - 1 ? x.To : lastToPosition,
                        Width = ObjectProvider.Create<RoadSegmentWidth>()
                    }).ToArray();
            });
    }

    protected override ChangeRoadSegmentsDynamicAttributesRequest Request { get; }

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

        VerifyThatTicketHasCompleted(new ChangeRoadSegmentsDynamicAttributesResponse());

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        var @event = command.Changes.Single().RoadSegmentAttributesModified;
        var change = Request.ChangeRequests.Single();
        return @event.Id == roadSegmentId
               && @event.Lanes.EqualsCollection(change.Lanes, (x1, x2) =>
                   x1.FromPosition == x2.FromPosition
                   && x1.ToPosition == x2.ToPosition
                   && x1.Count == x2.Count
                   && x1.Direction == x2.Direction
               )
               && @event.Surfaces.EqualsCollection(change.Surfaces, (x1, x2) =>
                   x1.FromPosition == x2.FromPosition
                   && x1.ToPosition == x2.ToPosition
                   && x1.Type == x2.Type
               )
               && @event.Widths.EqualsCollection(change.Widths, (x1, x2) =>
                   x1.FromPosition == x2.FromPosition
                   && x1.ToPosition == x2.ToPosition
                   && x1.Width == x2.Width
               )
               ;
    }
}
