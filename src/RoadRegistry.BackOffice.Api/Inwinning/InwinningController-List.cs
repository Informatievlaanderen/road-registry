namespace RoadRegistry.BackOffice.Api.Inwinning;

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

public partial class InwinningController
{
    /// <summary>
    ///     List of extracts.
    /// </summary>
    [ProducesResponseType(typeof(InwinningExtractsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ListInwinningExtracten))]
    [HttpGet("extracten", Name = nameof(ListInwinningExtracten))]
    public async Task<ActionResult> ListInwinningExtracten(
        CancellationToken cancellationToken = default)
    {
        var @operator = ApiContext.HttpContextAccessor.HttpContext.GetOperator();
        if (@operator is null)
        {
            throw new InvalidOperationException("User is authenticated but no operator could be found.");
        }

        var request = new InwinningExtractListRequest(@operator);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(new InwinningExtractsListResponse
        {
            Items = response.Items
                .Select(x => new InwinningExtractListItem
                {
                    DownloadId = x.DownloadId,
                    Beschrijving = x.Description,
                    AangevraagdOp = x.RequestedOn,
                    Informatief = x.IsInformative,
                    DownloadStatus = x.DownloadStatus,
                    UploadStatus = x.UploadStatus,
                    Gesloten = x.Closed
                })
                .ToList()
        });
    }
}

public sealed record InwinningExtractsListResponse
{
    public ICollection<InwinningExtractListItem> Items { get; init; }
}

public sealed record InwinningExtractListItem
{
    public string DownloadId { get; init; }
    public string Beschrijving { get; init; }
    public DateTimeOffset AangevraagdOp { get; init; }
    public bool Informatief { get; init; }
    public string DownloadStatus { get; init; }
    public string? UploadStatus { get; init; }
    public bool Gesloten { get; init; }
}
