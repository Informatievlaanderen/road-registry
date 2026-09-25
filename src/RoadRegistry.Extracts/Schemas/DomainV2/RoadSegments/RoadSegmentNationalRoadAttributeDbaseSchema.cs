namespace RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentNationalRoadAttributeDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(NW_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_TEMPID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(NWNUMMER)),
                    new DbaseFieldLength(8)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15))
        ];
    }

    public DbaseField NW_OIDN => Fields[0];
    public DbaseField WS_TEMPID => Fields[1];
    public DbaseField NWNUMMER => Fields[2];
    public DbaseField CREATIE => Fields[3];
}
