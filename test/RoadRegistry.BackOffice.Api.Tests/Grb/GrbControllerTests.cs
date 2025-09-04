namespace RoadRegistry.BackOffice.Api.Tests.Grb;

using Api.Grb;
using AutoFixture;
using Editor.Schema;
using Infrastructure;
using RoadRegistry.Tests.BackOffice;

public partial class GrbControllerTests : ControllerMinimalTests<GrbController>, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private readonly DbContextBuilder _dbContextBuilderFixture;
    private EditorContext _editorContext;

    public GrbControllerTests(
        DbContextBuilder dbContextBuilderFixture,
        GrbController controller)
        : base(controller)
    {
        _dbContextBuilderFixture = dbContextBuilderFixture.ThrowIfNull();
        _fixture = new Fixture();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGeometry();
        _fixture.CustomizeMultiPolygon();
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
