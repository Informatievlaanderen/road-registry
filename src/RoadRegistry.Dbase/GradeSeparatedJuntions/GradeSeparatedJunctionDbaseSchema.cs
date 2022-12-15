namespace RoadRegistry.Dbase.GradeSeparatedJuntions;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.GradeSeparatedJunctionDbaseSchema
{
    public GradeSeparatedJunctionDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64)),
            
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

    public DbaseField BEGINORG => this.GetField();
    public DbaseField BEGINTIJD => this.GetField();
    public DbaseField LBLBGNORG => this.GetField();
    public DbaseField LBLTYPE => this.GetField();
}
