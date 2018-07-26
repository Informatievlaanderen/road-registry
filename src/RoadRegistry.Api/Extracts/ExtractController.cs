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
            IReadOnlyCollection<RoadSegmentRecord> roadSegments;
            IReadOnlyCollection<RoadSegmentDynamicLaneAttributeRecord> roadSegmentDynamicLaneAttributes;
            // TODO: Make sure there's a transaction to ensure the count and iteration are in sync
            // using (var transaction = context.Database.BeginTransaction())
            //  {
            roadSegments = await context.RoadSegments.AsUntrackedCollectionAsync();
            roadSegmentDynamicLaneAttributes = await context.RoadLaneAttributes.AsUntrackedCollectionAsync();
            // }
            PrintMessage("Queried data");


            var fileBuilder = new RoadRegistryExtractsBuilder();
            PrintMessage("Create roadregistry archive");
            var zip = new RoadRegistryExtractArchive("wegenregister");

            zip.Add(fileBuilder.CreateRoadSegmentFiles(roadSegments));
            PrintMessage("Added road segments files");

            zip.Add(fileBuilder.CreateRoadSegmentDynamicLaneAttributeFile(roadSegmentDynamicLaneAttributes));
            PrintMessage("Added road lane attributes file");

            zip.Add(fileBuilder.CreateLaneDirectionFile());
            PrintMessage("Added road lane direction file");

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
