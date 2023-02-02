namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using FluentValidation.Results;
using Messages;

public class RoadSegmentGeometryValidator : AbstractValidator<RoadSegmentGeometry>
{
    //TODO-rik obsolete wnr de nieuwe geometry validator er is (zie ModifyRoadSegment)
    //protected override bool PreValidate(ValidationContext<RoadSegmentGeometry> context, ValidationResult result)
    //{
    //    if (context.InstanceToValidate != null && GeometryTranslator.Translate(context.InstanceToValidate).Length < 2)
    //    {
    //        context.AddFailure(context.PropertyName, "The 'Geometry' must have a length of at least 2.");
    //    }

    //    return base.PreValidate(context, result);
    //}

    public RoadSegmentGeometryValidator()
    {
        RuleFor(c => c.SpatialReferenceSystemIdentifier).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MultiLineString)
            .NotNull()
            .MaximumLength(1)
            .When(c => !ReferenceEquals(c.MultiLineString, null), ApplyConditionTo.CurrentValidator);
        RuleForEach(c => c.MultiLineString).NotNull().SetValidator(new LineStringValidator());
    }
}
