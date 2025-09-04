namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;
using Infrastructure;
using RoadRegistry.Editor.Schema;

public partial class InformationControllerTests : ControllerMinimalTests<InformationController>
{
    private readonly EditorContext _editorContext;
    private readonly DbContextBuilder _fixture;

    public InformationControllerTests(
        DbContextBuilder fixture,
        InformationController controller,
        EditorContext editorContext)
        : base(controller)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
    }
}
