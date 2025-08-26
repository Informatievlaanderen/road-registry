namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using FluentValidation;
using FluentValidation.Results;
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
        [FromQuery] bool allOrganizations = false,
        [FromQuery] string? page = null, //TODO-pr erg raar, als je hier een int van maakt dan krijg je 400 errors vanuit de UI, later te bekijken in common api library
        CancellationToken cancellationToken = default)
    {
        var organizationCode = ApiContext.HttpContextAccessor.HttpContext.GetOperatorName();
        if (organizationCode is null)
        {
            throw new InvalidOperationException("User is authenticated but no operator could be found.");
        }

        if (!int.TryParse(page, out var pageIndex) || pageIndex < 0)
        {
            throw new ValidationException([new ValidationFailure(nameof(page), "Page index must be a non-negative integer.")]);
        }

        var filterByOrganizationCode = allOrganizations ? null : organizationCode;

        var request = new ExtractListRequest(filterByOrganizationCode, pageIndex);
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
                .ToList(),
            MoreDataAvailable = response.MoreDataAvailable
        });
    }
}

public sealed record ExtractsListResponse
{
    public ICollection<ExtractListItem> Items { get; init; }
    public bool MoreDataAvailable { get; init; }
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
