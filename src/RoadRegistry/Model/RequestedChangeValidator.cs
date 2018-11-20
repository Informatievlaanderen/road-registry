namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using FluentValidation;
    using Messages;

    public class RequestedChangeValidator : AbstractValidator<RequestedChange>
    {
        public RequestedChangeValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.AddRoadNode).SetValidator(new AddRoadNodeValidator());
            RuleFor(c => c.AddRoadSegment).SetValidator(new AddRoadSegmentValidator(reader));
        }
    }
}