namespace RoadRegistry.Projections
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Events;
    using Shaperon;

    public class TypeReferences
    {
        public static RoadSegmentAccessRestrictionRecord[] RoadSegmentAccessRestrictions => CreateDbaseRecords<RoadSegmentAccessRestrictionRecord, RoadSegmentAccessRestriction>();
        public static RoadSegmentGeometryDrawMethodRecord[] RoadSegmentGeometryDrawMethodRecords => CreateDbaseRecords<RoadSegmentGeometryDrawMethodRecord, RoadSegmentGeometryDrawMethod>();
        public static RoadSegmentStatusRecord[] RoadSegmentStatusRecords => CreateDbaseRecords<RoadSegmentStatusRecord, RoadSegmentStatus>();
        public static ReferencePointTypeDbaseRecord[] ReferencePointTypes => CreateDbaseRecords<ReferencePointTypeDbaseRecord, ReferencePointType>();
        public static GradeSeparatedJunctionTypeRecord[] GradeSeparatedJunctionTypes => CreateDbaseRecords<GradeSeparatedJunctionTypeRecord, GradeSeparatedJunctionType>();

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

        h3. WegverhardLktType

        Lijst met mogelijke wegverhardingen.

        h4. DBF Mapping

        || DBF Column || Legacy DB Column || Enum Mapping ||
        | TYPE | code | CodeForEach(HardeningType) |
        | LBLTYPE | label | LabelForEach(HardeningType) |
        | DEFTYPE | definitie | DescriptionForEach(HardeningType) |

        h4. DBF Description

        || Name || DataType || Length || Decimal ||
        | TYPE | Number | 2 | 0 |
        | LBLTYPE | Character | 64 | 0 |
        | DEFTYPE | Character | 255 | 0 |

        h3. WegknoopLktType

        Lijst met mogelijke wegknoop types.

        h4. DBF Mapping

        || DBF Column || Legacy DB Column || Enum Mapping ||
        | TYPE | code | CodeForEach(RoadNodeType) |
        | LBLTYPE | label | LabelForEach(RoadNodeType) |
        | DEFTYPE | definitie | DescriptionForEach(RoadNodeType) |

        h4. DBF Description

        || Name || DataType || Length || Decimal ||
        | TYPE | Number | 2 | 0 |
        | LBLTYPE | Character | 64 | 0 |
        | DEFTYPE | Character | 255 | 0 |

        h3. WegsegmentLktMorf

        Lijst met mogelijke wegmorfologiën die aan een wegsegment kunnen worden toegekend.

        h4. DBF Mapping

        || DBF Column || Legacy DB Column || Enum Mapping ||
        | MORF | code | CodeForEach(RoadSegmentMorphology) |
        | LBLMORF | label | LabelForEach(RoadSegmentMorphology) |
        | DEFMORF | definitie | DescriptionForEach(RoadSegmentMorphology) |

        h4. DBF Description

        || Name || DataType || Length || Decimal ||
        | MORF | Number | 3 | 0 |
        | LBLMORF | Character | 64 | 0 |
        | DEFMORF | Character | 255 | 0 |

         */
    }

    /*

     */
}
