namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;

public class RoadNetworkChangesAcceptedBuilder
{
    private readonly Fixture _fixture;
    private readonly List<AcceptedChange> _acceptedChanges = [];

    private string _requestId;
    private string _reasonForChange;
    private string _operator;
    private string _organizationId;
    private string _organizationName;
    private TransactionId _transactionId;
    private string _when;

    private RoadNetworkChangesAcceptedBuilder(Fixture fixture)
    {
        _fixture = fixture;

        _requestId = ChangeRequestId.FromArchiveId(_fixture.Create<ArchiveId>());
        _reasonForChange = _fixture.Create<Reason>();
        _operator = _fixture.Create<OperatorName>();
        _organizationId = _fixture.Create<OrganizationId>();
        _organizationName = _fixture.Create<OrganizationName>();
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
