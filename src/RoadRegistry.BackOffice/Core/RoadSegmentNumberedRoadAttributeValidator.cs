namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using ProblemCodes;

public class RoadSegmentNumberedRoadAttributeValidator : AbstractValidator<Messages.RoadSegmentNumberedRoadAttribute>
{
    public RoadSegmentNumberedRoadAttributeValidator()
    {
        RuleFor(c => c.AttributeId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.IsRequired)
            .Must(NumberedRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoad.NotValid);

        RuleFor(x => x.Direction)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadDirection.IsRequired)
            .Must(RoadSegmentNumberedRoadDirection.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadDirection.NotValid);

        RuleFor(x => x.Ordinal)
            .Cascade(CascadeMode.Stop)
            .Must(RoadSegmentNumberedRoadOrdinal.Accepts)
            .WithProblemCode(ProblemCode.RoadSegment.NumberedRoadOrdinal.NotValid);
    }
}
