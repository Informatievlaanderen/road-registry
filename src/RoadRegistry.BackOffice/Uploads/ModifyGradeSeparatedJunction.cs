namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class ModifyGradeSeparatedJunction : ITranslatedChange
{
    public ModifyGradeSeparatedJunction(
        RecordNumber recordNumber,
        GradeSeparatedJunctionId id,
        GradeSeparatedJunctionType type,
        RoadSegmentId upperSegmentId,
        RoadSegmentId lowerSegmentId)
    {
        RecordNumber = recordNumber;
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        UpperSegmentId = upperSegmentId;
        LowerSegmentId = lowerSegmentId;
    }

    public RecordNumber RecordNumber { get; }
    public GradeSeparatedJunctionId Id { get; }
    public GradeSeparatedJunctionType Type { get; }
    public RoadSegmentId UpperSegmentId { get; }
    public RoadSegmentId LowerSegmentId { get; }

    public void TranslateTo(RequestedChange message)
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
