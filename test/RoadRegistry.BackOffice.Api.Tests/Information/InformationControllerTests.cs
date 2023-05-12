namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;
using Infrastructure;
using Infrastructure.Containers;
using MediatR;

[Collection(nameof(SqlServerCollection))]
public partial class InformationControllerTests : ControllerMinimalTests<InformationController>
{
    private readonly SqlServer _fixture;

    public InformationControllerTests(SqlServer fixture, IMediator mediator) : base(mediator)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }
}