namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using MediatR;
using RoadRegistry.BackOffice.Api.Extracts.V2;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Schema;
using SqlStreamStore;

public partial class ExtractsControllerTests : ControllerTests<ExtractsController>, IAsyncLifetime
{
    private readonly DbContextBuilder _dbContextBuilderFixture;
    private ExtractsDbContext _editorContext;

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
        _editorContext = _dbContextBuilderFixture.CreateExtractsDbContext();
    }
}
