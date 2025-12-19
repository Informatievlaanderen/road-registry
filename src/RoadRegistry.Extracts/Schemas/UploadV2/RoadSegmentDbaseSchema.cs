// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

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
                    new DbaseFieldName(nameof(BEHEER)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(METHODE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TGBEP)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0))
        };
    }

    public DbaseField B_WK_OIDN => this.GetField();
    public DbaseField BEHEER => this.GetField();
    public DbaseField E_WK_OIDN => this.GetField();
    public DbaseField LSTRNMID => this.GetField();
    public DbaseField METHODE => this.GetField();
    public DbaseField MORF => this.GetField();
    public DbaseField RSTRNMID => this.GetField();
    public DbaseField STATUS => this.GetField();
    public DbaseField TGBEP => this.GetField();
    public DbaseField WEGCAT => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
