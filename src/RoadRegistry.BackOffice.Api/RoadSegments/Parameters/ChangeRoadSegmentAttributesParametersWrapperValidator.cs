namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Editor.Schema;
using FluentValidation;

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext)
    {
        ChangeAttributeParametersValidator validator = new(editorContext);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}
