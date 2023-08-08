namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Api.Extracts;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Editor.Schema;
using Infrastructure;
using Infrastructure.Containers;
using MediatR;
using RoadRegistry.Tests.BackOffice;
using SqlStreamStore;

[Collection(nameof(SqlServerCollection))]
public partial class ExtractsControllerTests : ControllerTests<ExtractsController>, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private readonly SqlServer _sqlServerFixture;
    private EditorContext _editorContext;

    public ExtractsControllerTests(
        SqlServer sqlServerFixture,
        ExtractsController controller,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(controller, mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
        _sqlServerFixture = sqlServerFixture;
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
        _editorContext = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync());
    }
}
