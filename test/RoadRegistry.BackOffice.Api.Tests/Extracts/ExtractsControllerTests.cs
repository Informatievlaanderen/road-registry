namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Api.Extracts;
using Editor.Schema;
using Infrastructure;
using RoadRegistry.Extensions;

public partial class ExtractsControllerTests : ControllerMinimalTests<ExtractsController>, IAsyncLifetime
{
    private readonly DbContextBuilder _dbContextBuilderFixture;
    private EditorContext _editorContext;

    public ExtractsControllerTests(
        DbContextBuilder dbContextBuilderFixture,
        ExtractsController controller)
        : base(controller)
    {
        _dbContextBuilderFixture = dbContextBuilderFixture.ThrowIfNull();
    }

    public async Task DisposeAsync()
    {
        await _editorContext.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _editorContext = _dbContextBuilderFixture.CreateEditorContext();
    }
}
