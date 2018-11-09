namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;

    public class AddRoadNodeValidator : AbstractValidator<Messages.AddRoadNode>
    {
        public AddRoadNodeValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.Id).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Type)
                .NotEmpty()
                .Must(RoadNodeType.CanParse)
                .When(c => c.Type != null, ApplyConditionTo.CurrentValidator)
                .WithMessage("The 'Type' is not a RoadNodeType.");
            RuleFor(c => c.Geometry)
                .NotNull()
                .Must(data => {
                    var acceptable = false;
                    try
                    {
                        acceptable = reader.TryReadAs<PointM>(data, out _);
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
