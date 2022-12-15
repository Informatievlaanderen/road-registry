namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceAttributeDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentSurfaceAttributeDbaseSchema
{
    public RoadSegmentSurfaceAttributeDbaseSchema()
    {
        Fields = Fields.Concat(new[]
        {
            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_GIDN)),
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

    public DbaseField BEGINORG => Fields[8];
    public DbaseField BEGINTIJD => Fields[7];
    public DbaseField LBLBGNORG => Fields[9];
    public DbaseField LBLTYPE => Fields[4];
    public DbaseField WS_GIDN => Fields[2];
}
