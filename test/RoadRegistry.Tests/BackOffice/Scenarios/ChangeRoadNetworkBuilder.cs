namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

public class ChangeRoadNetworkBuilder
{
    private readonly Fixture _fixture;
    private readonly List<RequestedChange> _changes = [];

    private string _requestId;
    private string _reason;
    private string _operator;
    private string _organizationId;

    public ChangeRoadNetworkBuilder(Fixture fixture)
    {
        _fixture = fixture;

        _requestId = ChangeRequestId.FromArchiveId(_fixture.Create<ArchiveId>());
        _reason = _fixture.Create<Reason>();
        _operator = _fixture.Create<OperatorName>();
        _organizationId = _fixture.Create<OrganizationId>();
    }

    public ChangeRoadNetworkBuilder(RoadNetworkTestData testData)
        : this(testData.ObjectProvider)
    {
        _requestId = testData.RequestId;
        _reason = testData.ReasonForChange;
        _operator = testData.ChangedByOperator;
        _organizationId = testData.ChangedByOrganization;
    }

    public ChangeRoadNetworkBuilder WithModifyRoadSegment(ModifyRoadSegment change, Action<ModifyRoadSegment> configure = null)
    {
        configure?.Invoke(change);

        _changes.Add(new RequestedChange
        {
            ModifyRoadSegment = change
        });

        return this;
    }

    public ChangeRoadNetworkBuilder WithModifyOutlinedRoadSegment(ModifyRoadSegment change, Action<ModifyRoadSegment> configure = null)
    {
        change.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;
        change.StartNodeId = 0;
        change.EndNodeId = 0;

        return WithModifyRoadSegment(change, configure);
    }

    public ChangeRoadNetworkBuilder WithRemoveRoadSegments(ICollection<int> ids, RoadSegmentGeometryDrawMethod? drawMethod = null)
    {
        _changes.Add(new RequestedChange
        {
            RemoveRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = drawMethod ?? RoadSegmentGeometryDrawMethod.Measured,
                Ids = ids.ToArray()
            }
        });

        return this;
    }

    public Command Build()
    {
        return new Command(new ChangeRoadNetwork
        {
            RequestId = _requestId,
            Reason = _reason,
            Operator = _operator,
            OrganizationId = _organizationId,
            Changes = _changes.ToArray()
        });
    }
}
