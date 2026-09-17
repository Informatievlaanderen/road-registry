namespace RoadRegistry.MartenDb.MigrationGenerator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// Schema objects the migrations add by hand next to the ones Marten manages. Marten does not know them, so its patch
// proposes to drop them again; they are taken out of the generated delta here, so a new migration never removes them.
public static partial class HandManagedSchema
{
    private static readonly HashSet<string> Indexes = new(StringComparer.OrdinalIgnoreCase)
    {
        // 20260710000000_add_events_correlation_seq_index
        "eventstore.ix_mt_events_correlation_seq",

        // 20260917140000_add_topology_columns_to_aggregate_documents
        "eventstore.ix_mt_doc_roadsegment_road_segment_id",
        "eventstore.ix_mt_doc_roadsegment_geometry",
        "eventstore.ix_mt_doc_roadsegment_start_node_id",
        "eventstore.ix_mt_doc_roadsegment_end_node_id",
        "eventstore.ix_mt_doc_gradeseparatedjunction_grade_separated_junction_id",
        "eventstore.ix_mt_doc_gradeseparatedjunction_lower_road_segment_id",
        "eventstore.ix_mt_doc_gradeseparatedjunction_upper_road_segment_id",
        "eventstore.ix_mt_doc_gradejunction_grade_junction_id",
        "eventstore.ix_mt_doc_gradejunction_road_segment_id_1",
        "eventstore.ix_mt_doc_gradejunction_road_segment_id_2"
    };

    // 20260917140000_add_topology_columns_to_aggregate_documents: generated from the document JSON.
    private static readonly HashSet<(string Table, string Column)> Columns = new(
    [
        ("eventstore.mt_doc_roadsegment", "road_segment_id"),
        ("eventstore.mt_doc_roadsegment", "geometry"),
        ("eventstore.mt_doc_roadsegment", "start_node_id"),
        ("eventstore.mt_doc_roadsegment", "end_node_id"),
        ("eventstore.mt_doc_roadsegment", "is_removed"),
        ("eventstore.mt_doc_roadsegment", "has_migrated"),
        ("eventstore.mt_doc_gradeseparatedjunction", "grade_separated_junction_id"),
        ("eventstore.mt_doc_gradeseparatedjunction", "lower_road_segment_id"),
        ("eventstore.mt_doc_gradeseparatedjunction", "upper_road_segment_id"),
        ("eventstore.mt_doc_gradeseparatedjunction", "is_removed"),
        ("eventstore.mt_doc_gradeseparatedjunction", "has_migrated"),
        ("eventstore.mt_doc_gradejunction", "grade_junction_id"),
        ("eventstore.mt_doc_gradejunction", "road_segment_id_1"),
        ("eventstore.mt_doc_gradejunction", "road_segment_id_2"),
        ("eventstore.mt_doc_gradejunction", "is_removed")
    ]);

    public static string RemoveDrops(string delta)
    {
        var lines = delta
            .Split('\n')
            .Where(line => !IsDropOfHandManagedObject(line.TrimEnd('\r').Trim()));

        return string.Join('\n', lines);
    }

    private static bool IsDropOfHandManagedObject(string statement)
    {
        var dropIndex = DropIndexStatement().Match(statement);
        if (dropIndex.Success)
        {
            return Indexes.Contains(dropIndex.Groups["index"].Value);
        }

        var dropColumn = DropColumnStatement().Match(statement);
        if (dropColumn.Success)
        {
            return Columns.Contains((dropColumn.Groups["table"].Value.ToLowerInvariant(), dropColumn.Groups["column"].Value.ToLowerInvariant()));
        }

        return false;
    }

    [GeneratedRegex(@"^drop index (if exists )?(?<index>[\w\.]+);$", RegexOptions.IgnoreCase)]
    private static partial Regex DropIndexStatement();

    [GeneratedRegex(@"^alter table (?<table>[\w\.]+) drop column (if exists )?(?<column>\w+)( cascade)?;$", RegexOptions.IgnoreCase)]
    private static partial Regex DropColumnStatement();
}
