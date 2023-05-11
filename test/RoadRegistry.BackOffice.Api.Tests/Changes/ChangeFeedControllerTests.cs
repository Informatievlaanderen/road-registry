namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using Abstractions;
using Api.Changes;
using Editor.Schema.RoadNetworkChanges;
using Infrastructure.Containers;
using MediatR;
using Microsoft.Data.SqlClient;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;

[Collection(nameof(SqlServerCollection))]
public partial class ChangeFeedControllerTests : ControllerMinimalTests<ChangeFeedController>
{
    private readonly SqlServer _fixture;

    public ChangeFeedControllerTests(SqlServer fixture, IMediator mediator) : base(mediator)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }
}
