namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using ProblemCodes;

public class RoadSegmentNationalRoadAttributeValidator : AbstractValidator<Messages.RoadSegmentNationalRoadAttribute>
{
    public RoadSegmentNationalRoadAttributeValidator()
    {
        RuleFor(c => c.AttributeId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.NationalRoad.IsRequired)
            .Must(NumberedRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.NationalRoad.NotValid);
    }
}
