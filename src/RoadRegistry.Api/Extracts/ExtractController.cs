namespace RoadRegistry.Api.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.Api;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.CommandHandling;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Projections;
    using Responses;
    using Microsoft.EntityFrameworkCore;
    using System.Data;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extracten")]
    [ApiExplorerSettings(GroupName = "Extracten")]
    public class ExtractController : ApiBusController
    {
        public ExtractController(ICommandHandlerResolver bus)
            : base(bus)
        { }

        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als wegenregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RoadRegistryResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ShapeContext context,
            CancellationToken cancellationToken)
        {
            using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                if ((await context.RoadRegistryImportStates.SingleOrDefaultAsync(cancellationToken))?.ImportComplete ?? false)
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);

                var fileBuilder = new RoadRegistryExtractsBuilder();
                var zip = new RoadRegistryExtractArchive("wegenregister")
                {
                    fileBuilder.CreateOrganizationsFile(context.Organizations),
                    fileBuilder.CreateRoadNodesFiles(context),
                    fileBuilder.CreateRoadSegmentsFiles(context),
                    fileBuilder.CreateRoadSegmentDynamicLaneAttributesFile(context.RoadLaneAttributes),
                    fileBuilder.CreateRoadSegmentDynamicWidtAttributesFile(context.RoadWidthAttributes),
                    fileBuilder.CreateRoadSegmentDynamicHardeningAttributesFile(context.RoadHardeningAttributes),
                    fileBuilder.CreateRoadSegmentNationalRoadAttributesFile(context.NationalRoadAttributes),
                    fileBuilder.CreateRoadSegmentEuropeanRoadAttributesFile(context.EuropeanRoadAttributes),
                    fileBuilder.CreateRoadSegmentNumberedRoadAttributesFile(context.NumberedRoadAttributes),
                    fileBuilder.CreateReferencePointsFiles(context),
                    fileBuilder.CreateGradeSeperatedJunctionsFile(context.GradeSeparatedJunctions),
                    fileBuilder.CreateRoadNodeTypesFile(),
                    fileBuilder.CreateHardeningTypesFile(),
                    fileBuilder.CreateNumberedRoadSegmentDirectionsFile(),
                    fileBuilder.CreateReferencePointTypesFile(),
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
