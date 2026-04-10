namespace RoadRegistry.BackOffice.Api.Inwinning;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Sync.MunicipalityRegistry;
using RoadRegistry.Sync.MunicipalityRegistry.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class InwinningsstatusController
{
    /// <summary>
    ///     Get gemeente inwinningsstatus
    /// </summary>
    /// <param name="nisCode"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="municipalityContext"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(GemeenteInwinningsstatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetGemeenteInwinningsstatus))]
    [HttpGet("gemeente/{nisCode}", Name = nameof(GetGemeenteInwinningsstatus))]
    [AllowAnonymous]
    public async Task<IActionResult> GetGemeenteInwinningsstatus(
        [FromRoute] string nisCode,
        [FromServices] ExtractsDbContext extractsDbContext,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        CancellationToken cancellationToken = default)
    {
        var inwinningszone = await extractsDbContext.Inwinningszones
            .Where(x => x.NisCode == nisCode)
            .SingleOrDefaultAsync(cancellationToken);

        if (inwinningszone is null)
        {
            var municipality = await municipalityContext.Municipalities.SingleOrDefaultAsync(x => x.NisCode == nisCode && x.Status == MunicipalityStatus.Current, cancellationToken);
            if (municipality is null)
            {
                return NotFound();
            }

            return Ok(new GemeenteInwinningsstatus
            {
                Inwinningsstatus = Inwinningsstatus.NietGestart
            });
        }

        var extractDownloads = await (
            from extractDownload in extractsDbContext.ExtractDownloads
            join extractRequest in extractsDbContext.ExtractRequests on extractDownload.ExtractRequestId equals extractRequest.ExtractRequestId
            where extractRequest.ExternalRequestId == $"INWINNING_{inwinningszone.NisCode}" && !extractDownload.IsInformative
            orderby extractDownload.RequestedOn descending
            select extractDownload
        ).ToListAsync(cancellationToken);

        var historiekExtracten = new List<GemeenteInwinningsstatusExtract>();

        foreach (var extractDownload in extractDownloads)
        {
            var historiek = new List<GemeenteInwinningsstatusExtractHistoriekItem>();
            if (extractDownload.Status == ExtractDownloadStatus.Available)
            {
                historiek.Add(new GemeenteInwinningsstatusExtractHistoriekItem
                {
                    Status = "beschikbaar",
                    Datum = extractDownload.RequestedOn
                });
            }

            var uploadStatusHistory = await (
                from uploadHistory in extractsDbContext.ExtractUploadStatusHistory
                join upload in extractsDbContext.ExtractUploads on uploadHistory.UploadId equals upload.UploadId
                where upload.DownloadId == extractDownload.DownloadId && uploadHistory.Status != ExtractUploadStatus.Processing
                select uploadHistory
                ).ToListAsync(cancellationToken);
            foreach (var uploadStatusHistoryItem in uploadStatusHistory)
            {
                historiek.Add(new GemeenteInwinningsstatusExtractHistoriekItem
                {
                    Status = uploadStatusHistoryItem.Status switch
                    {
                        ExtractUploadStatus.AutomaticValidationFailed => "verworpen",
                        ExtractUploadStatus.AutomaticValidationSucceeded => "automatische controles geslaagd",
                        ExtractUploadStatus.ManualValidationFailed => "geweigerd",
                        ExtractUploadStatus.Accepted => "goedgekeurd",
                        _ => throw new ArgumentOutOfRangeException($"Status {uploadStatusHistoryItem.Status}")
                    },
                    Datum = uploadStatusHistoryItem.Timestamp
                });
            }

            if (extractDownload.Closed)
            {
                historiek.Add(new GemeenteInwinningsstatusExtractHistoriekItem
                {
                    Status = "gesloten",
                    Datum = extractDownload.ClosedOn ?? historiek.Max(x => x.Datum).AddMilliseconds(1)
                });
            }

            historiekExtracten.Add(new GemeenteInwinningsstatusExtract
            {
                DownloadId = new DownloadId(extractDownload.DownloadId).ToString(),
                Historiek = historiek.OrderBy(x => x.Datum).ToList()
            });
        }

        return Ok(new GemeenteInwinningsstatus
        {
            Inwinningsstatus = inwinningszone.Completed
                ? Inwinningsstatus.Compleet
                : Inwinningsstatus.Locked,
            HistoriekExtracten = historiekExtracten
        });
    }
}

public class GemeenteInwinningsstatus
{
    [RoadRegistryEnumDataType(typeof(Inwinningsstatus))]
    public string Inwinningsstatus { get; init; }

    public List<GemeenteInwinningsstatusExtract> HistoriekExtracten { get; init; }
}

public class GemeenteInwinningsstatusExtract
{
    public string DownloadId { get; init; }
    public List<GemeenteInwinningsstatusExtractHistoriekItem> Historiek { get; init; }
}

public class GemeenteInwinningsstatusExtractHistoriekItem
{
    public string Status { get; init; }
    public DateTimeOffset Datum { get; init; }
}

public class GemeenteInwinningsstatusResponseExamples : IExamplesProvider<GemeenteInwinningsstatus>
{
    public GemeenteInwinningsstatus GetExamples()
    {
        var startDatum = DateTimeOffset.UtcNow.AddDays(-1);

        return new GemeenteInwinningsstatus
        {
            Inwinningsstatus = Inwinningsstatus.Locked.ToDutchString(),
            HistoriekExtracten = [
                new GemeenteInwinningsstatusExtract
                {
                    DownloadId = "a4c19f2ce7f242f8908c918d0aaaca23",
                    Historiek =
                    [
                        new GemeenteInwinningsstatusExtractHistoriekItem
                        {
                            Status = "beschikbaar",
                            Datum = startDatum
                        },
                        new GemeenteInwinningsstatusExtractHistoriekItem
                        {
                            Status = "verworpen",
                            Datum = startDatum.AddHours(1)
                        },
                        new GemeenteInwinningsstatusExtractHistoriekItem
                        {
                            Status = "automatische controles geslaagd",
                            Datum = startDatum.AddHours(2)
                        },
                        new GemeenteInwinningsstatusExtractHistoriekItem
                        {
                            Status = "goedgekeurd",
                            Datum = startDatum.AddHours(2).AddMinutes(30)
                        },
                        new GemeenteInwinningsstatusExtractHistoriekItem
                        {
                            Status = "gesloten",
                            Datum = startDatum.AddHours(2).AddMinutes(31)
                        }
                    ]
                }
            ]
        };
    }
}
