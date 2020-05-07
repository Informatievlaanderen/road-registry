namespace RoadRegistry.BackOffice.Api.Downloads
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Editor.Schema;
    using Framework;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Microsoft.Net.Http.Headers;
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
        public async Task<IActionResult> Get([FromServices] EditorContext context)
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
                    var writer = new RoadNetworkForEditorToZipArchiveWriter(_manager, encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, context, HttpContext.RequestAborted);
                    }
                })
            {
                FileDownloadName = "wegenregister.zip"
            };
        }

        [HttpGet("for-product")]
        public async Task<IActionResult> Get([FromServices] ProductContext context)
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
                    var writer = new RoadNetworkForProductToZipArchiveWriter(_manager, encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, context, HttpContext.RequestAborted);
                    }
                })
            {
                FileDownloadName = "wegenregister.zip"
            };
        }
    }
}
