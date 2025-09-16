namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

public class ChangeRoadNetworkBuilder
{
    private readonly List<RequestedChange> _changes = [];

    private string _requestId;
    private string _reason;
    private string _operator;
    private string _organizationId;
    private TicketId? _ticketId;
    private ExtractRequestId? _extractRequestId;
    private DownloadId? _downloadId;

    public ChangeRoadNetworkBuilder(Fixture fixture)
    {
        _requestId = ChangeRequestId.FromArchiveId(fixture.Create<ArchiveId>());
        _reason = fixture.Create<Reason>();
        _operator = fixture.Create<OperatorName>();
        _organizationId = fixture.Create<OrganizationId>();
    }

    public ChangeRoadNetworkBuilder(RoadNetworkTestData testData)
        : this(testData.ObjectProvider)
    {
        _requestId = testData.RequestId;
        _reason = testData.ReasonForChange;
        _operator = testData.ChangedByOperator;
        _organizationId = testData.ChangedByOrganization;
    }

    public ChangeRoadNetworkBuilder WithAddRoadNode(AddRoadNode change, Action<AddRoadNode> configure = null)
    {
        configure?.Invoke(change);

        _changes.Add(new RequestedChange
        {
            AddRoadNode = change
        });

        return this;
    }

    public ChangeRoadNetworkBuilder WithModifyRoadNode(ModifyRoadNode change, Action<ModifyRoadNode> configure = null)
    {
        configure?.Invoke(change);

        _changes.Add(new RequestedChange
        {
            ModifyRoadNode = change
        });

        return this;
    }

    public ChangeRoadNetworkBuilder WithAddRoadSegment(AddRoadSegment change, Action<AddRoadSegment> configure = null)
    {
        configure?.Invoke(change);

        _changes.Add(new RequestedChange
        {
            AddRoadSegment = change
        });

        return this;
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

    public ChangeRoadNetworkBuilder WithRemoveOutlinedRoadSegment(RemoveOutlinedRoadSegment change, Action<RemoveOutlinedRoadSegment> configure = null)
    {
        configure?.Invoke(change);

        _changes.Add(new RequestedChange
        {
            RemoveOutlinedRoadSegment = change
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

    public ChangeRoadNetworkBuilder WithTicketId(TicketId ticketId)
    {
        _ticketId = ticketId;

        return this;
    }

    public ChangeRoadNetworkBuilder WithExtractRequestId(ExtractRequestId extractRequestId)
    {
        _extractRequestId = extractRequestId;

        return this;
    }

    public ChangeRoadNetworkBuilder WithDownloadId(DownloadId downloadId)
    {
        _downloadId = downloadId;

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
            TicketId = _ticketId,
            ExtractRequestId = _extractRequestId,
            Changes = _changes.ToArray(),
            DownloadId = _downloadId
        });
    }
}
