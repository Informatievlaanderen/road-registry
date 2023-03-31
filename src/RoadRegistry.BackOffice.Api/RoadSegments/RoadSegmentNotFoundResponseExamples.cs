using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Filters;

namespace RoadRegistry.BackOffice.Api.RoadSegments
{
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Core;
    using Extensions;

    public class RoadSegmentNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public RoadSegmentNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
        {
            return new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadsegment:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = new RoadSegmentNotFound().TranslateToDutch().Message,
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
        }
    }
}
