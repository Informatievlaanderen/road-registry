namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class PostDeleteOutlineParametersValidator : AbstractValidator<PostDeleteOutlineParameters>
{
    public PostDeleteOutlineParametersValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .Cascade(CascadeMode.Stop)
            .Must(RoadSegmentId.IsValid)
            .WithProblemCode(ProblemCode.Common.IncorrectObjectId)
            ;
    }
}
