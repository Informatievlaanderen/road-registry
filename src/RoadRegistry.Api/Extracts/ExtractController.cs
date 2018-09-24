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
                var fileBuilder = new RoadRegistryExtractsBuilder(PrintMessage);
                PrintMessage("Create roadregistry archive");
                var zip = new RoadRegistryExtractArchive("wegenregister")
                {
                    () => fileBuilder
                        .CreateOrganizationsFileAsync(context
                            .Organizations
                            .OrderBy(record => record.SortableCode)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadNodesFilesAsync(context
                            .RoadNodes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentsFilesAsync(context
                            .RoadSegments
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentDynamicLaneAttributesFileAsync(context
                            .RoadLaneAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentDynamicWidtAttributesFileAsync(context
                            .RoadWidthAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentDynamicHardeningAttributesFileAsync(context
                            .RoadHardeningAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentNationalRoadAttributesFileAsync(context
                            .NationalRoadAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentEuropeanRoadAttributesFileAsync(context
                            .EuropeanRoadAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateRoadSegmentNumberedRoadAttributesFileAsync(context
                            .NumberedRoadAttributes
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateReferencePointsFilesAsync(context
                            .RoadReferencePoints
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder
                        .CreateGradeSeperatedJunctionsFileAsync(context
                            .GradeSeparatedJunctions
                            .OrderBy(record => record.Id)
                            .AsReadOnlyAsync(cancellationToken)
                        ),
                    () => fileBuilder.CreateRoadNodeTypesFile(),
                    () => fileBuilder.CreateHardeningTypesFile(),
                    () => fileBuilder.CreateNumberedRoadSegmentDirectionsFile(),
                    () => fileBuilder.CreateReferencePointTypesFile(),
                    () => fileBuilder.CreateRoadSegmentCategoriesFile(),
                    () => fileBuilder.CreateRoadSegmentAccessRestrictionsFile(),
                    () => fileBuilder.CreateRoadSegmentGeometryDrawMethodsFile(),
                    () => fileBuilder.CreateRoadSegmentMorphologiesFile(),
                    () => fileBuilder.CreateRoadSegmentStatusesFile(),
                    () => fileBuilder.CreateGradeSeperatedJunctionTypesFile(),
                    () => fileBuilder.CreateLaneDirectionsFile()
                };
                return zip.CreateCallbackFileStreamResult();
            }
        }

        private void PrintMessage(string message)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}]|> {message}");
#endif
        }
    }
}
