// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV1.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentStatusDbaseSchema : DbaseSchema
{
    public RoadSegmentStatusDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(STATUS)),
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLSTATUS)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFSTATUS)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFSTATUS => Fields[2];
    public DbaseField LBLSTATUS => Fields[1];
    public DbaseField STATUS => Fields[0];
}
