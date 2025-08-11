namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(typeof(ExtractsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetList))]
    [HttpGet("", Name = nameof(GetList))]
    public async Task<ActionResult> GetList(
        [FromQuery] bool includeAllOrganizations = false,
        CancellationToken cancellationToken = default)
    {
        var organizationCode = ApiContext.HttpContextAccessor.HttpContext.GetOperatorName();
        if (organizationCode is null)
        {
            throw new InvalidOperationException("User is authenticated but no operator could be found.");
        }

        var filterByOrganizationCode = includeAllOrganizations ? null : organizationCode;

        var request = new ExtractListRequest(filterByOrganizationCode);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(new ExtractsListResponse
        {
            Items = response.Items
                .Select(x => new ExtractListItem
                {
                    DownloadId = x.DownloadId,
                    Description = x.Description,
                    ExtractRequestId = x.ExtractRequestId,
                    RequestedOn = x.RequestedOn,
                    IsInformative = x.IsInformative,
                    DownloadStatus = x.DownloadStatus,
                    UploadStatus = x.UploadStatus,
                    Closed = x.Closed
                })
                .ToList()
        });
    }
}

public sealed record ExtractsListResponse
{
    public ICollection<ExtractListItem> Items { get; init; }
}

public sealed record ExtractListItem
{
    public string DownloadId { get; init; }
    public string Description { get; init; }
    public string ExtractRequestId { get; init; }
    public DateTimeOffset RequestedOn { get; init; }
    public bool IsInformative { get; init; }
    public string DownloadStatus { get; init; }
    public string? UploadStatus { get; init; }
    public bool Closed { get; init; }
}
