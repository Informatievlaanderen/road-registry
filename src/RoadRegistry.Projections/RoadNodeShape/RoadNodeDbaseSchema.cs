using Shaperon;

namespace RoadRegistry.Projections
{
    public class RoadNodeDbaseSchema
    {       
        public RoadNodeDbaseSchema()
        {
            WK_OIDN = new DbaseField(
                new DbaseFieldName(nameof(WK_OIDN)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0));

            WK_UIDN = new DbaseField(
                new DbaseFieldName(nameof(WK_UIDN)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));
            
            TYPE = new DbaseField(
                new DbaseFieldName(nameof(TYPE)),
                DbaseFieldType.Number,
                ByteOffset.Initial,
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0));
            
            LBLTYPE = new DbaseField(
                new DbaseFieldName(nameof(LBLTYPE)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(64),
                new DbaseDecimalCount(0));

            BEGINTIJD = new DbaseField(
                new DbaseFieldName(nameof(BEGINTIJD)),
                DbaseFieldType.DateTime,
                ByteOffset.Initial,
                new DbaseFieldLength(12),
                new DbaseDecimalCount(0));
            
            BEGINORG = new DbaseField(
                new DbaseFieldName(nameof(BEGINORG)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));

            LBLBGINORG = new DbaseField(
                new DbaseFieldName(nameof(LBLBGINORG)),
                DbaseFieldType.Character,
                ByteOffset.Initial,
                new DbaseFieldLength(18),
                new DbaseDecimalCount(0));
        }

        public DbaseField WK_OIDN { get; }
        public DbaseField WK_UIDN { get; }
        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField BEGINTIJD { get; }
        public DbaseField BEGINORG { get; }
        public DbaseField LBLBGINORG { get; }
    }
}