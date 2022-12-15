namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthAttributeDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentWidthAttributeDbaseSchema
{
    public RoadSegmentWidthAttributeDbaseSchema()
    {
        Fields = Fields.Concat(new[]
        {
            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_GIDN)),
                    new DbaseFieldLength(18)),
            
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
    public DbaseField WS_GIDN => this.GetField();
}
