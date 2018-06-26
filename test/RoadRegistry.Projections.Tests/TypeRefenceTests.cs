namespace RoadRegistry.Projections.Tests
{
    using Events;
    using Shaperon;
    using Xunit;

    public class TypeRefenceTests
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
