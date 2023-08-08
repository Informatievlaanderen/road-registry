namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using Api.Changes;
using Infrastructure;
using Infrastructure.Containers;
using MediatR;

[Collection(nameof(SqlServerCollection))]
public partial class ChangeFeedControllerTests : ControllerMinimalTests<ChangeFeedController>
{
    private readonly SqlServer _fixture;

    public ChangeFeedControllerTests(SqlServer fixture, ChangeFeedController controller, IMediator mediator) : base(controller, mediator)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }
}
