namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using Commands;
    using FluentValidation;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator(WellKnownBinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            RuleFor(c => c.Changes).NotNull();
            RuleForEach(c => c.Changes).NotNull().SetValidator(new RequestedChangeValidator(reader));
        }
    }
}