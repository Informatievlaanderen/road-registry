namespace RoadRegistry.Model
{
    using System;
    using System.Threading;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;
    using NetTopologySuite.Geometries;

    public class AddRoadSegmentValidator : AbstractValidator<Messages.AddRoadSegment>
    {
        public AddRoadSegmentValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
            RuleFor(c => c.StartNodeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.EndNodeId).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Geometry)
                .NotNull()
                .Must(data => {
                    var acceptable = false;
                    if(data != null)
                    {
                        try
                        {
                            acceptable = reader.TryReadAs<MultiLineString>(data, out _);
                        }
                        catch
                        {
                            acceptable = false;
                        }
                    }
                    return acceptable;
                })
                .WithMessage("The 'Geometry' is not a MultiLineString.");
            RuleFor(c => c.Maintainer).NotEmpty();
            RuleFor(c => c.GeometryDrawMethod).IsInEnum();
            RuleFor(c => c.Morphology).IsInEnum();
            RuleFor(c => c.Status).IsInEnum();
            RuleFor(c => c.Category).IsInEnum();
            RuleFor(c => c.AccessRestriction).IsInEnum();
            RuleFor(c => c.PartOfEuropeanRoads).NotNull();
            RuleForEach(c => c.PartOfEuropeanRoads).NotNull().SetValidator(new RoadSegmentEuropeanRoadAttributesValidator());
            RuleFor(c => c.PartOfNationalRoads).NotNull();
            RuleForEach(c => c.PartOfNationalRoads).NotNull().SetValidator(new RoadSegmentNationalRoadAttributesValidator());
            RuleFor(c => c.PartOfNumberedRoads).NotNull();
            RuleForEach(c => c.PartOfNumberedRoads).NotNull().SetValidator(new RoadSegmentNumberedRoadAttributesValidator());
            RuleFor(c => c.Lanes).NotNull();
            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RoadSegmentLaneAttributesValidator());
            RuleFor(c => c.Widths).NotNull();
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RoadSegmentWidthPropertiesValidator());
            RuleFor(c => c.Surfaces).NotNull();
            RuleForEach(c => c.Surfaces).NotNull().SetValidator(new RoadSegmentSurfaceAttributesValidator());
        }
    }
}
