namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using Aiv.Vbr.Shaperon;
    using Xunit;

    public class TypeReferenceTests
    {
        [Fact]
        public void All_road_node_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadNodeTypes,
                new[]
                {
                    new RoadNodeTypeDbaseRecord(RoadNodeType.RealNode),
                    new RoadNodeTypeDbaseRecord(RoadNodeType.FakeNode),
                    new RoadNodeTypeDbaseRecord(RoadNodeType.EndNode),
                    new RoadNodeTypeDbaseRecord(RoadNodeType.MiniRoundabout),
                    new RoadNodeTypeDbaseRecord(RoadNodeType.TurnLoopNode),
                });
        }

        [Fact]
        public void All_road_segment_access_restriction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentAccessRestrictions,
                new[]
                {
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.PublicRoad),
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.PhysicallyImpossible),
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.LegallyForbidden),
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.PrivateRoad),
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.Seasonal),
                    new RoadSegmentAccessRestrictionDbaseRecord(RoadSegmentAccessRestriction.Toll),
                });
        }

        [Fact]
        public void All_road_segment_geometry_draw_method_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentGeometryDrawMethods,
                new[]
                {
                    new RoadSegmentGeometryDrawMethodDbaseRecord(RoadSegmentGeometryDrawMethod.Outlined),
                    new RoadSegmentGeometryDrawMethodDbaseRecord(RoadSegmentGeometryDrawMethod.Measured),
                    new RoadSegmentGeometryDrawMethodDbaseRecord(RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications),
                });
        }

        [Fact]
        public void All_road_segment_status_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentStatuses,
                new[]
                {
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.Unknown),
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.PermitRequested),
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.BuildingPermitGranted),
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.UnderConstruction),
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.InUse),
                    new RoadSegmentStatusDbaseRecord(RoadSegmentStatus.OutOfUse),
                });
        }

        [Fact]
        public void All_road_segment_morphology_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentMorphologies,
                new[]
                {
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Unknown),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Motorway),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Road_consisting_of_one_roadway),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Traffic_circle),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Special_traffic_situation),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Traffic_square),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Parallel_road),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Frontage_road),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Entry_or_exit_of_a_car_park),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Entry_or_exit_of_a_service),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Pedestrain_zone),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Service_road),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Primitive_road),
                    new RoadSegmentMorphologyDbaseRecord(RoadSegmentMorphology.Ferry)
                });
        }

        [Fact]
        public void All_road_segment_category_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentCategories,
                new[]
                {
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.Unknown),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.NotApplicable),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.MainRoad),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.LocalRoad),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.LocalRoadType1),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.LocalRoadType2),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.LocalRoadType3),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadI),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadII),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadIIType1),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadIIType2),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadIIType3),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.PrimaryRoadIIType4),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.SecondaryRoad),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.SecondaryRoadType1),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.SecondaryRoadType2),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.SecondaryRoadType3),
                    new RoadSegmentCategoryDbaseRecord(RoadSegmentCategory.SecondaryRoadType4),
                });
        }

        [Fact]
        public void All_hardening_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.HardeningTypes,
                new[]
                {
                    new HardeningTypeDbaseRecord(HardeningType.NotApplicable),
                    new HardeningTypeDbaseRecord(HardeningType.Unknown),
                    new HardeningTypeDbaseRecord(HardeningType.SolidHardening),
                    new HardeningTypeDbaseRecord(HardeningType.LooseHardening),
                });
        }

        [Fact]
        public void All_lane_direction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.LaneDirections,
                new[]
                {
                    new LaneDirectionDbaseRecord(LaneDirection.Unknown),
                    new LaneDirectionDbaseRecord(LaneDirection.Forward),
                    new LaneDirectionDbaseRecord(LaneDirection.Backward),
                    new LaneDirectionDbaseRecord(LaneDirection.Independent),
                });
        }

        [Fact]
        public void All_numbered_road_segement_direction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.NumberedRoadSegmentDirections,
                new[]
                {
                    new NumberedRoadSegmentDirectionDbaseRecord(NumberedRoadSegmentDirection.Unknown),
                    new NumberedRoadSegmentDirectionDbaseRecord(NumberedRoadSegmentDirection.Forward),
                    new NumberedRoadSegmentDirectionDbaseRecord(NumberedRoadSegmentDirection.Backward),
                });
        }

        [Fact]
        public void All_reference_point_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.ReferencePointTypes,
                new[]
                {
                    new ReferencePointTypeDbaseRecord(ReferencePointType.Unknown),
                    new ReferencePointTypeDbaseRecord(ReferencePointType.KilometerMarker),
                    new ReferencePointTypeDbaseRecord(ReferencePointType.HectometerMarker),
                });
        }

        [Fact]
        public void All_grade_separated_junction_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.GradeSeparatedJunctionTypes,
                new[]
                {
                    new GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType.Unknown),
                    new GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType.Tunnel),
                    new GradeSeparatedJunctionTypeDbaseRecord(GradeSeparatedJunctionType.Bridge),
                });
        }

        private void AssertDbaseRecordCollectionsContainSameElements<TDbaseRecord>(TDbaseRecord[] actualRecords, TDbaseRecord[] expectedRecords)
            where TDbaseRecord : DbaseRecord
        {
            Assert.Equal(expectedRecords.Length, actualRecords.Length);
            for (var i = 0; i < expectedRecords.Length; i++)
            {
                Assert.Equal(expectedRecords[i], actualRecords[i], new DbaseRecordComparer<TDbaseRecord>());
            }
        }
    }

    internal class DbaseRecordComparer<TDbaseRecord> : IEqualityComparer<TDbaseRecord>
        where TDbaseRecord :DbaseRecord
    {
        public bool Equals(TDbaseRecord x, TDbaseRecord y)
        {
            if (null == x && null == y)
                return true;

            if (null == x || null == y || x.GetType() != y.GetType())
                return false;

            return x.IsDeleted == y.IsDeleted && Equal(x.Values, y.Values);
        }

        private bool Equal(DbaseFieldValue[] xValues, DbaseFieldValue[] yValues)
        {
            if (null == xValues && null == yValues)
                return true;

            if (xValues?.Length != yValues?.Length)
                return false;

            return xValues
                .Select<DbaseFieldValue, Func<bool>>((_, i) => { return () => Equals(xValues[i], yValues[i]); })
                .All(areEqual => areEqual());
        }

        private static bool Equals(DbaseFieldValue x, DbaseFieldValue y)
        {
            if (null == x && null == y)
                return false;

            if (
                null == x
                || null == y
                || x.GetType() != y.GetType()
                || false == x.Field.Equals(y.Field))
                return false;

            if (x is DbaseDateTime xDateTime && y is DbaseDateTime yDateTime)
                return Equals(xDateTime.Value, yDateTime.Value);

            if (x is DbaseInt32 xInt32 && y is DbaseInt32 yInt32)
                return Equals(xInt32.Value, yInt32.Value);

            if (x is DbaseSingle xSingle && y is DbaseSingle ySingle)
                return Equals(xSingle.Value, ySingle.Value);

            if (x is DbaseDouble xDouble && y is DbaseDouble yDouble)
                return Equals(xDouble.Value, yDouble.Value);

            if (x is DbaseString xString && y is DbaseString yString)
                return Equals(xString.Value, yString.Value);

            throw new NotImplementedException($"No equality impelemented for {x.GetType().FullName}");
        }

        public int GetHashCode(TDbaseRecord obj)
        {
            unchecked
            {
                return
                    typeof(TDbaseRecord).Name.GetHashCode()
                    ^ (obj?.IsDeleted.GetHashCode() ?? 0 * 397)
                    ^ GetValuesHash(obj?.Values);
            }
        }

        private static int GetValuesHash(DbaseFieldValue[] values)
        {
            if (null == values || 0 == values.Length)
                return 0;

            return values
                .Select(GetHashCode)
                .Aggregate((i, j) => { unchecked { return i ^ j; } });
        }

        private static int GetHashCode(DbaseFieldValue fieldValue)
        {
            if (null == fieldValue)
                return 0;

            int CreateFieldHashForValue(object value)
            {
                unchecked
                {
                    return (fieldValue.Field?.GetHashCode() ?? 0) ^ 397 ^ value.GetHashCode();
                }
            }

            if (fieldValue is DbaseDateTime dbaseDateTime)
                return CreateFieldHashForValue(dbaseDateTime.Value);

            if (fieldValue is DbaseInt32 dbaseInt32)
                return CreateFieldHashForValue(dbaseInt32.Value);

            if (fieldValue is DbaseSingle dbaseSingle)
                return CreateFieldHashForValue(dbaseSingle.Value);

            if (fieldValue is DbaseDouble dbaseDouble)
                return CreateFieldHashForValue(dbaseDouble.Value);

            if (fieldValue is DbaseString dbaseString)
                return CreateFieldHashForValue(dbaseString.Value);

            throw new NotImplementedException($"No GetHashCode implementation for {fieldValue.GetType().FullName}");
        }
    }
}
