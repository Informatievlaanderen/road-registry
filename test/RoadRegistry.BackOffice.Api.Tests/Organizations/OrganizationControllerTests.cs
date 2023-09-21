namespace RoadRegistry.BackOffice.Api.Tests.Organizations;

using Api.Organizations;
using BackOffice.Extracts;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Uploads;
using Editor.Projections;
using Editor.Schema;
using Infrastructure;
using MediatR;
using Microsoft.IO;
using SqlStreamStore;
using Xunit.Abstractions;

public partial class OrganizationControllerTests : ControllerTests<OrganizationsController>, IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _encoding;

    public OrganizationControllerTests(
        ITestOutputHelper testOutputHelper,
        OrganizationsController controller,
        IMediator mediator,
        IStreamStore streamStore,
        EditorContext editorContext,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding)
        : base(controller, mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
        _testOutputHelper = testOutputHelper;
        _editorContext = editorContext;
        _manager = manager;
        _encoding = fileEncoding;
    }

    public async Task InitializeAsync()
    {
        await _editorContext.Organizations.AddRangeAsync(
            new OrganizationRecord {
                Code = "AGIV",
                SortableCode = "AGIV",
                DbaseSchemaVersion = BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord.DbaseSchemaVersion,
                DbaseRecord = new BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord
                {
                    ORG = { Value = "AGIV" },
                    LBLORG = { Value = "Agentschap voor Geografische Informatie Vlaanderen" }
                }.ToBytes(_manager, _encoding)
            },
            new OrganizationRecord {
                Code = "11040",
                SortableCode = "11040",
                DbaseSchemaVersion = BackOffice.Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord.DbaseSchemaVersion,
                DbaseRecord = new BackOffice.Extracts.Dbase.Organizations.V2.OrganizationDbaseRecord
                {
                    ORG = { Value = "11040" },
                    LBLORG = { Value = "Gemeente Schoten" },
                    OVOCODE = { Value = "OVO002229" }
                }.ToBytes(_manager, _encoding)
            }
        );

        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _editorContext.DisposeAsync();
    }
}
