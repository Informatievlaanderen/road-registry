namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using FluentValidation;
using RoadRegistry.Editor.Schema;

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext)
    {
        ChangeAttributeParametersValidator validator = new(editorContext);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}
