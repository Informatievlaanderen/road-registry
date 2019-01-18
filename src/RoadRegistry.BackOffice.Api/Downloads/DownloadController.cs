namespace RoadRegistry.Api.Downloads
{
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BackOffice.Schema;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RoadRegistryResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ShapeContext context,
            CancellationToken cancellationToken)
        {
            using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(cancellationToken);
                if(info == null || !info.CompletedImport)
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);

                var fileBuilder = new RoadRegistryExtractsBuilder();
                var zip = new RoadRegistryExtractArchive("wegenregister")
                {
                    fileBuilder.CreateOrganizationsFile(context.Organizations),
                    fileBuilder.CreateRoadNodesFiles(context),
                    fileBuilder.CreateRoadSegmentsFiles(context),
                    fileBuilder.CreateRoadSegmentLaneAttributesFile(context.RoadSegmentLaneAttributes),
                    fileBuilder.CreateRoadSegmentWidtAttributesFile(context.RoadSegmentWidthAttributes),
                    fileBuilder.CreateRoadSegmentSurfaceAttributesFile(context.RoadSegmentSurfaceAttributes),
                    fileBuilder.CreateRoadSegmentNationalRoadAttributesFile(context.RoadSegmentNationalRoadAttributes),
                    fileBuilder.CreateRoadSegmentEuropeanRoadAttributesFile(context.RoadSegmentEuropeanRoadAttributes),
                    fileBuilder.CreateRoadSegmentNumberedRoadAttributesFile(context.RoadSegmentNumberedRoadAttributes),
                    fileBuilder.CreateGradeSeperatedJunctionsFile(context.GradeSeparatedJunctions),
                    fileBuilder.CreateRoadNodeTypesFile(),
                    fileBuilder.CreateSurfaceTypesFile(),
                    fileBuilder.CreateNumberedRoadSegmentDirectionsFile(),
                    fileBuilder.CreateRoadSegmentCategoriesFile(),
                    fileBuilder.CreateRoadSegmentAccessRestrictionsFile(),
                    fileBuilder.CreateRoadSegmentGeometryDrawMethodsFile(),
                    fileBuilder.CreateRoadSegmentMorphologiesFile(),
                    fileBuilder.CreateRoadSegmentStatusesFile(),
                    fileBuilder.CreateGradeSeperatedJunctionTypesFile(),
                    fileBuilder.CreateLaneDirectionsFile()
                };
                return zip.CreateCallbackFileStreamResult(cancellationToken);
            }
        }
    }
}
