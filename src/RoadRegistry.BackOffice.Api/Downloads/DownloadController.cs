namespace RoadRegistry.Api.Downloads
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BackOffice.Schema;
    using BackOffice.Schema.ReferenceData;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json.Converters;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("download")]
    [ApiExplorerSettings(GroupName = "Downloads")]
    public class DownloadController : ControllerBase
    {
        /// <summary>
        /// Request an archive of the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="context">The database context to query data with.</param>
        /// <param name="cancellationToken">The token that controls request cancellation.</param>
        /// <response code="200">Returned if the road registry can be downloaded.</response>
        /// <response code="500">Returned if the road registry can not be downloaded due to an unforeseen server error.</response>
        /// <response code="503">Returned if the road registry can not yet be downloaded (e.g. because the import has not yet completed).</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DownloadResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ShapeContext context,
            CancellationToken cancellationToken)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);
            if (info == null || !info.CompletedImport)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/zip"),
                async (stream, actionContext) =>
                {
                    var encoding = Encoding.ASCII; // TODO: Inject
                    using(await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
                    {
                        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                        {
                            await new OrganizationArchiveWriter(encoding).WriteAsync(archive, context, cancellationToken);
                            await new RoadNodeArchiveWriter(encoding).WriteAsync(archive, context, cancellationToken);
                            await new RoadSegmentArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentLaneAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentWidthAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentSurfaceAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentNationalRoadAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentEuropeanRoadAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new RoadSegmentNumberedRoadAttributeArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new GradeSeperatedJunctionArchiveWriter(encoding).WriteAsync(archive, context,
                                cancellationToken);
                            await new DbaseFileArchiveWriter<RoadNodeTypeDbaseRecord, RoadNodeTypeDbaseSchema>(
                                    "WegknoopLktType.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadNodeTypeDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<SurfaceTypeDbaseRecord, SurfaceTypeDbaseSchema>(
                                    "WegverhardLktType.dbf", encoding)
                                .WriteAsync(archive, Lists.AllSurfaceTypeDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<NumberedRoadSegmentDirectionDbaseRecord, NumberedRoadSegmentDirectionDbaseSchema>(
                                    "GenumwegLktRichting.dbf", encoding)
                                .WriteAsync(archive, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<RoadSegmentCategoryDbaseRecord, RoadSegmentCategoryDbaseSchema>(
                                    "WegsegmentLktWegcat.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadSegmentCategoryDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<RoadSegmentAccessRestrictionDbaseRecord, RoadSegmentAccessRestrictionDbaseSchema>(
                                    "WegsegmentLktTgbep.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<RoadSegmentGeometryDrawMethodDbaseRecord, RoadSegmentGeometryDrawMethodDbaseSchema>(
                                    "WegsegmentLktMethode.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<RoadSegmentMorphologyDbaseRecord, RoadSegmentMorphologyDbaseSchema>(
                                    "WegsegmentLktMorf.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadSegmentMorphologyDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<RoadSegmentStatusDbaseRecord, RoadSegmentStatusDbaseSchema>(
                                    "WegsegmentLktStatus.dbf", encoding)
                                .WriteAsync(archive, Lists.AllRoadSegmentStatusDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<GradeSeparatedJunctionTypeDbaseRecord, GradeSeparatedJunctionTypeDbaseSchema>(
                                    "OgkruisingLktType.dbf", encoding)
                                .WriteAsync(archive, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, cancellationToken);
                            await new DbaseFileArchiveWriter<LaneDirectionDbaseRecord, LaneDirectionDbaseSchema>(
                                    "RijstrokenLktRichting.dbf", encoding)
                                .WriteAsync(archive, Lists.AllLaneDirectionDbaseRecords, cancellationToken);
                        }
                    }
                })
            {
                FileDownloadName = "wegenregister.zip"
            };
        }
    }
}
