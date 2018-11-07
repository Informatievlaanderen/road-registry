namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;

    public class RequestedChangeValidator : AbstractValidator<Commands.RequestedChange>
    {
        public RequestedChangeValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator(reader));
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator(reader));
        }
    }
}