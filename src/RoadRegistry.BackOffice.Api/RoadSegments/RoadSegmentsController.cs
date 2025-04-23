namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "Wegsegmenten")]
public partial class RoadSegmentsController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(BackofficeApiControllerContext context, IMediator mediator)
        : base(context)
    {
        _mediator = mediator;
    }
}

public class RoadSegmentNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProblemDetailsHelper _problemDetailsHelper;
    private readonly string _apiVersion;

    public RoadSegmentNotFoundResponseExamples(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper,
        string? apiVersion = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _problemDetailsHelper = problemDetailsHelper;
        _apiVersion = apiVersion;
    }

    public ProblemDetails GetExamples()
    {
        return new ProblemDetails
        {
            ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:roadsegment:not-found",
            HttpStatus = StatusCodes.Status404NotFound,
            Title = ProblemDetails.DefaultTitle,
            Detail = new RoadSegmentNotFound().TranslateToDutch().Message,
            ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, _apiVersion)
        };
    }
}

public class RoadSegmentNotFoundResponseExamplesV2 : RoadSegmentNotFoundResponseExamples
{
    public RoadSegmentNotFoundResponseExamplesV2(
        IHttpContextAccessor httpContextAccessor,
        ProblemDetailsHelper problemDetailsHelper)
        : base(httpContextAccessor, problemDetailsHelper, "v2")
    { }
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
