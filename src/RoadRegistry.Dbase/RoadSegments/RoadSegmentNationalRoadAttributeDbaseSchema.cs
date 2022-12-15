namespace RoadRegistry.Dbase.RoadSegments;

using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseSchema : BackOffice.Uploads.BeforeFeatureCompare.Schema.RoadSegmentNationalRoadAttributeDbaseSchema
{
    public RoadSegmentNationalRoadAttributeDbaseSchema()
    {
        Fields = Fields.Concat(new[]
        {
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
    
    public DbaseField BEGINORG => Fields[4];
    public DbaseField BEGINTIJD => Fields[3];
    public DbaseField LBLBGNORG => Fields[5];
}
