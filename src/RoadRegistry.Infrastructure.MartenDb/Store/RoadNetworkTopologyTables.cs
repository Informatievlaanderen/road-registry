namespace RoadRegistry.Infrastructure.MartenDb.Store;

using BackOffice;

// The aggregate document tables the road network topology is looked up in. Next to Marten's own columns they carry
// columns generated from the document JSON - the ids, geometry and road nodes a lookup filters and joins on - added by
// the 20260917140000_add_topology_columns_to_aggregate_documents migration. Marten neither writes nor knows about them.
public static class RoadNetworkTopologyTables
{
    // road_segment_id, geometry, start_node_id, end_node_id, is_removed, has_migrated
    public const string RoadSegments = $"{WellKnownSchemas.MartenEventStore}.mt_doc_roadsegment";

    // grade_separated_junction_id, lower_road_segment_id, upper_road_segment_id, is_removed, has_migrated
    public const string GradeSeparatedJunctions = $"{WellKnownSchemas.MartenEventStore}.mt_doc_gradeseparatedjunction";

    // grade_junction_id, road_segment_id_1, road_segment_id_2, is_removed
    public const string GradeJunctions = $"{WellKnownSchemas.MartenEventStore}.mt_doc_gradejunction";
}
