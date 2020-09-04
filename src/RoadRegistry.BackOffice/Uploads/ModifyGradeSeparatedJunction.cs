namespace RoadRegistry.BackOffice.Uploads
{
    using System;

    public class ModifyGradeSeparatedJunction : ITranslatedChange
    {
        public ModifyGradeSeparatedJunction(
            GradeSeparatedJunctionId id,
            GradeSeparatedJunctionType type,
            RoadSegmentId upperSegmentId,
            RoadSegmentId lowerSegmentId)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            UpperSegmentId = upperSegmentId;
            LowerSegmentId = lowerSegmentId;
        }
        public GradeSeparatedJunctionId Id { get; }
        public GradeSeparatedJunctionType Type { get; }
        public RoadSegmentId UpperSegmentId { get; }
        public RoadSegmentId LowerSegmentId { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyGradeSeparatedJunction = new Messages.ModifyGradeSeparatedJunction
            {
                Id = Id,
                Type = Type.ToString(),
                UpperSegmentId = UpperSegmentId,
                LowerSegmentId = LowerSegmentId
            };
        }
    }
}
