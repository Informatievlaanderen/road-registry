namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class AddGradeSeparatedJunction : ITranslatedChange
{
    public AddGradeSeparatedJunction(
        RecordNumber recordNumber,
        GradeSeparatedJunctionId temporaryId,
        GradeSeparatedJunctionType type,
        RoadSegmentId upperSegmentId,
        RoadSegmentId lowerSegmentId)
    {
        RecordNumber = recordNumber;
        TemporaryId = temporaryId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        UpperSegmentId = upperSegmentId;
        LowerSegmentId = lowerSegmentId;
    }

    public RoadSegmentId LowerSegmentId { get; }
    public RecordNumber RecordNumber { get; }
    public GradeSeparatedJunctionId TemporaryId { get; }
    public GradeSeparatedJunctionType Type { get; }
    public RoadSegmentId UpperSegmentId { get; }

    public void TranslateTo(RequestedChange message)
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
