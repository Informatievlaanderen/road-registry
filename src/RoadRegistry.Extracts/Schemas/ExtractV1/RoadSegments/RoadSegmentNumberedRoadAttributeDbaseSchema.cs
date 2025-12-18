namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNumberedRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentNumberedRoadAttributeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(GW_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(IDENT8)),
                    new DbaseFieldLength(8)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RICHTING)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLRICHT)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VOLGNUMMER)),
                    new DbaseFieldLength(5),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINTIJD)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLBGNORG)),
                    new DbaseFieldLength(64))
        };
    }

    public DbaseField BEGINORG => Fields[7];
    public DbaseField BEGINTIJD => Fields[6];
    public DbaseField GW_OIDN => Fields[0];
    public DbaseField IDENT8 => Fields[2];
    public DbaseField LBLBGNORG => Fields[8];
    public DbaseField LBLRICHT => Fields[4];
    public DbaseField RICHTING => Fields[3];
    public DbaseField VOLGNUMMER => Fields[5];
    public DbaseField WS_OIDN => Fields[1];
}
