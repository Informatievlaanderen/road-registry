namespace RoadRegistry.Projections
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Events;
    using Shaperon;

    public class TypeReferences
    {
        public static RoadNodeTypeDbaseRecord[] RoadNodeTypes => CreateDbaseRecords<RoadNodeTypeDbaseRecord, RoadNodeType>();
        public static RoadSegmentAccessRestrictionDbaseRecord[] RoadSegmentAccessRestrictions => CreateDbaseRecords<RoadSegmentAccessRestrictionDbaseRecord, RoadSegmentAccessRestriction>();
        public static RoadSegmentGeometryDrawMethodDbaseRecord[] RoadSegmentGeometryDrawMethods => CreateDbaseRecords<RoadSegmentGeometryDrawMethodDbaseRecord, RoadSegmentGeometryDrawMethod>();
        public static RoadSegmentStatusDbaseRecord[] RoadSegmentStatuses => CreateDbaseRecords<RoadSegmentStatusDbaseRecord, RoadSegmentStatus>();
        public static RoadSegmentMorphologyDbaseRecord[] RoadSegmentMorphologies => CreateDbaseRecords<RoadSegmentMorphologyDbaseRecord, RoadSegmentMorphology>();
        public static HardeningTypeDbaseRecord[] HardeningTypes => CreateDbaseRecords<HardeningTypeDbaseRecord, HardeningType>();
        public static ReferencePointTypeDbaseRecord[] ReferencePointTypes => CreateDbaseRecords<ReferencePointTypeDbaseRecord, ReferencePointType>();
        public static GradeSeparatedJunctionTypeDbaseRecord[] GradeSeparatedJunctionTypes => CreateDbaseRecords<GradeSeparatedJunctionTypeDbaseRecord, GradeSeparatedJunctionType>();

        private static TDbaseRecord[] CreateDbaseRecords<TDbaseRecord, TEnum>()
            where TDbaseRecord : DbaseRecord
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            return ((TEnum[]) Enum.GetValues(typeof(TEnum)))
                .OrderBy(value => value.ToInt32(CultureInfo.InvariantCulture))
                .Select(value => (TDbaseRecord)Activator.CreateInstance(typeof(TDbaseRecord), value))
                .ToArray();
        }


        /*


        h3. WegsegmentLktWegcat

        Lijst met de mogelijke wegcategorieën tot dewelke een wegsegment kan behoren.

        h4. DBF Mapping

        || DBF Column || Legacy DB Column || Enum Mapping ||
        | WEGCAT | code | CodeForEach(RoadSegmentCategory) |
        | LBLWEGCAT | label | LabelForEach(RoadSegmentCategory) |
        | DEFWEGCAT | definitie | DescriptionForEach(RoadSegmentCategory) |

        h4. DBF Description

        || Name || DataType || Length || Decimal ||
        | WEGCAT | Character | 5 | 0 |
        | LBLWEGCAT | Character | 64 | 0 |
        | DEFWEGCAT | Character | 255 | 0 |

        h3. GenumwegLktRichting

        Lijst met mogelijke richtingen van een genummerde weg.

        h4. DBF Mapping

        || DBF Column || Legacy DB Column || Enum Mapping ||
        | RICHTING | code | CodeForEach(NumberedRoadSegmentDirection) |
        | LBLRICHT | label | LabelForEach(NumberedRoadSegmentDirection) |
        | DEFRICHT | definitie | DescriptionForEach(NumberedRoadSegmentDirection) |

        h4. DBF Description

        || Name || DataType || Length || Decimal ||
        | RICHTING | Number | 2 | 0 |
        | LBLRICHT | Character | 64 | 0 |
        | DEFRICHT | Character | 255 | 0 |

         */
    }
}
