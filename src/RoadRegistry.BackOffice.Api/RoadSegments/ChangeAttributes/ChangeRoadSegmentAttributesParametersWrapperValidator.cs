namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using FluentValidation;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Infrastructure;

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext, IOrganizationCache organizationCache)
    {
        ChangeAttributeParametersValidator validator = new(editorContext, organizationCache);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}
