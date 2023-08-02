namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Editor.Schema;

public class ChangeRoadSegmentsDynamicAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentsDynamicAttributesParameters>
{
    private readonly EditorContext _editorContext;

    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentsDynamicAttributesParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null && context.InstanceToValidate.Any())
        {
            return true;
        }

        context.AddFailure(new ValidationFailure
        {
            PropertyName = "request",
            ErrorCode = ProblemCode.RoadSegment.ChangeAttributesRequestNull
        });

        return false;
    }

    public ChangeRoadSegmentsDynamicAttributesParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext;

        var context = new ChangeRoadSegmentsParametersValidatorContext();

        RuleFor(request => request)
            .Cascade(CascadeMode.Continue)
            .Must(request =>
            {
                LoadNonExistingOrRemovedRoadSegmentIds(request
                    .Where(x => x is not null && x.WegsegmentId is not null && RoadSegmentId.Accepts(x.WegsegmentId.Value))
                    .Select(x => x.WegsegmentId.Value)
                    .ToArray(), context);
                return true;
            });
        
        RuleForEach(request => request)
            .NotNull()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .SetValidator(new ChangeRoadSegmentDynamicAttributesParametersValidator(context));
    }
    
    private IEnumerable<int> FindRoadSegmentIds(IEnumerable<int> ids)
    {
        return _editorContext.RoadSegments
            .Select(s => s.Id)
            .Where(roadSegmentId => ids.Contains(roadSegmentId));
    }

    private void LoadNonExistingOrRemovedRoadSegmentIds(ICollection<int> ids, ChangeRoadSegmentsParametersValidatorContext context)
    {
        var nonExistingOrRemovedRoadSegmentIds = ids.Except(FindRoadSegmentIds(ids)).ToArray();
        context.AddNonExistingOrRemovedRoadSegmentIds(nonExistingOrRemovedRoadSegmentIds);
    }
}

public class ChangeRoadSegmentsParametersValidatorContext
{
    private readonly List<int> _nonExistingOrRemovedRoadSegmentIds = new();

    public void AddNonExistingOrRemovedRoadSegmentIds(int[] ids)
    {
        _nonExistingOrRemovedRoadSegmentIds.AddRange(ids);
    }

    public bool RoadSegmentExists(int id)
    {
        return !_nonExistingOrRemovedRoadSegmentIds.Contains(id);
    }
}
