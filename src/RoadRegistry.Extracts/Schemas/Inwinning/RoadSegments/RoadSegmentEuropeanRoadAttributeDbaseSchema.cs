namespace RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentEuropeanRoadAttributeDbaseSchema()
    {
        Fields =
        [
            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(EU_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_TEMPID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(EUNUMMER)),
                    new DbaseFieldLength(4)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15))
        ];
    }

    public DbaseField EU_OIDN => Fields[0];
    public DbaseField WS_TEMPID => Fields[1];
    public DbaseField EUNUMMER => Fields[2];
    public DbaseField CREATIE => Fields[3];
}
