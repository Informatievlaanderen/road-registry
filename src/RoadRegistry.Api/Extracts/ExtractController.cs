namespace RoadRegistry.Api.Extracts
{
    using System;
    using System.Collections.Generic;
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
    using Swashbuckle.AspNetCore.Examples;

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
            IReadOnlyCollection<OrganizationRecord> organizations;
            IReadOnlyCollection<RoadNodeRecord> nodeRecords;
            IReadOnlyCollection<RoadReferencePointRecord> referencePointRecords;
            IReadOnlyCollection<RoadSegmentRecord> roadSegments;
            IReadOnlyCollection<RoadSegmentDynamicLaneAttributeRecord> roadSegmentDynamicLaneAttributes;
            IReadOnlyCollection<RoadSegmentDynamicWidthAttributeRecord> roadSegmentDynamicWidthAttributeRecords;
            IReadOnlyCollection<RoadSegmentDynamicHardeningAttributeRecord> roadSegmentDynamicHardeningAttributeRecords;
            IReadOnlyCollection<RoadSegmentNationalRoadAttributeRecord> roadSegmentNationalRoadAttributeRecords;
            IReadOnlyCollection<RoadSegmentEuropeanRoadAttributeRecord> roadSegmentEuropeanRoadAttributeRecords;
            IReadOnlyCollection<RoadSegmentNumberedRoadAttributeRecord> roadSegmentNumberedRoadAttributeRecords;
            IReadOnlyCollection<GradeSeparatedJunctionRecord> gradeSeparatedJunctionRecords;
            // TODO: Make sure there's a transaction to ensure the count and iteration are in sync
            // using (var transaction = context.Database.BeginTransaction())
            //  {
            organizations = await context.Organizations.AsUntrackedCollectionAsync();
            nodeRecords = await context.RoadNodes.AsUntrackedCollectionAsync();
            referencePointRecords = await context.RoadReferencePoints.AsUntrackedCollectionAsync();
            roadSegments = await context.RoadSegments.AsUntrackedCollectionAsync();
            roadSegmentDynamicLaneAttributes = await context.RoadLaneAttributes.AsUntrackedCollectionAsync();
            roadSegmentDynamicWidthAttributeRecords = await context.RoadWidthAttributes.AsUntrackedCollectionAsync();
            roadSegmentDynamicHardeningAttributeRecords = await context.RoadHardeningAttributes.AsUntrackedCollectionAsync();
            roadSegmentNationalRoadAttributeRecords = await context.NationalRoadAttributes.AsUntrackedCollectionAsync();
            roadSegmentEuropeanRoadAttributeRecords = await context.EuropeanRoadAttributes.AsUntrackedCollectionAsync();
            roadSegmentNumberedRoadAttributeRecords = await context.NumberedRoadAttributes.AsUntrackedCollectionAsync();
            gradeSeparatedJunctionRecords = await context.GradeSeparatedJunctions.AsUntrackedCollectionAsync();
            // }
            PrintMessage("Queried data");

            var fileBuilder = new RoadRegistryExtractsBuilder();
            PrintMessage("Create roadregistry archive");
            var zip = new RoadRegistryExtractArchive("wegenregister")
            {
                fileBuilder.CreateOrganizationsFile(organizations),

                fileBuilder.CreateRoadNodesFiles(nodeRecords),
                fileBuilder.CreateRoadNodeTypesFile(),

                fileBuilder.CreateRoadSegmentsFiles(roadSegments),
                fileBuilder.CreateRoadSegmentCategoriesFile(),
                fileBuilder.CreateRoadSegmentAccessRestrictionsFile(),
                fileBuilder.CreateRoadSegmentGeometryDrawMethodsFile(),
                fileBuilder.CreateRoadSegmentMorphologiesFile(),
                fileBuilder.CreateRoadSegmentStatusesFile(),
                fileBuilder.CreateRoadSegmentDynamicLaneAttributesFile(roadSegmentDynamicLaneAttributes),
                fileBuilder.CreateRoadSegmentDynamicWidtAttributesFile(roadSegmentDynamicWidthAttributeRecords),
                fileBuilder.CreateRoadSegmentDynamicHardeningAttributesFile(roadSegmentDynamicHardeningAttributeRecords),
                fileBuilder.CreateHardeningTypesFile(),
                fileBuilder.CreateRoadSegmentNationalRoadAttributesFile(roadSegmentNationalRoadAttributeRecords),
                fileBuilder.CreateRoadSegmentEuropeanRoadAttributesFile(roadSegmentEuropeanRoadAttributeRecords),
                fileBuilder.CreateRoadSegmentNumberedRoadAttributesFile(roadSegmentNumberedRoadAttributeRecords),
                fileBuilder.CreateNumberedRoadSegmentDirectionsFile(),

                fileBuilder.CreateReferencePointsFiles(referencePointRecords),
                fileBuilder.CreateReferencePointTypesFile(),

                fileBuilder.CreateGradeSeperatedJunctionsFile(gradeSeparatedJunctionRecords),
                fileBuilder.CreateGradeSeperatedJunctionTypesFile(),

                fileBuilder.CreateLaneDirectionsFile()
            };


            PrintMessage("Create download");
            return zip.CreateResponse();
        }

        private void PrintMessage(string message)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}]|> {message}");
#endif
        }
    }
}
