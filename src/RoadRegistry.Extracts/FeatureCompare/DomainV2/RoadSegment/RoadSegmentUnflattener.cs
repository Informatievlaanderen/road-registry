namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using RoadNode;

public class RoadSegmentUnflattener
{
    public static List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        RoadSegmentId maxUsedRoadSegmentId,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        return new RoadSegmentUnflattener().UnflattenRoadSegments(featureType, records, maxUsedRoadSegmentId, ogcFeaturesCache, context, cancellationToken);
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> UnflattenRoadSegments(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        RoadSegmentId maxUsedRoadSegmentId,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        // Step 1: Build a graph of road segments and nodes
        var segmentsByNode = BuildSegmentNodeGraph(featureType, records, context, cancellationToken);

        // Step 2: Classify nodes according to the rules
        var nodeClassifications = ClassifyNodes(featureType, records, segmentsByNode, context, cancellationToken);

        // Step 3: Merge segments connected by schijnknopen (nodes with no type assigned)
        var unflattenedRecords = MergeSegmentsAtSchijnknopen(records, maxUsedRoadSegmentId, segmentsByNode, nodeClassifications, context.Tolerances, ogcFeaturesCache, cancellationToken);
        return unflattenedRecords;
    }

    private Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildSegmentNodeGraph(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var segmentsByNode = new Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var geometry = record.Attributes.Geometry;
            var startPoint = geometry.Coordinates.First();
            var endPoint = geometry.Coordinates.Last();

            // Find nodes at start and end points
            var startNode = FindNodeAtPoint(featureType, startPoint, context);
            var endNode = FindNodeAtPoint(featureType, endPoint, context);

            if (startNode is not null)
            {
                var key = (startNode.Id, startNode.Attributes.Geometry);
                if (!segmentsByNode.ContainsKey(key))
                {
                    segmentsByNode[key] = [];
                }

                segmentsByNode[key].Add(record);
            }

            if (endNode is not null)
            {
                var key = (endNode.Id, endNode.Attributes.Geometry);
                if (!segmentsByNode.ContainsKey(key))
                {
                    segmentsByNode[key] = [];
                }

                segmentsByNode[key].Add(record);
            }
        }

        return segmentsByNode;
    }

    private RoadNodeFeatureCompareRecord? FindNodeAtPoint(
        FeatureType featuretype,
        Coordinate point,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        return context.GetRoadNodeRecords(featuretype)
            .NotRemoved()
            .FirstOrDefault(x => x.Attributes.Geometry.IsReasonablyEqualTo(point, context.Tolerances));
    }

    private Dictionary<RoadNodeId, RoadNodeTypeV2> ClassifyNodes(FeatureType featureType, IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var nodeClassifications = new Dictionary<RoadNodeId, RoadNodeTypeV2>();
        var roadNodeRecords = context.GetRoadNodeRecords(featureType);

        foreach (var (node, connectedSegments) in segmentsByNode)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeId = node.Item1;
            var nodeRecord = roadNodeRecords.FirstOrDefault(x => x.Id == nodeId);
            if (nodeRecord is null)
            {
                continue;
            }

            var segmentCount = connectedSegments.Count;

            // Rule 1: eindknoop - connected to exactly 1 segment
            if (segmentCount == 1)
            {
                nodeClassifications[nodeId] = RoadNodeTypeV2.Eindknoop;
                continue;
            }

            // Rule 2: echte knoop - connected to more than 2 segments
            if (segmentCount > 2)
            {
                nodeClassifications[nodeId] = RoadNodeTypeV2.EchteKnoop;
                continue;
            }

            // Rule 3: Connected to exactly 2 segments - check validation node conditions
            if (segmentCount == 2)
            {
                // Rule 3.1: grensknoop = 1
                if (nodeRecord.Attributes.Grensknoop == true)
                {
                    nodeClassifications[nodeId] = RoadNodeTypeV2.Validatieknoop;
                    continue;
                }

                // Rule 3.2: Check if node prevents invalid geometry conditions
                if (PreventsInvalidGeometry(records, nodeRecord.Attributes.Geometry, connectedSegments, context))
                {
                    nodeClassifications[nodeId] = RoadNodeTypeV2.Validatieknoop;
                    continue;
                }

                // If none of the validation conditions apply, it's a schijnknoop
                nodeClassifications[nodeId] = RoadNodeTypeV2.Schijnknoop;
            }
        }

        return nodeClassifications;
    }

    private bool PreventsInvalidGeometry(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Point nodeGeometry,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> connectedSegments,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        if (connectedSegments.Count != 2)
        {
            return false;
        }

        var segment1 = connectedSegments[0];
        var segment2 = connectedSegments[1];

        // Rule 3.2.a: Prevents a segment from crossing itself
        if (HasNonTrivialIntersection(segment1.Attributes.Geometry, segment2.Attributes.Geometry))
        {
            return true;
        }

        // Rule 3.2.b: Prevents a segment from having same start and end node
        if (GetOtherCoordinate(segment1, nodeGeometry, context).IsReasonablyEqualTo(GetOtherCoordinate(segment2, nodeGeometry, context), context.Tolerances))
        {
            return true;
        }

        // Rule 3.2.c: Check if these two segments would cross multiple times with the same segment without this node
        if (SegmentsCrossMultipleTimes(records, segment1, segment2))
        {
            return true;
        }

        return false;
    }

    private bool HasNonTrivialIntersection(Geometry geom1, Geometry geom2)
    {
        if (!geom1.Intersects(geom2))
        {
            return false;
        }

        var intersection = geom1.Intersection(geom2);

        // Exclude point-only intersections (touching at endpoints)
        return intersection.Dimension > 0; // 0 = Point, 1 = LineString, 2 = Polygon
    }


    private Coordinate GetOtherCoordinate(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment,
        Point nodeGeometry,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var coords = segment.Attributes.Geometry.Coordinates;
        var startPoint = coords.First();
        var endPoint = coords.Last();

        return startPoint.IsReasonablyEqualTo(nodeGeometry.Coordinate, context.Tolerances)
            ? endPoint
            : startPoint;
    }

    private bool SegmentsCrossMultipleTimes(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment1,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> segment2)
    {
        var geometry1 = segment1.Attributes.Geometry;
        var geometry2 = segment2.Attributes.Geometry;

        var otherIntersectingSegments = records
            .Where(x => x.Attributes.TempId != segment1.Attributes.TempId && x.Attributes.TempId != segment2.Attributes.TempId)
            .Where(x => x.Attributes.Geometry.Intersects(geometry1) && x.Attributes.Geometry.Intersects(geometry2))
            .ToList();

        return otherIntersectingSegments.Any();
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> MergeSegmentsAtSchijnknopen(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        RoadSegmentId maxUsedRoadSegmentId,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        VerificationContextTolerances tolerances,
        OgcFeaturesCache ogcFeaturesCache,
        CancellationToken cancellationToken)
    {
        var result = new List<RoadSegmentFeatureWithDynamicAttributes>();
        var processedSegments = new HashSet<RoadSegmentTempId>();
        var nextRoadSegmentIdProvider = new NextRoadSegmentIdProvider(maxUsedRoadSegmentId);

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processedSegments.Contains(record.Attributes.TempId))
            {
                continue;
            }

            // Find all segments connected through schijnknopen
            var segmentChain = BuildSegmentChain(record, segmentsByNode, nodeClassifications, processedSegments, tolerances);

            var dynamicRecord = BuildFeatureWithDynamicAttributes(segmentChain, nextRoadSegmentIdProvider, ogcFeaturesCache);
            result.Add(dynamicRecord);
        }

        return result;
    }

    private List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> BuildSegmentChain(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> startSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        HashSet<RoadSegmentTempId> processedSegments,
        VerificationContextTolerances tolerances)
    {
        var chain = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> { startSegment };
        processedSegments.Add(startSegment.Attributes.TempId);

        // Traverse forward from end point
        TraverseChain(startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward: true, tolerances);

        // Traverse backward from start point
        TraverseChain(startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward: false, tolerances);

        return chain;
    }

    private void TraverseChain(
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> currentSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> chain,
        HashSet<RoadSegmentTempId> processedSegments,
        bool isForward,
        VerificationContextTolerances tolerances)
    {
        var nextNodeCoordinate = isForward
            ? currentSegment.Attributes.Geometry.Coordinates.Last()
            : currentSegment.Attributes.Geometry.Coordinates.First();

        var nodeAtPoint = segmentsByNode
            .FirstOrDefault(kvp => kvp.Value.Contains(currentSegment) && kvp.Key.Item2.IsReasonablyEqualTo(nextNodeCoordinate, tolerances))
            .Key;

        if (nodeAtPoint == default)
        {
            return;
        }

        // Check if this node is a schijnknoop
        if (!nodeClassifications.TryGetValue(nodeAtPoint.Item1, out var nodeType) || nodeType != RoadNodeTypeV2.Schijnknoop)
        {
            return;
        }

        // Find the other segment connected to this schijnknoop
        var connectedSegments = segmentsByNode[nodeAtPoint];
        var nextSegment = connectedSegments.FirstOrDefault(s =>
            s.Attributes.TempId != currentSegment.Attributes.TempId && !processedSegments.Contains(s.Attributes.TempId));
        if (nextSegment is null)
        {
            return;
        }

        processedSegments.Add(nextSegment.Attributes.TempId);

        if (isForward)
            chain.Add(nextSegment);
        else
            chain.Insert(0, nextSegment);

        // Continue traversing
        TraverseChain(nextSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward, tolerances);
    }

    private RoadSegmentFeatureWithDynamicAttributes BuildFeatureWithDynamicAttributes(
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> segments,
        NextRoadSegmentIdProvider nextRoadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache)
    {
        var firstSegment = segments.First();

        // Merge geometries
        var mergedGeometry = segments.Count > 1
            ? new MultiLineString([new LineString(segments.SelectMany(s => s.Attributes.Geometry.Coordinates).Distinct().ToArray())])
            : segments.Single().Attributes.Geometry;

        var status = segments.Select(x => x.Attributes.Status).Distinct().Single();
        var method = DetermineMethod(firstSegment.Attributes.Method, status, mergedGeometry, ogcFeaturesCache);
        var dynamicAttributes = RoadSegmentFeatureCompareWithDynamicAttributes.Build(
            firstSegment.Attributes.RoadSegmentId ?? nextRoadSegmentIdProvider.Next(),
            mergedGeometry,
            method,
            status,
            segments.Select(x => x.Attributes).ToList());

        return new RoadSegmentFeatureWithDynamicAttributes(
            firstSegment.RecordNumber,
            dynamicAttributes,
            segments);
    }

    private RoadSegmentGeometryDrawMethodV2 DetermineMethod(RoadSegmentGeometryDrawMethodV2? method, RoadSegmentStatusV2 status, MultiLineString geometry, OgcFeaturesCache ogcFeaturesCache)
    {
        if (method is not null)
        {
            return method;
        }

        if (status == RoadSegmentStatusV2.Gepland
            || !ogcFeaturesCache.HasOverlapWithFeatures(geometry, 0.75))
        {
            return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
        }

        return RoadSegmentGeometryDrawMethodV2.Ingemeten;
    }

    private sealed class NextRoadSegmentIdProvider
    {
        private RoadSegmentId _nextValue;

        public NextRoadSegmentIdProvider(RoadSegmentId initialValue)
        {
            _nextValue = initialValue;
        }

        public RoadSegmentId Next()
        {
            var result = _nextValue;
            _nextValue = _nextValue.Next();
            return result;
        }
    }
}
