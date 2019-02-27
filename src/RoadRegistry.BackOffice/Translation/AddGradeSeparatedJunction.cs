namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public class AddGradeSeparatedJunction : ITranslatedChange
    {
        public AddGradeSeparatedJunction(
            GradeSeparatedJunctionId temporaryId,
            GradeSeparatedJunctionType type,
            RoadSegmentId upperSegmentId,
            RoadSegmentId lowerSegmentId)
        {
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            UpperSegmentId = upperSegmentId;
            LowerSegmentId = lowerSegmentId;
        }
        public GradeSeparatedJunctionId TemporaryId { get; }
        public GradeSeparatedJunctionType Type { get; }
        public RoadSegmentId UpperSegmentId { get; }
        public RoadSegmentId LowerSegmentId { get; }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                UpperSegmentId = UpperSegmentId,
                LowerSegmentId = LowerSegmentId
            };
        }
    }
}
