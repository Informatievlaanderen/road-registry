// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentDbaseSchema : DbaseSchema
{
    public RoadSegmentDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_UIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_GIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(B_WK_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(E_WK_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(STATUS)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLSTATUS)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(MORF)),
                    new DbaseFieldLength(3),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLMORF)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WEGCAT)),
                    new DbaseFieldLength(5)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLWEGCAT)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(LSTRNMID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LSTRNM)),
                    new DbaseFieldLength(80)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RSTRNMID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(RSTRNM)),
                    new DbaseFieldLength(80)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEHEER)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLBEHEER)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(METHODE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLMETHOD)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(OPNDATUM)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINTIJD)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TGBEP)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTGBEP)),
                    new DbaseFieldLength(64))
        };
    }

    public DbaseField WS_OIDN => Fields[0];
    public DbaseField WS_UIDN => Fields[1];
    public DbaseField WS_GIDN => Fields[2];
    public DbaseField B_WK_OIDN => Fields[3];
    public DbaseField E_WK_OIDN => Fields[4];
    public DbaseField STATUS => Fields[5];
    public DbaseField LBLSTATUS => Fields[6];
    public DbaseField MORF => Fields[7];
    public DbaseField LBLMORF => Fields[8];
    public DbaseField WEGCAT => Fields[9];
    public DbaseField LBLWEGCAT => Fields[10];
    public DbaseField LSTRNMID => Fields[11];
    public DbaseField LSTRNM => Fields[12];
    public DbaseField RSTRNMID => Fields[13];
    public DbaseField RSTRNM => Fields[14];
    public DbaseField BEHEER => Fields[15];
    public DbaseField LBLBEHEER => Fields[16];
    public DbaseField METHODE => Fields[17];
    public DbaseField LBLMETHOD => Fields[18];
    public DbaseField OPNDATUM => Fields[19];
    public DbaseField BEGINTIJD => Fields[20];
    public DbaseField BEGINORG => Fields[21];
    public DbaseField TGBEP => Fields[22];
    public DbaseField LBLTGBEP => Fields[23];
}
