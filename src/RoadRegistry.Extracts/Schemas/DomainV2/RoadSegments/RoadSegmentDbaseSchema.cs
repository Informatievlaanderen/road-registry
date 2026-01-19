// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseSchema : DbaseSchema
{
    public RoadSegmentDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WS_TEMPID)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(STATUS)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(MORF)),
                    new DbaseFieldLength(3),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WEGCAT)),
                    new DbaseFieldLength(5)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(LSTRNMID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RSTRNMID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBEHEER)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(RBEHEER)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TOEGANG)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VERHARDING)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(AUTOHEEN)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(AUTOTERUG)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(FIETSHEEN)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(FIETSTERUG)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VOETGANGER)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(VERSIE)),
                    new DbaseFieldLength(15))
        ];
    }

    public DbaseField WS_OIDN => Fields[0];
    public DbaseField WS_TEMPID => Fields[1];
    public DbaseField STATUS => Fields[2];
    public DbaseField MORF => Fields[3];
    public DbaseField WEGCAT => Fields[4];
    public DbaseField LSTRNMID => Fields[5];
    public DbaseField RSTRNMID => Fields[6];
    public DbaseField LBEHEER => Fields[7];
    public DbaseField RBEHEER => Fields[8];
    public DbaseField TOEGANG => Fields[9];
    public DbaseField VERHARDING => Fields[10];
    public DbaseField AUTOHEEN => Fields[11];
    public DbaseField AUTOTERUG => Fields[12];
    public DbaseField FIETSHEEN => Fields[13];
    public DbaseField FIETSTERUG => Fields[14];
    public DbaseField VOETGANGER => Fields[15];
    public DbaseField CREATIE => Fields[16];
    public DbaseField VERSIE => Fields[17];
}
