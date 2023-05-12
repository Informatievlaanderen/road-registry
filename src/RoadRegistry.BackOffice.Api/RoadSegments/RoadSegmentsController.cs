namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Runtime.Serialization;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Core;
using Core.ProblemCodes;
using Editor.Projections;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Hosts.Infrastructure.Options;
using Infrastructure.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Newtonsoft.Json;
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

    public RoadSegmentsController(TicketingOptions ticketingOptions, IMediator mediator)
        : base(ticketingOptions)
    {
        _mediator = mediator;
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

public class RoadSegmentLaneParameters
{
    /// <summary>Aantal rijstroken van de wegsegmentschets.</summary>
    [JsonProperty]
    [DataMember(Name = "Aantal", Order = 1)]
    public int? Aantal { get; set; }

    /// <summary>De richting van deze rijstroken t.o.v. de richting van het wegsegment (begin- naar eindknoop).</summary>
    [JsonProperty]
    [DataMember(Name = "Richting", Order = 2)]
    public string Richting { get; set; }
}

public class RoadSegmentLaneParametersValidator : AbstractValidator<RoadSegmentLaneParameters>
{
    public RoadSegmentLaneParametersValidator()
    {
        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Lane.IsRequired)
            .GreaterThan(0)
            .WithProblemCode(ProblemCode.RoadSegment.Lane.GreaterThanZero)
            .LessThanOrEqualTo(RoadSegmentLaneCount.Maximum)
            .WithProblemCode(ProblemCode.RoadSegment.Lane.LessThanOrEqualToMaximum);

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.IsRequired)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.RoadSegment.LaneDirection.NotValid);
    }
}

public class RoadSegmentIdValidator : AbstractValidator<int>
{
    public RoadSegmentIdValidator(EditorContext editorContext)
    {
        ArgumentNullException.ThrowIfNull(editorContext);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            .MustAsync((id, cancellationToken) => { return editorContext.RoadSegments.AnyAsync(x => x.Id == id, cancellationToken); })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.NotFound);
    }
}

public class RoadSegmentOutlinedIdValidator : AbstractValidator<int>
{
    public RoadSegmentOutlinedIdValidator(EditorContext editorContext, RecyclableMemoryStreamManager recyclableMemoryStreamManager, FileEncoding fileEncoding)
    {
        ArgumentNullException.ThrowIfNull(editorContext);

        RuleFor(x => x).Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.Accepts)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            .MustAsync((id, cancellationToken) => { return editorContext.RoadSegments.AnyAsync(x => x.Id == id, cancellationToken); })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.NotFound)
            .MustAsync(async (id, cancellationToken) =>
            {
                var roadSegment = await editorContext.RoadSegments.FindAsync(new object[] { id }, cancellationToken);
                var dbaseRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment!.DbaseRecord, recyclableMemoryStreamManager, fileEncoding);
                return dbaseRecord.METHODE.Value == RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
            })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.GeometryDrawMethod.NotOutlined);
    }
}