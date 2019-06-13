namespace RoadRegistry.Api.Activities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("activity")]
    [ApiExplorerSettings(GroupName = "Activities")]
    public class ActivityController : ControllerBase
    {
        /// <summary>
        /// Request an archive of the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="context">The database context to query data with.</param>
        /// <response code="200">Returned if the road registry activity can be downloaded.</response>
        /// <response code="500">Returned if the road registry activity can not be downloaded due to an unforeseen server error.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ActivityResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get([FromServices] ShapeContext context)
        {
            var activities = new List<ActivityResponseItem>();
            await context.RoadNetworkActivities.ForEachAsync(activity =>
            {
                var item = new ActivityResponseItem
                {
                    Id = activity.Id,
                    Title = activity.Title,
                    Type = activity.Type
                };
                switch (activity.Type)
                {
                    case nameof(BeganRoadNetworkImport):
                        item.Content = null;
                        break;
                    case nameof(CompletedRoadNetworkImport):
                        item.Content = null;
                        break;
                    case nameof(RoadNetworkChangesArchiveUploaded):
                        item.Content = JsonConvert.DeserializeObject(activity.Content,
                            typeof(RoadNetworkChangesArchiveUploadedActivity));
                        break;
                    case nameof(RoadNetworkChangesArchiveAccepted):
                        item.Content = JsonConvert.DeserializeObject(activity.Content,
                            typeof(RoadNetworkChangesArchiveAcceptedActivity));
                        break;
                    case nameof(RoadNetworkChangesArchiveRejected):
                        item.Content = JsonConvert.DeserializeObject(activity.Content,
                            typeof(RoadNetworkChangesArchiveRejectedActivity));
                        break;
                }

                activities.Add(item);
            }, HttpContext.RequestAborted);

            return new JsonResult(new ActivityResponse
            {
                Activities = activities.ToArray()
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
