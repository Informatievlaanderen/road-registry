namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Commands;
    using FluentValidation;

    public class AddRoadNodeValidator : AbstractValidator<Commands.AddRoadNode>
    {
        public AddRoadNodeValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Type).IsInEnum();
            RuleFor(c => c.Geometry)
                .NotNull()
                .Must(data => {
                    var acceptable = false;
                    try
                    {
                        acceptable = reader.TryReadAs<PointM>(data, out PointM ignored);
                    }
                    catch
                    {
                    }
                    return acceptable;
                })
                .When(c => c.Geometry != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Geometry' is not a PointM.");
        }
    }
}
