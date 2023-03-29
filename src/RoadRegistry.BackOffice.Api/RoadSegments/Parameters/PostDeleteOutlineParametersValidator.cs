namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using FluentValidation;
using Abstractions.Validation;

public class PostDeleteOutlineParametersValidator : AbstractValidator<PostDeleteOutlineParameters>
{
    public PostDeleteOutlineParametersValidator()
    {
        RuleFor(x => x.WegsegmentId)
            .Cascade(CascadeMode.Stop)
            .Must(x => int.TryParse(x, out int roadSegmentId) && RoadSegmentId.Accepts(roadSegmentId))
            .WithErrorCode(ValidationErrors.Common.IncorrectObjectId.Code)
            .WithMessage(x => ValidationErrors.Common.IncorrectObjectId.Message(x.WegsegmentId))
            ;
    }
}
