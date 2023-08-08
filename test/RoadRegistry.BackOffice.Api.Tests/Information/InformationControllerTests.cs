namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;
using Infrastructure;
using Infrastructure.Containers;
using MediatR;
using RoadRegistry.Editor.Schema;

[Collection(nameof(SqlServerCollection))]
public partial class InformationControllerTests : ControllerMinimalTests<InformationController>
{
    private readonly EditorContext _editorContext;
    private readonly SqlServer _fixture;
    private readonly CancellationTokenSource _tokenSource;

    public InformationControllerTests(
        SqlServer fixture,
        InformationController controller,
        EditorContext editorContext,
        IMediator mediator)
        : base(controller, mediator)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _tokenSource = new CancellationTokenSource();
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
    }
}
