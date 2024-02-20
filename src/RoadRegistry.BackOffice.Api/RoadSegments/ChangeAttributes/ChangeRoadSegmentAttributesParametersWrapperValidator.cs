namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using FluentValidation;
using RoadRegistry.Editor.Schema;

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext, IOrganizationCache organizationCache)
    {
        ChangeAttributeParametersValidator validator = new(editorContext, organizationCache);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}
