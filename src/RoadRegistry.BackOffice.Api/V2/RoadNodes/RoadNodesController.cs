namespace RoadRegistry.BackOffice.Api.V2.RoadNodes;

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
[ApiRoute("wegknopen")]
[ApiExplorerSettings(GroupName = "Wegknopen")]
public partial class RoadNodesController : BackofficeApiController
{
    internal const string PublicApiVersion = "v3";

    public RoadNodesController(BackofficeApiControllerContext apiContext)
        : base(apiContext)
    {
    }
}

public class RoadNodeNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public RoadNodeNotFoundResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadnode:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = new RoadNodeNotFound().TranslateToDutch(WellKnownProblemTranslators.Default).Message,
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, RoadNodesController.PublicApiVersion)
        };
}

public class RoadNodeGoneResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;

    public RoadNodeGoneResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
    }

    public ProblemDetails GetExamples() =>
        new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadnode:gone",
            HttpStatus = StatusCodes.Status410Gone,
            Title = ProblemDetails.DefaultTitle,
            Detail = "Verwijderd wegknoop.", //TODO-pr confirm what is actually returned
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext!, RoadNodesController.PublicApiVersion)
        };
}

public class RoadNodeIdValidator : AbstractValidator<int>
{
    public RoadNodeIdValidator()
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(RoadNodeId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId);
    }
}
