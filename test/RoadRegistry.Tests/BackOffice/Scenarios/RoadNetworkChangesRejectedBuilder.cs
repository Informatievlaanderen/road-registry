namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RejectedChange = RoadRegistry.BackOffice.Messages.RejectedChange;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class RoadNetworkChangesRejectedBuilder
{
    private readonly List<RejectedChange> _rejectedChanges = [];

    private readonly string _requestId;
    private readonly string _reasonForChange;
    private readonly string _operator;
    private readonly string _organizationId;
    private readonly string _organizationName;
    private TransactionId _transactionId;
    private string _when;

    private RoadNetworkChangesRejectedBuilder(Fixture fixture)
    {
        _requestId = ChangeRequestId.FromArchiveId(fixture.Create<ArchiveId>());
        _reasonForChange = fixture.Create<Reason>();
        _operator = fixture.Create<OperatorName>();
        _organizationId = fixture.Create<OrganizationId>();
        _organizationName = fixture.Create<OrganizationName>();
        WithTransactionId(1);
        WithClock(SystemClock.Instance);
    }

    public RoadNetworkChangesRejectedBuilder(RoadNetworkTestData testData)
        : this(testData.ObjectProvider)
    {
        _requestId = testData.RequestId;
        _reasonForChange = testData.ReasonForChange;
        _operator = testData.ChangedByOperator;
        _organizationId = testData.ChangedByOrganization;
        _organizationName = testData.ChangedByOrganizationName;
    }

    public RoadNetworkChangesRejectedBuilder WithTransactionId(int transactionId)
    {
        _transactionId = new TransactionId(transactionId);
        return this;
    }
    public RoadNetworkChangesRejectedBuilder WithClock(IClock clock)
    {
        return WithWhen(InstantPattern.ExtendedIso.Format(clock.GetCurrentInstant()));
    }
    public RoadNetworkChangesRejectedBuilder WithWhen(string when)
    {
        _when = when;
        return this;
    }

    public RoadNetworkChangesRejectedBuilder WithRemoveRoadSegments(RemoveRoadSegments change, Problem[] problems)
    {
        return WithChange(new RejectedChange
        {
            RemoveRoadSegments = change,
            Problems = problems
        });
    }

    public RoadNetworkChangesRejected Build()
    {
        return new RoadNetworkChangesRejected
        {
            RequestId = _requestId,
            Reason = _reasonForChange,
            Operator = _operator,
            OrganizationId = _organizationId,
            Organization = _organizationName,
            TransactionId = _transactionId,
            Changes = _rejectedChanges.ToArray(),
            When = _when
        };
    }

    private RoadNetworkChangesRejectedBuilder WithChange(RejectedChange acceptedChange)
    {
        acceptedChange.Problems ??= [];
        _rejectedChanges.Add(acceptedChange);

        return this;
    }
}
