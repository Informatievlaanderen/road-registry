namespace RoadRegistry.BackOffice.Handlers.Organizations;

using Abstractions;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions.Organizations;
using ZipArchiveWriters.ExtractHost;

public class GetOrganizationsRequestHandler : EndpointRequestHandler<GetOrganizationsRequest, GetOrganizationsResponse>
{
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;

    public GetOrganizationsRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<GetOrganizationsRequestHandler> logger,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding)
        : base(dispatcher, logger)
    {
        _editorContext = editorContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
    }

    public override async Task<GetOrganizationsResponse> HandleAsync(GetOrganizationsRequest request, CancellationToken cancellationToken)
    {
        var organizationRecordReader = new OrganizationDbaseRecordReader(_manager, _fileEncoding);

        var organizations = (
            await _editorContext.Organizations
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken)
            )
            .Select(organization => organizationRecordReader.Read(organization.DbaseRecord, organization.DbaseSchemaVersion))
            .ToList();
        
        return new GetOrganizationsResponse(organizations);
    }
}
