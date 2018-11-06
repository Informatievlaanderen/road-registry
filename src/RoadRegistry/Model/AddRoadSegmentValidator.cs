namespace RoadRegistry.Model
{
    using System;
    using System.Threading;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;
    using NetTopologySuite.Geometries;

    public class AddRoadSegmentValidator : AbstractValidator<Commands.AddRoadSegment>
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
            RuleForEach(c => c.PartOfEuropeanRoads).NotNull().SetValidator(new RoadSegmentEuropeanRoadPropertiesValidator());
            RuleFor(c => c.PartOfNationalRoads).NotNull();
            RuleForEach(c => c.PartOfNationalRoads).NotNull().SetValidator(new RoadSegmentNationalRoadPropertiesValidator());
            RuleFor(c => c.PartOfNumberedRoads).NotNull();
            RuleForEach(c => c.PartOfNumberedRoads).NotNull().SetValidator(new RoadSegmentNumberedRoadPropertiesValidator());
            RuleFor(c => c.Lanes)
                .NotNull()
                .Must(BeAdjacent)
                    .When(c => c.Lanes != null, ApplyConditionTo.CurrentValidator)
                    .WithMessage("The lanes must be adjacent to one another, meaning their positions must line up.");
            RuleForEach(c => c.Lanes).NotNull().SetValidator(new RoadSegmentLanePropertiesValidator());
            RuleFor(c => c.Widths)
                .NotNull()
                .Must(BeAdjacent)
                    .When(c => c.Widths != null, ApplyConditionTo.CurrentValidator)
                    .WithMessage("The widths must be adjacent to one another, meaning their positions must line up.");
            RuleForEach(c => c.Widths).NotNull().SetValidator(new RoadSegmentWidthPropertiesValidator());
            RuleFor(c => c.Hardenings)
                .NotNull()
                .Must(BeAdjacent)
                    .When(c => c.Hardenings != null, ApplyConditionTo.CurrentValidator)
                    .WithMessage("The hardenings must be adjacent to one another, meaning their positions must line up.");
            RuleForEach(c => c.Hardenings).NotNull().SetValidator(new RoadSegmentHardeningPropertiesValidator());
        }

        private static bool BeAdjacent(Commands.RoadSegmentLaneProperties[] properties)
        {
            var position = new RoadSegmentPosition(0.0);
            foreach (var property in properties)
            {
                if (property == null)
                {
                    return false;
                }

                if(!RoadSegmentPosition.IsWellformed(property.FromPosition)
                    || !RoadSegmentPosition.IsWellformed(property.ToPosition))
                {
                    return false;
                }

                if (new RoadSegmentPosition(property.FromPosition) != position)
                {
                    return false;
                }

                position = new RoadSegmentPosition(property.ToPosition);
            }

            return true;
        }

        private static bool BeAdjacent(Commands.RoadSegmentWidthProperties[] properties)
        {
            var position = new RoadSegmentPosition(0.0);
            foreach (var property in properties)
            {
                if (property == null)
                {
                    return false;
                }

                if(!RoadSegmentPosition.IsWellformed(property.FromPosition)
                   || !RoadSegmentPosition.IsWellformed(property.ToPosition))
                {
                    return false;
                }

                if (new RoadSegmentPosition(property.FromPosition) != position)
                {
                    return false;
                }

                position = new RoadSegmentPosition(property.ToPosition);
            }

            return true;
        }

        private static bool BeAdjacent(Commands.RoadSegmentHardeningProperties[] properties)
        {
            var position = new RoadSegmentPosition(0.0);
            foreach (var property in properties)
            {
                if (property == null)
                {
                    return false;
                }

                if(!RoadSegmentPosition.IsWellformed(property.FromPosition)
                   || !RoadSegmentPosition.IsWellformed(property.ToPosition))
                {
                    return false;
                }

                if (new RoadSegmentPosition(property.FromPosition) != position)
                {
                    return false;
                }

                position = new RoadSegmentPosition(property.ToPosition);
            }

            return true;
        }
    }
}
