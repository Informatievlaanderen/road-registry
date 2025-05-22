namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Api.Extracts;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Editor.Schema;
using Infrastructure;
using MediatR;
using SqlStreamStore;

public partial class ExtractsControllerTests : ControllerTests<ExtractsController>, IAsyncLifetime
{
    private readonly DbContextBuilder _dbContextBuilderFixture;
    private EditorContext _editorContext;

    public ExtractsControllerTests(
        DbContextBuilder dbContextBuilderFixture,
        ExtractsController controller,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(controller, mediator, streamStore, uploadClient, extractUploadClient)
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
