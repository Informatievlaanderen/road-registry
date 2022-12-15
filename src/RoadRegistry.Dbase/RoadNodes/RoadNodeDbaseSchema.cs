namespace RoadRegistry.Dbase.RoadNodes;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadNodeDbaseSchema
{
    public RoadNodeDbaseSchema()
    {
        Fields = Fields.Concat(new[]
        {
            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WK_UIDN)),
                    new DbaseFieldLength(18)),

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
        }).ToArray();
    }

    public DbaseField BEGINORG => this.GetField();
    public DbaseField BEGINTIJD => this.GetField();
    public DbaseField LBLBGNORG => this.GetField();
    public DbaseField LBLTYPE => this.GetField();
    public DbaseField WK_UIDN => this.GetField();
}
