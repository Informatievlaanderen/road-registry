namespace RoadRegistry.BackOffice.Api.Organizations.Handlers;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Organizations;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class GetOrganizationsRequestHandler : EndpointRequestHandler<GetOrganizationsRequest, GetOrganizationsResponse>
{
    private readonly EditorContext _editorContext;

    public GetOrganizationsRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<GetOrganizationsRequestHandler> logger,
        EditorContext editorContext)
        : base(dispatcher, logger)
    {
        _editorContext = editorContext;
    }

    protected override async Task<GetOrganizationsResponse> InnerHandleAsync(GetOrganizationsRequest request, CancellationToken cancellationToken)
    {
        var organizations = (
            await _editorContext.OrganizationsV2
                .Where(x => x.IsMaintainer)
                .ToListAsync(cancellationToken)
            )
            .Select(organization => new OrganizationDetail
            {
                Code = new OrganizationId(organization.Code),
                Name = new OrganizationName(organization.Name),
                OvoCode = OrganizationOvoCode.FromValue(organization.OvoCode),
                KboNumber = OrganizationKboNumber.FromValue(organization.KboNumber)
            })
            .ToList();

        return new GetOrganizationsResponse(organizations);
    }
}
