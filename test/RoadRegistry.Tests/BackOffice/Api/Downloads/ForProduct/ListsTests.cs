namespace RoadRegistry.BackOffice.Api.Downloads.ForProduct
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Dbase.Lists;
    using Hosts;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    public class ListsTests
    {
        [Fact]
        public void All_road_node_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadNodeTypeDbaseRecords,
                new[]
                {
                    RecordFrom(RoadNodeType.RealNode),
                    RecordFrom(RoadNodeType.FakeNode),
                    RecordFrom(RoadNodeType.EndNode),
                    RecordFrom(RoadNodeType.MiniRoundabout),
                    RecordFrom(RoadNodeType.TurningLoopNode)
                });
        }

        private static RoadNodeTypeDbaseRecord RecordFrom(RoadNodeType item)
        {
            return new RoadNodeTypeDbaseRecord
            {
                TYPE = {Value = item.Translation.Identifier},
                LBLTYPE = {Value = item.Translation.Name},
                DEFTYPE = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_road_segment_access_restriction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadSegmentAccessRestrictionDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentAccessRestriction.PublicRoad),
                    RecordFrom(RoadSegmentAccessRestriction.PhysicallyImpossible),
                    RecordFrom(RoadSegmentAccessRestriction.LegallyForbidden),
                    RecordFrom(RoadSegmentAccessRestriction.PrivateRoad),
                    RecordFrom(RoadSegmentAccessRestriction.Seasonal),
                    RecordFrom(RoadSegmentAccessRestriction.Toll)
                });
        }

        private static RoadSegmentAccessRestrictionDbaseRecord RecordFrom(RoadSegmentAccessRestriction item)
        {
            return new RoadSegmentAccessRestrictionDbaseRecord
            {
                TYPE = {Value = item.Translation.Identifier},
                LBLTYPE = {Value = item.Translation.Name},
                DEFTYPE = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_road_segment_geometry_draw_method_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentGeometryDrawMethod.Outlined),
                    RecordFrom(RoadSegmentGeometryDrawMethod.Measured),
                    RecordFrom(RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications)
                });
        }

        private static RoadSegmentGeometryDrawMethodDbaseRecord RecordFrom(RoadSegmentGeometryDrawMethod item)
        {
            return new RoadSegmentGeometryDrawMethodDbaseRecord
            {
                METHODE = { Value = item.Translation.Identifier },
                LBLMETHOD = { Value = item.Translation.Name },
                DEFMETHOD = { Value = item.Translation.Description }
            };
        }

        [Fact]
        public void All_road_segment_status_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadSegmentStatusDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentStatus.Unknown),
                    RecordFrom(RoadSegmentStatus.PermitRequested),
                    RecordFrom(RoadSegmentStatus.BuildingPermitGranted),
                    RecordFrom(RoadSegmentStatus.UnderConstruction),
                    RecordFrom(RoadSegmentStatus.InUse),
                    RecordFrom(RoadSegmentStatus.OutOfUse)
                });
        }


        private static RoadSegmentStatusDbaseRecord RecordFrom(RoadSegmentStatus item)
        {
            return new RoadSegmentStatusDbaseRecord
            {
                STATUS = { Value = item.Translation.Identifier},
                LBLSTATUS = {Value = item.Translation.Name},
                DEFSTATUS = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_road_segment_morphology_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadSegmentMorphologyDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentMorphology.Unknown),
                    RecordFrom(RoadSegmentMorphology.Motorway),
                    RecordFrom(RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway),
                    RecordFrom(RoadSegmentMorphology.Road_consisting_of_one_roadway),
                    RecordFrom(RoadSegmentMorphology.TrafficCircle),
                    RecordFrom(RoadSegmentMorphology.SpecialTrafficSituation),
                    RecordFrom(RoadSegmentMorphology.TrafficSquare),
                    RecordFrom(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction),
                    RecordFrom(RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction),
                    RecordFrom(RoadSegmentMorphology.ParallelRoad),
                    RecordFrom(RoadSegmentMorphology.FrontageRoad),
                    RecordFrom(RoadSegmentMorphology.Entry_or_exit_of_a_car_park),
                    RecordFrom(RoadSegmentMorphology.Entry_or_exit_of_a_service),
                    RecordFrom(RoadSegmentMorphology.PedestrainZone),
                    RecordFrom(RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles),
                    RecordFrom(RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles),
                    RecordFrom(RoadSegmentMorphology.ServiceRoad),
                    RecordFrom(RoadSegmentMorphology.PrimitiveRoad),
                    RecordFrom(RoadSegmentMorphology.Ferry)
                });
        }

        private static RoadSegmentMorphologyDbaseRecord RecordFrom(RoadSegmentMorphology item)
        {
            return new RoadSegmentMorphologyDbaseRecord
            {
                MORF = { Value = item.Translation.Identifier},
                LBLMORF = {Value = item.Translation.Name},
                DEFMORF = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_road_segment_category_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllRoadSegmentCategoryDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentCategory.Unknown),
                    RecordFrom(RoadSegmentCategory.NotApplicable),
                    RecordFrom(RoadSegmentCategory.MainRoad),
                    RecordFrom(RoadSegmentCategory.LocalRoad),
                    RecordFrom(RoadSegmentCategory.LocalRoadType1),
                    RecordFrom(RoadSegmentCategory.LocalRoadType2),
                    RecordFrom(RoadSegmentCategory.LocalRoadType3),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadI),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadII),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadIIType1),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadIIType2),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadIIType3),
                    RecordFrom(RoadSegmentCategory.PrimaryRoadIIType4),
                    RecordFrom(RoadSegmentCategory.SecondaryRoad),
                    RecordFrom(RoadSegmentCategory.SecondaryRoadType1),
                    RecordFrom(RoadSegmentCategory.SecondaryRoadType2),
                    RecordFrom(RoadSegmentCategory.SecondaryRoadType3),
                    RecordFrom(RoadSegmentCategory.SecondaryRoadType4)
                });
        }

        private static RoadSegmentCategoryDbaseRecord RecordFrom(RoadSegmentCategory item)
        {
            return new RoadSegmentCategoryDbaseRecord
            {
                WEGCAT = { Value = item.Translation.Identifier },
                LBLWEGCAT = {Value = item.Translation.Name},
                DEFWEGCAT = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_surface_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllSurfaceTypeDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentSurfaceType.NotApplicable),
                    RecordFrom(RoadSegmentSurfaceType.Unknown),
                    RecordFrom(RoadSegmentSurfaceType.SolidSurface),
                    RecordFrom(RoadSegmentSurfaceType.LooseSurface)
                });
        }

        private static SurfaceTypeDbaseRecord RecordFrom(RoadSegmentSurfaceType item)
        {
            return new SurfaceTypeDbaseRecord
            {
                TYPE = {Value = item.Translation.Identifier},
                LBLTYPE = {Value = item.Translation.Name},
                DEFTYPE = {Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_lane_direction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllLaneDirectionDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentLaneDirection.Unknown),
                    RecordFrom(RoadSegmentLaneDirection.Forward),
                    RecordFrom(RoadSegmentLaneDirection.Backward),
                    RecordFrom(RoadSegmentLaneDirection.Independent)
                });
        }

        private static LaneDirectionDbaseRecord RecordFrom(RoadSegmentLaneDirection item)
        {
            return new LaneDirectionDbaseRecord
            {
                RICHTING = { Value = item.Translation.Identifier},
                LBLRICHT= { Value = item.Translation.Name},
                DEFRICHT= { Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_numbered_road_segement_direction_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllNumberedRoadSegmentDirectionDbaseRecords,
                new[]
                {
                    RecordFrom(RoadSegmentNumberedRoadDirection.Unknown),
                    RecordFrom(RoadSegmentNumberedRoadDirection.Forward),
                    RecordFrom(RoadSegmentNumberedRoadDirection.Backward)
                });
        }

        private static NumberedRoadSegmentDirectionDbaseRecord RecordFrom(RoadSegmentNumberedRoadDirection item)
        {
            return new NumberedRoadSegmentDirectionDbaseRecord
            {
                RICHTING = { Value = item.Translation.Identifier},
                LBLRICHT= { Value = item.Translation.Name},
                DEFRICHT= { Value = item.Translation.Description}
            };
        }

        [Fact]
        public void All_grade_separated_junction_type_records_are_defined()
        {
            AssertDbaseRecordCollectionsContainSameElements(
                Lists.AllGradeSeparatedJunctionTypeDbaseRecords,
                new[]
                {
                    RecordFrom(GradeSeparatedJunctionType.Unknown),
                    RecordFrom(GradeSeparatedJunctionType.Tunnel),
                    RecordFrom(GradeSeparatedJunctionType.Bridge)
                });
        }

        private static GradeSeparatedJunctionTypeDbaseRecord RecordFrom(GradeSeparatedJunctionType item)
        {
            return new GradeSeparatedJunctionTypeDbaseRecord
            {
                TYPE = {Value = item.Translation.Identifier},
                LBLTYPE = {Value = item.Translation.Name},
                DEFTYPE = {Value = item.Translation.Description}
            };
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
}
