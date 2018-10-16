namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;

    public class RoadNetworkChangeValidator : AbstractValidator<Commands.RoadNetworkChange>
    {
        public RoadNetworkChangeValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator(reader));
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator());
        }
    }
}