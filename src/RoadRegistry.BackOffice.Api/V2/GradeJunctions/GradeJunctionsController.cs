namespace RoadRegistry.BackOffice.Api.V2.GradeJunctions;

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
using Swashbuckle.AspNetCore.Filters;
using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;
using Version = RoadRegistry.BackOffice.Api.Infrastructure.Version;

[ApiVersion(Version.V2)]
[ApiRoute("gelijkgrondsekruisingen")]
[ApiExplorerSettings(GroupName = "Gelijkgrondsekruisingen")]
public partial class GradeJunctionsController : BackofficeApiController
{
    internal const string PublicApiVersion = "v3";

    public GradeJunctionsController(BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
    }
}

public class GradeJunctionNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public GradeJunctionNotFoundResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:gradeJunction:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = new GradeJunctionNotFound().TranslateToDutch(WellKnownProblemTranslators.Default).Message,
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, GradeJunctionsController.PublicApiVersion)
        };
}

public class GradeJunctionGoneResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public GradeJunctionGoneResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:gradejunction:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd gelijkgrondse kruising.", //TODO-pr confirm what is actually returned
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, GradeJunctionsController.PublicApiVersion)
        };
}

public class GradeJunctionIdValidator : AbstractValidator<int>
{
    public GradeJunctionIdValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(GradeJunctionId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
