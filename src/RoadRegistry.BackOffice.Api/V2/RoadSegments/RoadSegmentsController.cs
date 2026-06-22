namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using Swashbuckle.AspNetCore.Filters;
using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;
using Version = RoadRegistry.BackOffice.Api.Infrastructure.Version;

[ApiVersion(Version.V2)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "WegsegmentenV2")]
public partial class RoadSegmentsController : BackofficeApiController
{
    internal const string PublicApiVersion = "v3";

    public RoadSegmentsController(BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
    }
}

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

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadsegment:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand wegsegment.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, RoadSegmentsController.PublicApiVersion)
        };
}

public class RoadSegmentGoneResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public RoadSegmentGoneResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadsegment:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd wegsegment.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, RoadSegmentsController.PublicApiVersion)
        };
}

public class RoadSegmentIdValidator : AbstractValidator<int>
{
    public RoadSegmentIdValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
