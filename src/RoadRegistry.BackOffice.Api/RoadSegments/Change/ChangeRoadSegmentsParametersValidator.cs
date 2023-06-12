namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System.Collections.Generic;
using System.Linq;
using Editor.Schema;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Core.ProblemCodes;

public class ChangeRoadSegmentsParametersValidator : AbstractValidator<ChangeRoadSegmentsParameters>
{
    private readonly EditorContext _editorContext;

    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentsParameters> context, ValidationResult result)
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

    public ChangeRoadSegmentsParametersValidator(EditorContext editorContext)
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
            .SetValidator(new ChangeRoadSegmentParametersValidator(context));
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
