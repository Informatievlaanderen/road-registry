namespace RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseSchema : DbaseSchema
{
    public RoadNodeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WK_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0))
        };
    }

    public DbaseField TYPE => this.GetField();
    public DbaseField WK_OIDN => this.GetField();
}
