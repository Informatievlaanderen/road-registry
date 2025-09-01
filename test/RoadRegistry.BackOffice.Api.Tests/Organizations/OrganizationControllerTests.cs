namespace RoadRegistry.BackOffice.Api.Tests.Organizations;

using Api.Organizations;
using Editor.Schema;
using Editor.Schema.Organizations;
using Infrastructure;
using SqlStreamStore;

public partial class OrganizationControllerTests : ControllerMinimalTests<OrganizationsController>, IAsyncLifetime
{
    private readonly IStreamStore _store;
    private readonly EditorContext _editorContext;

    public OrganizationControllerTests(
        OrganizationsController controller,
        IStreamStore streamStore,
        EditorContext editorContext)
        : base(controller)
    {
        _store = streamStore;
        _editorContext = editorContext.ThrowIfNull();
    }

    public async Task InitializeAsync()
    {
        await _editorContext.OrganizationsV2.AddRangeAsync(
            new OrganizationRecordV2 {
                Code = "AGIV",
                Name = "Agentschap voor Geografische Informatie Vlaanderen",
                IsMaintainer = true
            },
            new OrganizationRecordV2 {
                Code = "11040",
                Name = "Gemeente Schoten",
                OvoCode = "OVO002229",
                IsMaintainer = true
            },
            new OrganizationRecordV2 {
                Code = "ABC",
                Name = "Random Bedrijf",
                OvoCode = "OVO001234",
                KboNumber = "0123456789",
                IsMaintainer = false
            }
        );

        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _editorContext.DisposeAsync();
    }
}
