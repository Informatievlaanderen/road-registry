namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FluentValidation;

public class PostChangeOutlineGeometryParametersValidator : AbstractValidator<PostChangeOutlineGeometryParameters>
{
    public PostChangeOutlineGeometryParametersValidator()
    {
        RuleFor(x => x.MiddellijnGeometrie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.IsRequired)
            .Must(GeometryTranslator.GmlIsValidLineString)
            .WithProblemCode(ProblemCode.RoadSegment.Geometry.NotValid);
    }
}
