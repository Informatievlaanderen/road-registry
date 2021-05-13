namespace RoadRegistry.BackOffice.Api.Downloads
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Configuration;
    using Editor.Schema;
    using Framework;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Microsoft.Net.Http.Headers;
    using NodaTime;
    using NodaTime.Text;
    using Product.Schema;
    using ZipArchiveWriters.ForEditor;
    using ZipArchiveWriters.ForProduct;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("download")]
    [ApiExplorerSettings(GroupName = "Downloads")]
    public class DownloadController : ControllerBase
    {
        private readonly RecyclableMemoryStreamManager _manager;

        public DownloadController(RecyclableMemoryStreamManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        [HttpGet("for-editor")]
        public async Task<IActionResult> Get(
            [FromServices] EditorContext context,
            [FromServices] ZipArchiveWriterOptions zipArchiveWriterOptions,
            [FromServices] IStreetNameCache streetNameCache)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
            if (info == null || !info.CompletedImport)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/zip"),
                async (stream, actionContext) =>
                {
                    var encoding = Encoding.GetEncoding(1252);
                    var writer = new RoadNetworkForEditorToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, _manager, encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, context, HttpContext.RequestAborted);
                    }
                })
            {
                FileDownloadName = "wegenregister.zip"
            };
        }

        [HttpGet("for-product/{date}")]
        public async Task<IActionResult> Get(
            string date,
            [FromServices] ProductContext context,
            [FromServices] ZipArchiveWriterOptions zipArchiveWriterOptions,
            [FromServices] IStreetNameCache streetNameCache)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
            if (info == null || !info.CompletedImport)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var result = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd").Parse(date);
            if (!result.Success)
            {
                return BadRequest();
            }

            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/zip"),
                async (stream, actionContext) =>
                {
                    var encoding = Encoding.GetEncoding(1252);
                    var writer = new RoadNetworkForProductToZipArchiveWriter(result.Value, zipArchiveWriterOptions, streetNameCache, _manager, encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, context, HttpContext.RequestAborted);
                    }
                })
            {
                FileDownloadName = $"wegenregister-{date}.zip"
            };
        }
    }
}
