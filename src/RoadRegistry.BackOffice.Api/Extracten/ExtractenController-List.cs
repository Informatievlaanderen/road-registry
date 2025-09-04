namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Infrastructure.Extensions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     List of extracts.
    /// </summary>
    [ProducesResponseType(typeof(ExtractsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ListExtracten))]
    [HttpGet("", Name = nameof(ListExtracten))]
    public async Task<ActionResult> ListExtracten(
        [FromQuery] bool? eigenExtracten = null,
        [FromQuery] int? page = null,
        CancellationToken cancellationToken = default)
    {
        var organizationCode = ApiContext.HttpContextAccessor.HttpContext.GetOperatorName();
        if (organizationCode is null)
        {
            throw new InvalidOperationException("User is authenticated but no operator could be found.");
        }

        // NOTE: for currently unknown reasons, a 400-error is returned if the page parameter value is `0`.
        page ??= 0;
        if (page < 0)
        {
            throw new ValidationException([new ValidationFailure(nameof(page), "Page must be a non-negative integer.")]);
        }
        var pageIndex = page.Value;

        var filterByOrganizationCode = (eigenExtracten ?? true) ? organizationCode : null;

        var request = new ExtractListRequest(filterByOrganizationCode, pageIndex);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(new ExtractsListResponse
        {
            Items = response.Items
                .Select(x => new ExtractListItem
                {
                    DownloadId = x.DownloadId,
                    Beschrijving = x.Description,
                    AangevraagdOp = x.RequestedOn,
                    Informatief = x.IsInformative,
                    DownloadStatus = x.DownloadStatus,
                    UploadStatus = x.UploadStatus,
                    Gesloten = x.Closed
                })
                .ToList(),
            DataBeschikbaar = response.MoreDataAvailable
        });
    }
}

public sealed record ExtractsListResponse
{
    public ICollection<ExtractListItem> Items { get; init; }
    public bool DataBeschikbaar { get; init; }
}

public sealed record ExtractListItem
{
    public string DownloadId { get; init; }
    public string Beschrijving { get; init; }
    public DateTimeOffset AangevraagdOp { get; init; }
    public bool Informatief { get; init; }
    public string DownloadStatus { get; init; }
    public string? UploadStatus { get; init; }
    public bool Gesloten { get; init; }
}
