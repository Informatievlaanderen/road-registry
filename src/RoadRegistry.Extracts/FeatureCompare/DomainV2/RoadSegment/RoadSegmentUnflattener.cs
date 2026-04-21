namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Valid;
using RoadNode;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadNode.Errors;
using RoadRegistry.ValueObjects.Problems;

public sealed record UnflattenRoadSegmentsResult
{
    public required List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments { get; init; }
    public required List<RoadNodeId> ConsumedRoadNodeIds { get; init; }
    public required ZipArchiveProblems Problems { get; init; }
}

public class RoadSegmentUnflattener
{
    public static UnflattenRoadSegmentsResult Unflatten(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        return new RoadSegmentUnflattener().UnflattenRoadSegments(featureType, records, roadSegmentIdProvider, ogcFeaturesCache, context, cancellationToken);
    }

    private UnflattenRoadSegmentsResult UnflattenRoadSegments(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var problems = ZipArchiveProblems.None;

        // Step 1: Build a graph of road segments and nodes
        var segmentsByNode = BuildSegmentNodeGraph(featureType, records, context, cancellationToken);

        // Step 2: Classify only nodes that are NOT used for validation (Eindknoop, EchteKnoop, Grensknoop)
        var nodeClassifications = ClassifyNonValidationNodes(featureType, segmentsByNode, context, cancellationToken);

        // Step 3: Merge ALL segments connected by nodes that aren't Eindknoop/EchteKnoop/Grensknoop
        var mergedResult = MergeSegmentsAtNonStructuralNodes(featureType, records, roadSegmentIdProvider, segmentsByNode, nodeClassifications, context.Tolerances, ogcFeaturesCache, cancellationToken);
        problems += mergedResult.Problems;

        // Step 4: Detect invalid geometry conditions and insert Validatieknopen where needed
        var unflattenedRecords = DetectAndFixInvalidGeometries(featureType, segmentsByNode, nodeClassifications, mergedResult.RoadSegments, mergedResult.ConsumedRoadNodeIds, roadSegmentIdProvider, context, cancellationToken);
        problems += unflattenedRecords.Problems;

        return new UnflattenRoadSegmentsResult
        {
            RoadSegments = unflattenedRecords.RoadSegments,
            ConsumedRoadNodeIds = mergedResult.ConsumedRoadNodeIds.ToList(),
            Problems = problems
        };
    }

    private Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildSegmentNodeGraph(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> flatRoadSegments,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var segmentsByNode = new Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();
        var nodeRecords = context.GetRoadNodeRecords(featureType).NotRemoved().ToList();

        foreach (var flatRoadSegment in flatRoadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var geometry = flatRoadSegment.Attributes.Geometry;
            var startPoint = geometry.Coordinates.First();
            var endPoint = geometry.Coordinates.Last();

            // Find nodes at start and end points
            var startNode = FindNodeAtPoint(startPoint, nodeRecords, context.Tolerances);
            var endNode = FindNodeAtPoint(endPoint, nodeRecords, context.Tolerances);

            if (startNode is not null)
            {
                var key = (startNode.Id, startNode.Attributes.Geometry);
                segmentsByNode.TryAdd(key, []);
                segmentsByNode[key].Add(flatRoadSegment);
            }

            if (endNode is not null)
            {
                var key = (endNode.Id, endNode.Attributes.Geometry);
                segmentsByNode.TryAdd(key, []);
                segmentsByNode[key].Add(flatRoadSegment);
            }
        }

        return segmentsByNode;
    }

    private static RoadNodeFeatureCompareRecord? FindNodeAtPoint(Coordinate point, IReadOnlyCollection<RoadNodeFeatureCompareRecord> nodeRecords, VerificationContextTolerances tolerances)
    {
        return nodeRecords
            .FirstOrDefault(x => x.Attributes.Geometry.IsReasonablyEqualTo(point, tolerances));
    }

    private Dictionary<RoadNodeId, RoadNodeTypeV2> ClassifyNonValidationNodes(
        FeatureType featureType,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var nodeClassifications = new Dictionary<RoadNodeId, RoadNodeTypeV2>();
        var roadNodeRecords = context.GetRoadNodeRecords(featureType)
            .NotRemoved()
            .ToList();

        foreach (var (node, connectedSegments) in segmentsByNode)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeId = node.Item1;
            var nodeRecord = roadNodeRecords.SingleOrDefault(x => x.Id == nodeId);
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

            // Rule 3: grensknoop - boundary node that should always remain
            if (segmentCount == 2 && nodeRecord.Attributes.Grensknoop == true)
            {
                nodeClassifications[nodeId] = RoadNodeTypeV2.Validatieknoop;
                continue;
            }

            // All other nodes (connected to 2 segments, not grensknoop) will be merged
            // Validation nodes will be detected and inserted AFTER merging
        }

        return nodeClassifications;
    }

    private (List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, HashSet<RoadNodeId> ConsumedRoadNodeIds, ZipArchiveProblems Problems) MergeSegmentsAtNonStructuralNodes(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        VerificationContextTolerances tolerances,
        OgcFeaturesCache ogcFeaturesCache,
        CancellationToken cancellationToken)
    {
        var result = new List<RoadSegmentFeatureWithDynamicAttributes>();
        var processedSegments = new HashSet<RoadSegmentTempId>();
        var consumedNodes = new HashSet<RoadNodeId>();

        var problems = ZipArchiveProblems.None;

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processedSegments.Contains(record.Attributes.TempId))
            {
                continue;
            }

            try
            {
                // Find all segments connected through non-structural nodes (not Eindknoop/EchteKnoop/Grensknoop)
                var segmentChain = BuildSegmentChain(featureType, record, segmentsByNode, nodeClassifications, processedSegments, consumedNodes, tolerances);
                problems += segmentChain.Problems;

                var dynamicRecord = BuildFeatureWithDynamicAttributes(segmentChain.FlatRoadSegments, roadSegmentIdProvider, ogcFeaturesCache, tolerances);
                result.Add(dynamicRecord);
            }
            catch (ZipArchiveValidationException ex)
            {
                problems += ex.Problems;
            }
        }

        return (result, consumedNodes, problems);
    }

    private (List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> FlatRoadSegments, ZipArchiveProblems Problems) BuildSegmentChain(
        FeatureType featureType,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> startSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        HashSet<RoadSegmentTempId> processedSegments,
        HashSet<RoadNodeId> consumedNodes,
        VerificationContextTolerances tolerances)
    {
        var chain = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> { startSegment };
        processedSegments.Add(startSegment.Attributes.TempId);

        // Traverse forward from end point
        var problems = TraverseChain(featureType, startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, consumedNodes, isForward: true, tolerances);

        // Traverse backward from start point
        problems += TraverseChain(featureType, startSegment, segmentsByNode, nodeClassifications, chain, processedSegments, consumedNodes, isForward: false, tolerances);

        return (chain, problems);
    }

    private ZipArchiveProblems TraverseChain(
        FeatureType featureType,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> currentSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> chain,
        HashSet<RoadSegmentTempId> processedSegments,
        HashSet<RoadNodeId> consumedNodes,
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
            return ZipArchiveProblems.None;
        }

        // Stop if this node is a structural node (Eindknoop, EchteKnoop, or Grensknoop/Validatieknoop)
        if (nodeClassifications.TryGetValue(nodeAtPoint.Item1, out _))
        {
            // Stop at structural nodes
            return ZipArchiveProblems.None;
        }

        // Otherwise, continue through this non-structural node

        // Find the other segment connected to this schijnknoop
        var connectedSegments = segmentsByNode[nodeAtPoint];
        var nextSegment = connectedSegments.FirstOrDefault(s =>
            s.Attributes.TempId != currentSegment.Attributes.TempId && !processedSegments.Contains(s.Attributes.TempId));
        if (nextSegment is null)
        {
            return ZipArchiveProblems.None;
        }

        Feature<RoadSegmentFeatureCompareWithFlatAttributes>? previousSegment;
        bool appendAtEnd, appendReverse;
        var lastCoordinateInChain = chain.Last().Attributes.Geometry.Coordinates.Last();
        if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.First(), tolerances))
        {
            previousSegment = chain.LastOrDefault();
            appendAtEnd = true;
            appendReverse = false;
        }
        else if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.Last(), tolerances))
        {
            previousSegment = chain.LastOrDefault();
            appendAtEnd = true;
            appendReverse = true;
        }
        else
        {
            var firstCoordinateInChain = chain.First().Attributes.Geometry.Coordinates.First();
            if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.Last(), tolerances))
            {
                previousSegment = chain.FirstOrDefault();
                appendAtEnd = false;
                appendReverse = false;
            }
            else if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.First(), tolerances))
            {
                previousSegment = chain.FirstOrDefault();
                appendAtEnd = false;
                appendReverse = true;
            }
            else
            {
                return ZipArchiveProblems.None;
            }
        }

        if (previousSegment is not null && (previousSegment.Attributes.Status != nextSegment.Attributes.Status || previousSegment.Attributes.Method != nextSegment.Attributes.Method))
        {
            var recordContext = ExtractFileName.Wegknoop.AtDbaseRecord(featureType);
            return ZipArchiveProblems.Single(recordContext.Error(new RoadNodeIsNotAllowed().WithContext(ProblemContext.For(nodeAtPoint.Item1))));
        }

        if (appendAtEnd)
        {
            chain.Add(appendReverse ? Reverse(nextSegment) : nextSegment);
        }
        else
        {
            chain.Insert(0, appendReverse ? Reverse(nextSegment) : nextSegment);
        }

        processedSegments.Add(nextSegment.Attributes.TempId);
        consumedNodes.Add(nodeAtPoint.Item1);

        // Continue traversing
        return TraverseChain(featureType, nextSegment, segmentsByNode, nodeClassifications, chain, processedSegments, consumedNodes, isForward, tolerances);
    }

    private static Feature<RoadSegmentFeatureCompareWithFlatAttributes> Reverse(Feature<RoadSegmentFeatureCompareWithFlatAttributes> feature)
    {
        return feature with
        {
            Attributes = feature.Attributes with
            {
                Geometry = (MultiLineString)feature.Attributes.Geometry.Reverse(),
                LeftMaintenanceAuthorityId = feature.Attributes.RightMaintenanceAuthorityId,
                RightMaintenanceAuthorityId = feature.Attributes.LeftMaintenanceAuthorityId,
                LeftSideStreetNameId = feature.Attributes.RightSideStreetNameId,
                RightSideStreetNameId = feature.Attributes.LeftSideStreetNameId,
                CarAccessForward = feature.Attributes.CarAccessBackward,
                CarAccessBackward = feature.Attributes.CarAccessForward,
                BikeAccessForward = feature.Attributes.BikeAccessBackward,
                BikeAccessBackward = feature.Attributes.BikeAccessForward
            }
        };
    }

    private RoadSegmentFeatureWithDynamicAttributes BuildFeatureWithDynamicAttributes(
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> segments,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache,
        VerificationContextTolerances tolerances)
    {
        var longestSegment = segments.OrderByDescending(x => x.Attributes.Geometry.Length).First();

        // Merge geometries
        var mergedGeometry = segments.Count > 1
            ? longestSegment.Attributes.Geometry.Factory.CreateMultiLineString([
                longestSegment.Attributes.Geometry.Factory.CreateLineString(MergeSegmentsCoordinates(segments.Select(x => x.Attributes.Geometry.Coordinates), tolerances))
            ])
            : segments.Single().Attributes.Geometry;

        var status = segments.Select(x => x.Attributes.Status).Distinct().Single();
        var method = DetermineMethod(longestSegment.Attributes.Method, status, mergedGeometry, ogcFeaturesCache);
        var dynamicAttributes = RoadSegmentFeatureCompareWithDynamicAttributes.Build(
            longestSegment.Attributes.RoadSegmentId ?? roadSegmentIdProvider.NewId(),
            mergedGeometry,
            method,
            status,
            segments.Select(x => x.Attributes).ToList());

        return new RoadSegmentFeatureWithDynamicAttributes(
            longestSegment.RecordNumber,
            dynamicAttributes,
            segments);
    }

    private static Coordinate[] MergeSegmentsCoordinates(IEnumerable<Coordinate[]> segments, VerificationContextTolerances tolerances)
    {
        var coordinates = new List<Coordinate>();

        foreach (var segment in segments)
        {
            foreach (var coordinate in segment)
            {
                if (coordinates.Count == 0 || !coordinate.IsReasonablyEqualTo(coordinates.Last(), tolerances))
                {
                    coordinates.Add(coordinate);
                }
            }
        }

        return coordinates.ToArray();
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

    private (List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, ZipArchiveProblems Problems) DetectAndFixInvalidGeometries(
        FeatureType featureType,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<RoadSegmentFeatureWithDynamicAttributes> mergedRecords,
        HashSet<RoadNodeId> consumedRoadNodeIds,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var nonClassifiedNodes = context.GetRoadNodeRecords(featureType)
            .NotRemoved()
            .Where(x => !nodeClassifications.ContainsKey(x.Id))
            .ToList();

        var problems = ZipArchiveProblems.None;

        var result = mergedRecords.ToList();
        var segmentsQueue = new Queue<RoadSegmentFeatureWithDynamicAttributes>(mergedRecords);

        while (segmentsQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var segment = segmentsQueue.Dequeue();

            // Condition 1: Self-intersecting segment
            var tryFixSelfIntersectingResult = TryFixSelfIntersecting(segment, consumedRoadNodeIds, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixSelfIntersectingResult))
            {
                continue;
            }

            // Condition 2: Same start and end node
            var tryFixSameStartEndNodeResult = TryFixSameStartEndNode(segment, consumedRoadNodeIds, segmentsByNode, nodeClassifications, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixSameStartEndNodeResult))
            {
                continue;
            }

            // Condition 3: Multiple intersections with other segments
            var tryFixMultipleIntersectionsWithOtherSegmentsResult = TryFixMultipleIntersectionsWithOtherSegments(segment, consumedRoadNodeIds, result, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixMultipleIntersectionsWithOtherSegmentsResult))
            {
                continue;
            }
        }

        return (result.OrderBy(x => x.RecordNumber.ToInt32()).ToList(), problems);

        bool EnqueueSplitResult(RoadSegmentFeatureWithDynamicAttributes segment, IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> roadSegments)
        {
            // When segments are split, add the split segments to the queue to be processed again
            if (roadSegments.Count > 0)
            {
                result.Remove(segment);
                result.AddRange(roadSegments);

                foreach (var splitSegment in roadSegments)
                {
                    segmentsQueue.Enqueue(splitSegment);
                }

                return true;
            }

            return false;
        }
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> TryFixSelfIntersecting(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedRoadNodeIds,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var intersection = GetSelfIntersectingIntersectionGeometry(segment.Attributes.Geometry, context.Tolerances);
        if (intersection is null)
        {
            return [];
        }

        var splitResult = TrySplitAtValidatieknopen(segment, consumedRoadNodeIds, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
        if (splitResult is null)
        {
            throw new InvalidOperationException($"Impossible scenario: at this moment a node should always be found because of earlier validations. TempIds: {string.Join(",", segment.FlatFeatures.Select(x => x.Attributes.TempId.ToInt32()))}, IntersectionLocation: {intersection.Intersection.X.ToRoundedMeasurementString()} {intersection.Intersection.Y.ToRoundedMeasurementString()}");
        }

        return splitResult;
    }

    private IntersectionGeometry? GetSelfIntersectingIntersectionGeometry(
        MultiLineString geometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.SelfIntersects())
        {
            return null;
        }

        var isValidOp = new IsSimpleOp(geometry);
        if (isValidOp.IsSimple())
        {
            throw new InvalidOperationException("IsSimpleOp.IsSimple() returns true for self-intersecting geometry. This should not happen.");
        }

        var geometryWhichMustContainNode = ExtractLoop(geometry.GetSingleLineString(), isValidOp.NonSimpleLocation, tolerances.GeometryTolerance);
        var loopIndexedLine = new LengthIndexedLine(geometryWhichMustContainNode);
        var middlePoint = loopIndexedLine.ExtractPoint(geometryWhichMustContainNode.Length / 2.0);

        return new IntersectionGeometry(isValidOp.NonSimpleLocation, geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
    }

    private static LineString ExtractLoop(LineString geometry, Coordinate intersectionPoint, double tolerance)
    {
        var coords = geometry.Coordinates;
        var factory = geometry.Factory;

        // Find all segments that contain the intersection point
        var segmentIndices = new List<int>();

        for (var i = 0; i < coords.Length - 1; i++)
        {
            var segment = new LineSegment(coords[i], coords[i + 1]);
            var distance = segment.Distance(intersectionPoint);

            if (distance < tolerance)
            {
                segmentIndices.Add(i);
            }
        }

        if (segmentIndices.Count < 2)
        {
            throw new InvalidOperationException($"No loop found. Intersection point: {intersectionPoint.X.ToRoundedMeasurementString()} {intersectionPoint.Y.ToRoundedMeasurementString()}");
        }

        // Build the loop coordinates
        var loopCoords = new List<Coordinate>();

        // Add the intersection point at the start
        loopCoords.Add(intersectionPoint.Copy());

        // Add all coordinates between the two segments
        for (var i = segmentIndices[0] + 1; i <= segmentIndices[1]; i++)
        {
            loopCoords.Add(coords[i].Copy());
        }

        // Close the loop with the intersection point
        loopCoords.Add(intersectionPoint.Copy());

        return factory.CreateLineString(loopCoords.ToArray());
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> TryFixSameStartEndNode(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedRoadNodeIds,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var sameStartEndIntersectionGeometry = DetectSameStartEndNode(segment.Attributes.Geometry, context.Tolerances);
        if (sameStartEndIntersectionGeometry is null)
        {
            return [];
        }

        // ensure the merged segment is starting at the correct node when the start/end node are the same
        var tempIds = segment.FlatFeatures.Select(x => x.Attributes.TempId).ToArray();
        var wantedStartLocation = segmentsByNode
            .Where(x => x.Value.Any(s => tempIds.Contains(s.Attributes.TempId)))
            .Where(x => nodeClassifications.ContainsKey(x.Key.Item1))
            .Select(x => x.Key)
            .FirstOrDefault();
        if (wantedStartLocation.Item2 is null)
        {
            throw new InvalidOperationException($"Merged segment (Id {segment.Attributes.RoadSegmentId}) has the same start and end node, but unable to find the 1 structural node that connects it. TempIds: {string.Join(",", tempIds)}");
        }

        if (!segment.Attributes.Geometry.GetSingleLineString().StartPoint.IsReasonablyEqualTo(wantedStartLocation.Item2, context.Tolerances))
        {
            var splitResult2 = SplitGeometryAtNode(segment, wantedStartLocation.Item2.Coordinate, roadSegmentIdProvider);
            consumedRoadNodeIds.Remove(wantedStartLocation.Item1);
            return splitResult2;
        }

        var splitResult = TrySplitAtValidatieknopen(segment, consumedRoadNodeIds, sameStartEndIntersectionGeometry, nonClassifiedNodes, roadSegmentIdProvider, context);
        if (splitResult is null)
        {
            throw new InvalidOperationException($"Impossible scenario: at this moment a node should always be found because of earlier validations. TempIds: {string.Join(",", segment.FlatFeatures.Select(x => x.Attributes.TempId.ToInt32()))}, IntersectionLocation: {sameStartEndIntersectionGeometry.Intersection.X.ToRoundedMeasurementString()} {sameStartEndIntersectionGeometry.Intersection.Y.ToRoundedMeasurementString()}");
        }

        return splitResult;
    }

    private IntersectionGeometry? DetectSameStartEndNode(MultiLineString geometry, VerificationContextTolerances tolerances)
    {
        var coords = geometry.Coordinates;
        var startPoint = coords.First();
        var endPoint = coords.Last();

        if (startPoint.IsReasonablyEqualTo(endPoint, tolerances))
        {
            var geometryLengthIndexedLine = new LengthIndexedLine(geometry);
            var geometryWhichMustContainNode = (LineString)geometryLengthIndexedLine.ExtractLine(tolerances.GeometryTolerance, geometry.Length - tolerances.GeometryTolerance);
            var middlePoint = geometryLengthIndexedLine.ExtractPoint(geometry.Length / 2.0);

            return new IntersectionGeometry(startPoint, geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
        }

        return null;
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> TryFixMultipleIntersectionsWithOtherSegments(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedRoadNodeIds,
        IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> segments,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var otherSegments = segments.Where(x => x != segment).ToArray();

        foreach (var otherSegment in otherSegments)
        {
            var intersection = GetFirstMultipleIntersectionsIntersectionGeometry(segment.Attributes.Geometry, otherSegment.Attributes.Geometry, context.Tolerances);
            if (intersection is null)
            {
                continue;
            }

            var splitResult = TrySplitAtValidatieknopen(segment, consumedRoadNodeIds, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
            return splitResult ?? [];
        }

        return [];
    }

    private IntersectionGeometry? GetFirstMultipleIntersectionsIntersectionGeometry(
        MultiLineString geometry,
        MultiLineString otherGeometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.Intersects(otherGeometry))
        {
            return null;
        }

        var startEndCoordinates = new[] { geometry.Coordinates.First(), geometry.Coordinates.Last() };
        var intersectionCoordinates = geometry.Intersection(otherGeometry).Coordinates
            .Where(x => startEndCoordinates.All(startEndCoordinate => !startEndCoordinate.IsReasonablyEqualTo(x, tolerances))) // exclude start-end coordinates
            .ToArray();
        if (intersectionCoordinates.Length < 2)
        {
            return null;
        }

        var geometryLengthIndexedLine = new LengthIndexedLine(geometry);
        var intersectionPositions = intersectionCoordinates
            .Select(p => geometryLengthIndexedLine.IndexOf(p))
            .OrderBy(x => x)
            .ToList();

        var startPosition = intersectionPositions[0];
        var endPosition = intersectionPositions[1];

        var geometryWhichMustContainNode = (LineString)geometryLengthIndexedLine.ExtractLine(startPosition + tolerances.GeometryTolerance, endPosition - tolerances.GeometryTolerance);
        var middlePoint = geometryLengthIndexedLine.ExtractPoint((endPosition - startPosition) / 2.0);

        return new IntersectionGeometry(intersectionCoordinates[0], geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint));
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes>? TrySplitAtValidatieknopen(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedRoadNodeIds,
        IntersectionGeometry intersection,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        // Find the nearest knoop to the optimal position
        var nearestNode = FindNearestNodeToPosition(intersection.GeometryWhichMustContainNode, intersection.IdealNodeLocation, nonClassifiedNodes, context.Tolerances);
        if (nearestNode is null)
        {
            return null;
        }

        // Split the geometry at the found knoop
        var splitResult = SplitGeometryAtNode(segment, nearestNode.Attributes.Geometry.Coordinate, roadSegmentIdProvider);
        consumedRoadNodeIds.Remove(nearestNode.Id);

        return splitResult;
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> SplitGeometryAtNode(
        RoadSegmentFeatureWithDynamicAttributes segment,
        Coordinate splitPoint,
        IRoadSegmentIdProvider roadSegmentIdProvider)
    {
        var segmentGeometry = segment.Attributes.Geometry;

        var indexedLine = new LengthIndexedLine(segmentGeometry);
        var splitIndex = indexedLine.IndexOf(splitPoint);

        // Create two new geometries
        var geometry1 = ((LineString)indexedLine.ExtractLine(0, splitIndex)).ToMultiLineString();
        var geometry2 = ((LineString)indexedLine.ExtractLine(splitIndex, segmentGeometry.Length)).ToMultiLineString();

        // Create new segments with split geometries
        var segment1 = BuildRoadSegmentFeatureWithDynamicAttributes(geometry1, null);
        var segment2 = BuildRoadSegmentFeatureWithDynamicAttributes(geometry2, segment1.Attributes.RoadSegmentId);

        return [segment1, segment2];

        RoadSegmentFeatureWithDynamicAttributes BuildRoadSegmentFeatureWithDynamicAttributes(MultiLineString geometry, RoadSegmentId? excludeId)
        {
            var flatFeatures = segment.FlatFeatures
                .Select(x =>
                {
                    var clusterTolerance = 0.001;
                    return (
                        Feature: x,
                        OverlapSelfToOther: x.Attributes.Geometry.CalculateOverlapPercentage(geometry, clusterTolerance),
                        OverlapOtherToSelf: geometry.CalculateOverlapPercentage(x.Attributes.Geometry, clusterTolerance)
                    );
                })
                .Where(x => x.OverlapSelfToOther >= 0.99 || x.OverlapOtherToSelf >= 0.99)
                .Select(x => x.Feature)
                .ToArray();

            var roadSegmentId = flatFeatures
                .Where(x => excludeId is null || x.Attributes.RoadSegmentId != excludeId)
                .OrderByDescending(x => x.Attributes.Geometry.Length)
                .FirstOrDefault()?.Attributes.RoadSegmentId;

            return new RoadSegmentFeatureWithDynamicAttributes(
                flatFeatures.OrderByDescending(x => x.Attributes.Geometry.Length).First().RecordNumber,
                RoadSegmentFeatureCompareWithDynamicAttributes.Build(
                    roadSegmentId ?? roadSegmentIdProvider.NewId(),
                    geometry,
                    segment.Attributes.Method!,
                    segment.Attributes.Status!,
                    flatFeatures.Select(x => x.Attributes).ToList()),
                flatFeatures);
        }
    }

    private RoadNodeFeatureCompareRecord? FindNearestNodeToPosition(
        LineString allowedSegmentGeometry,
        Point point,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        VerificationContextTolerances tolerances)
    {
        return nonClassifiedNodes
            .Where(node => node.Attributes.Geometry.Distance(allowedSegmentGeometry) <= tolerances.GeometryTolerance)
            .OrderBy(node => node.Attributes.Geometry.Distance(point))
            .FirstOrDefault();
    }

    private sealed record IntersectionGeometry(Coordinate Intersection, LineString GeometryWhichMustContainNode, Point IdealNodeLocation);
}
