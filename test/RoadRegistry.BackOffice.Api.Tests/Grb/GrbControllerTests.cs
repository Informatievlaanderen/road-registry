namespace RoadRegistry.BackOffice.Api.Tests.Grb;

using Api.Grb;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Editor.Schema;
using Infrastructure;
using MediatR;
using RoadRegistry.Tests.BackOffice;
using SqlStreamStore;

public partial class GrbControllerTests : ControllerTests<GrbController>, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private readonly DbContextBuilder _dbContextBuilderFixture;
    private EditorContext _editorContext;

    public GrbControllerTests(
        DbContextBuilder dbContextBuilderFixture,
        GrbController controller,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(controller, mediator, streamStore, uploadClient, extractUploadClient)
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
