namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Infrastructure.Messages;

public class RoadNetworkChangesAcceptedBuilder
{
    private readonly List<AcceptedChange> _acceptedChanges = [];

    private readonly string _requestId;
    private readonly string _reasonForChange;
    private readonly string _operator;
    private readonly string _organizationId;
    private readonly string _organizationName;
    private TransactionId _transactionId;
    private string _when;

    private RoadNetworkChangesAcceptedBuilder(Fixture fixture)
    {
        _requestId = ChangeRequestId.FromArchiveId(fixture.Create<ArchiveId>());
        _reasonForChange = fixture.Create<Reason>();
        _operator = fixture.Create<OperatorName>();
        _organizationId = fixture.Create<OrganizationId>();
        _organizationName = fixture.Create<OrganizationName>();
        WithTransactionId(1);
        WithClock(SystemClock.Instance);
    }

    public RoadNetworkChangesAcceptedBuilder(RoadNetworkTestData testData)
        : this(testData.ObjectProvider)
    {
        _requestId = testData.RequestId;
        _reasonForChange = testData.ReasonForChange;
        _operator = testData.ChangedByOperator;
        _organizationId = testData.ChangedByOrganization;
        _organizationName = testData.ChangedByOrganizationName;
    }

    public RoadNetworkChangesAcceptedBuilder WithTransactionId(int transactionId)
    {
        _transactionId = new TransactionId(transactionId);
        return this;
    }
    public RoadNetworkChangesAcceptedBuilder WithClock(IClock clock)
    {
        return WithWhen(InstantPattern.ExtendedIso.Format(clock.GetCurrentInstant()));
    }
    public RoadNetworkChangesAcceptedBuilder WithWhen(string when)
    {
        _when = when;
        return this;
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadNodeAdded(
        RoadNodeAdded roadNodeAdded,
        Action<RoadNodeAdded> configure = null)
    {
        configure?.Invoke(roadNodeAdded);

        return WithChange(new AcceptedChange
        {
            RoadNodeAdded = roadNodeAdded
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithOutlinedRoadSegment(
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentAdded> configure = null)
    {
        roadSegmentAdded.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        roadSegmentAdded.StartNodeId = 0;
        roadSegmentAdded.EndNodeId = 0;

        return WithRoadSegmentAdded(roadSegmentAdded, configure);
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentAdded(
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentAdded> configure = null)
    {
        configure?.Invoke(roadSegmentAdded);

        return WithChange(new AcceptedChange
        {
            RoadSegmentAdded = roadSegmentAdded
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentRemoved(int id,
        RoadSegmentGeometryDrawMethod? geometryDrawMethod = null)
    {
        return WithChange(new AcceptedChange
        {
            RoadSegmentRemoved = new()
            {
                GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethod.Measured,
                Id = id
            }
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadNodeRemoved(int id)
    {
        return WithChange(new AcceptedChange
        {
            RoadNodeRemoved = new()
            {
                Id = id
            }
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadNodeModified(RoadNodeModified roadNodeModified)
    {
        return WithChange(new AcceptedChange
        {
            RoadNodeModified = roadNodeModified
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithOutlinedRoadSegmentModified(
        RoadSegmentModified roadSegment,
        Action<RoadSegmentModified> configure = null)
    {
        roadSegment.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        roadSegment.StartNodeId = 0;
        roadSegment.EndNodeId = 0;

        return WithRoadSegmentModified(roadSegment, configure);
    }
    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentModified(
        RoadSegmentModified roadSegment,
        Action<RoadSegmentModified> configure = null,
        Problem[] problems = null)
    {
        configure?.Invoke(roadSegment);

        return WithChange(new AcceptedChange
        {
            RoadSegmentModified = roadSegment,
            Problems = problems ?? []
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithGradeSeparatedJunctionAdded(
        GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded)
    {
        return WithChange(new AcceptedChange
        {
            GradeSeparatedJunctionAdded = gradeSeparatedJunctionAdded
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithGradeSeparatedJunctionRemoved(int id)
    {
        return WithChange(new AcceptedChange
        {
            GradeSeparatedJunctionRemoved = new()
            {
                Id = id
            }
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithGradeSeparatedJunctionModified(GradeSeparatedJunctionModified change)
    {
        return WithChange(new AcceptedChange
        {
            GradeSeparatedJunctionModified = change
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentAddedToEuropeanRoad(RoadSegmentAddedToEuropeanRoad change)
    {
        return WithChange(new AcceptedChange
        {
            RoadSegmentAddedToEuropeanRoad = change
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentAddedToNationalRoad(RoadSegmentAddedToNationalRoad change)
    {
        return WithChange(new AcceptedChange
        {
            RoadSegmentAddedToNationalRoad = change
        });
    }

    public RoadNetworkChangesAcceptedBuilder WithRoadSegmentAddedToNumberedRoad(RoadSegmentAddedToNumberedRoad change)
    {
        return WithChange(new AcceptedChange
        {
            RoadSegmentAddedToNumberedRoad = change
        });
    }

    public RoadNetworkChangesAccepted Build()
    {
        return new RoadNetworkChangesAccepted
        {
            RequestId = _requestId,
            Reason = _reasonForChange,
            Operator = _operator,
            OrganizationId = _organizationId,
            Organization = _organizationName,
            TransactionId = _transactionId,
            Changes = _acceptedChanges.ToArray(),
            When = _when
        };
    }

    private RoadNetworkChangesAcceptedBuilder WithChange(AcceptedChange acceptedChange)
    {
        acceptedChange.Problems ??= [];
        _acceptedChanges.Add(acceptedChange);

        return this;
    }
}
