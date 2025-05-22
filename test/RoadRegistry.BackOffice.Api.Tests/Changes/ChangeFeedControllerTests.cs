namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using System;
using Api.Changes;
using Infrastructure;
using MediatR;

public partial class ChangeFeedControllerTests : ControllerMinimalTests<ChangeFeedController>
{
    private readonly DbContextBuilder _fixture;

    public ChangeFeedControllerTests(DbContextBuilder fixture, ChangeFeedController controller, IMediator mediator) : base(controller, mediator)
    {
        _fixture = fixture.ThrowIfNull();
    }
}
