namespace RoadRegistry.BackOffice.Uploads.BeforeFeatureCompare.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentEuropeanRoadAttributeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(EU_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(EUNUMMER)),
                    new DbaseFieldLength(4))
        };
    }

    public DbaseField EU_OIDN => this.GetField();
    public DbaseField EUNUMMER => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
