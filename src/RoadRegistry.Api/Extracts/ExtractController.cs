namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Linq;
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
            PrintMessage("Start processing request");
            using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                var fileBuilder = new RoadRegistryExtractsBuilder();
                PrintMessage("Create roadregistry archive");
                var zip = new RoadRegistryExtractArchive("wegenregister")
                {
                    fileBuilder.CreateOrganizationsFile(context),
                    fileBuilder.CreateRoadNodesFiles(context),
                    fileBuilder.CreateRoadSegmentsFiles(context),
                    fileBuilder.CreateRoadSegmentDynamicLaneAttributesFile(context),
                    fileBuilder.CreateRoadSegmentDynamicWidtAttributesFile(context),
                    fileBuilder.CreateRoadSegmentDynamicHardeningAttributesFile(context),
                    fileBuilder.CreateRoadSegmentNationalRoadAttributesFile(context),
                    fileBuilder.CreateRoadSegmentEuropeanRoadAttributesFile(context),
                    fileBuilder.CreateRoadSegmentNumberedRoadAttributesFile(context),
                    fileBuilder.CreateReferencePointsFiles(context),
                    fileBuilder.CreateGradeSeperatedJunctionsFile(context),
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

        private void PrintMessage(string message)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}]|> {message}");
#endif
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestGet(
            [FromServices] ShapeContext context,
            CancellationToken cancellationToken)
        {
            PrintMessage("Start processing request");
            using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
            {
                var fileBuilder = new RoadRegistryExtractsBuilder();
                PrintMessage("Create roadregistry archive");
                return new RoadRegistryExtractArchive("wegenregister")
                {
                    fileBuilder.CreateOrganizationsFile(context),
//                    fileBuilder.CreateRoadNodeTypesFile(),
//                    fileBuilder.CreateHardeningTypesFile(),
//                    fileBuilder.CreateNumberedRoadSegmentDirectionsFile(),
//                    fileBuilder.CreateReferencePointTypesFile(),
//                    fileBuilder.CreateRoadSegmentCategoriesFile(),
//                    fileBuilder.CreateRoadSegmentAccessRestrictionsFile(),
//                    fileBuilder.CreateRoadSegmentGeometryDrawMethodsFile(),
//                    fileBuilder.CreateRoadSegmentMorphologiesFile(),
//                    fileBuilder.CreateRoadSegmentStatusesFile(),
//                    fileBuilder.CreateGradeSeperatedJunctionTypesFile(),
//                    fileBuilder.CreateLaneDirectionsFile()
                }.CreateCallbackFileStreamResult(cancellationToken);
            }
        }
    }
}
