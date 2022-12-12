using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Filters;

namespace RoadRegistry.BackOffice.Api.RoadSegments
{
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;

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
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return new ProblemDetails();
            }

            return new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:api",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = ValidationErrors.RoadSegment.NotFound.Message,
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(httpContext)
            };
        }
    }
}
