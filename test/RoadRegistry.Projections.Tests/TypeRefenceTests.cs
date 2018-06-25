namespace RoadRegistry.Projections.Tests
{
    using Events;
    using Shaperon;
    using Xunit;

    public class TypeRefenceTests
    {
        [Fact]
        public void All_road_segment_access_restriction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentAccessRestrictions,
                new[]
                {
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.PublicRoad),
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.PhysicallyImpossible),
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.LegallyForbidden),
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.PrivateRoad),
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.Seasonal),
                    new RoadSegmentAccessRestrictionRecord(RoadSegmentAccessRestriction.Toll),
                });
        }

        [Fact]
        public void All_road_segment_geometry_draw_method_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentGeometryDrawMethods,
                new[]
                {
                    new RoadSegmentGeometryDrawMethodRecord(RoadSegmentGeometryDrawMethod.Outlined),
                    new RoadSegmentGeometryDrawMethodRecord(RoadSegmentGeometryDrawMethod.Measured),
                    new RoadSegmentGeometryDrawMethodRecord(RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications),
                });
        }

        [Fact]
        public void All_road_segment_status_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentStatuses,
                new[]
                {
                    new RoadSegmentStatusRecord(RoadSegmentStatus.Unknown),
                    new RoadSegmentStatusRecord(RoadSegmentStatus.PermitRequested),
                    new RoadSegmentStatusRecord(RoadSegmentStatus.BuildingPermitGranted),
                    new RoadSegmentStatusRecord(RoadSegmentStatus.UnderConstruction),
                    new RoadSegmentStatusRecord(RoadSegmentStatus.InUse),
                    new RoadSegmentStatusRecord(RoadSegmentStatus.OutOfUse),
                });
        }

        [Fact]
        public void All_road_segment_morphology_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                TypeReferences.RoadSegmentMorphologies,
                new[]
                {
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Unknown),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Motorway),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Road_consisting_of_one_roadway),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Traffic_circle),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Special_traffic_situation),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Traffic_square),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Parallel_road),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Frontage_road),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Entry_or_exit_of_a_car_park),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Entry_or_exit_of_a_service),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Pedestrain_zone),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Service_road),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Primitive_road),
                    new RoadSegmentMorphologyRecord(RoadSegmentMorphology.Ferry)
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
                    new GradeSeparatedJunctionTypeRecord(GradeSeparatedJunctionType.Unknown),
                    new GradeSeparatedJunctionTypeRecord(GradeSeparatedJunctionType.Tunnel),
                    new GradeSeparatedJunctionTypeRecord(GradeSeparatedJunctionType.Bridge),
                });
        }

        private void AssertDbaseRecordCollectionsContainSameElements<T>(T[] actualRecords, T[] expectedRecords)
            where T : DbaseRecord
        {
            Assert.Equal(expectedRecords.Length, actualRecords.Length);
            for (var i = 0; i < expectedRecords.Length; i++)
            {
                Assert.Equal(expectedRecords[i], actualRecords[i]);
            }
        }
    }
}
