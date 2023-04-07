namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;

public class RoadSegmentIdValidator : AbstractValidator<int>
{
    public RoadSegmentIdValidator(EditorContext editorContext)
    {
        ArgumentNullException.ThrowIfNull(editorContext);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.IsValid)
            .WithName("objectId")
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            .MustAsync((id, cancellationToken) =>
            {
                return editorContext.RoadSegments.AnyAsync(x => x.Id == id, cancellationToken);
            })
            .WithName("objectId")
            .WithProblemCode(ProblemCode.RoadSegment.NotFound);
    }
}
