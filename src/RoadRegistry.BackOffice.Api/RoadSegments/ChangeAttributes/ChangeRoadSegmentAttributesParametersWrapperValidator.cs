namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using FluentValidation;
using RoadRegistry.Editor.Schema;

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext, IOrganizationRepository organizationRepository)
    {
        ChangeAttributeParametersValidator validator = new(editorContext, organizationRepository);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}
