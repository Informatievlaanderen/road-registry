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
                new DbaseFieldName(nameof(WS_TEMPID)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(METHODE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(STATUS)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(MORF)),
                    new DbaseFieldLength(4),
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
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(AUTOTERUG)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(FIETSHEEN)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(FIETSTERUG)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VOETGANGER)),
                    new DbaseFieldLength(2),
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

    public DbaseField WS_TEMPID => Fields[0];
    public DbaseField WS_OIDN => Fields[1];
    public DbaseField METHODE => Fields[2];
    public DbaseField STATUS => Fields[3];
    public DbaseField MORF => Fields[4];
    public DbaseField WEGCAT => Fields[5];
    public DbaseField LSTRNMID => Fields[6];
    public DbaseField RSTRNMID => Fields[7];
    public DbaseField LBEHEER => Fields[8];
    public DbaseField RBEHEER => Fields[9];
    public DbaseField TOEGANG => Fields[10];
    public DbaseField VERHARDING => Fields[11];
    public DbaseField AUTOHEEN => Fields[12];
    public DbaseField AUTOTERUG => Fields[13];
    public DbaseField FIETSHEEN => Fields[14];
    public DbaseField FIETSTERUG => Fields[15];
    public DbaseField VOETGANGER => Fields[16];
    public DbaseField CREATIE => Fields[17];
    public DbaseField VERSIE => Fields[18];
}
