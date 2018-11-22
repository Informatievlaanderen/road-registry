namespace RoadRegistry.Model
{
    using FluentValidation;

    public class AddRoadSegmentValidator : AbstractValidator<Messages.AddRoadSegment>
    {
        public AddRoadSegmentValidator()
        {
            RuleFor(c => c.TemporaryId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.StartNodeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.EndNodeId)
                .GreaterThanOrEqualTo(0)
                .NotEqual(c => c.StartNodeId);
            RuleFor(c => c.Geometry).NotNull().SetValidator(new RoadSegmentGeometryValidator());
            RuleFor(c => c.MaintenanceAuthority).NotEmpty();
            RuleFor(c => c.GeometryDrawMethod)
                .NotEmpty()
                .Must(RoadSegmentGeometryDrawMethod.CanParse)
                .When(c => c.GeometryDrawMethod != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'GeometryDrawMethod' is not a RoadSegmentGeometryDrawMethod.");
            RuleFor(c => c.Morphology)
                .NotEmpty()
                .Must(RoadSegmentMorphology.CanParse)
                .When(c => c.Morphology != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Morphology' is not a RoadSegmentMorphology.");
            RuleFor(c => c.Status)
                .NotEmpty()
                .Must(RoadSegmentStatus.CanParse)
                .When(c => c.Status != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Status' is not a RoadSegmentStatus.");
            RuleFor(c => c.Category)
                .NotEmpty()
                .Must(RoadSegmentCategory.CanParse)
                .When(c => c.Category != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Category' is not a RoadSegmentCategory.");
            RuleFor(c => c.AccessRestriction)
                .NotEmpty()
                .Must(RoadSegmentAccessRestriction.CanParse)
                .When(c => c.AccessRestriction != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'AccessRestriction' is not a RoadSegmentAccessRestriction.");
            RuleFor(c => c.PartOfEuropeanRoads).NotNull();
            RuleForEach(c => c.PartOfEuropeanRoads).NotNull().SetValidator(new RoadSegmentEuropeanRoadAttributesValidator());
            RuleFor(c => c.PartOfNationalRoads).NotNull();
            RuleForEach(c => c.PartOfNationalRoads).NotNull().SetValidator(new RoadSegmentNationalRoadAttributesValidator());
            RuleFor(c => c.PartOfNumberedRoads).NotNull();
            RuleForEach(c => c.PartOfNumberedRoads).NotNull().SetValidator(new RoadSegmentNumberedRoadAttributesValidator());
            RuleFor(c => c.Lanes).NotNull();
            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RoadSegmentLaneAttributesValidator());
            RuleFor(c => c.Widths).NotNull();
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RoadSegmentWidthAttributesValidator());
            RuleFor(c => c.Surfaces).NotNull();
            RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RoadSegmentSurfaceAttributesValidator());
        }
    }
}
