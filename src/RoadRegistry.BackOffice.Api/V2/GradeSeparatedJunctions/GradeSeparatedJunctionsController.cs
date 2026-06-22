namespace RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;

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
[ApiRoute("ongelijkgrondsekruisingen")]
[ApiExplorerSettings(GroupName = "OngelijkGrondseKruisingen")]
public partial class GradeSeparatedJunctionsController : BackofficeApiController
{
    public GradeSeparatedJunctionsController(BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
    }
}

public class GradeSeparatedJunctionNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public GradeSeparatedJunctionNotFoundResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:gradeseparatedjunction:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Onbestaand ongelijkgrondse kruising.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, PublicApi.ApiVersion)
        };
}

public class GradeSeparatedJunctionGoneResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public GradeSeparatedJunctionGoneResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:gradeseparatedjunction:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd ongelijkgrondse kruising.",
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, PublicApi.ApiVersion)
        };
}

public class GradeSeparatedJunctionIdValidator : AbstractValidator<int>
{
    public GradeSeparatedJunctionIdValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(GradeSeparatedJunctionId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
