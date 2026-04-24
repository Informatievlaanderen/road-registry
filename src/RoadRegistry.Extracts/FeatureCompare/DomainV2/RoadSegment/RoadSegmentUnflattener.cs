namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Valid;
using RoadNode;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadNode.Errors;
using RoadRegistry.RoadSegment;
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
        var structuralNodes = ClassifyStructuralNodes(featureType, segmentsByNode, context, cancellationToken);

        // Step 3: Merge ALL segments connected by nodes that aren't Eindknoop/EchteKnoop/Grensknoop
        var mergedResult = MergeSegmentsAtNonStructuralNodes(featureType, records, roadSegmentIdProvider, segmentsByNode, structuralNodes, context.Tolerances, ogcFeaturesCache, cancellationToken);
        problems += mergedResult.Problems;

        // Step 4: Detect invalid geometry conditions and insert Validatieknopen where needed
        var unflattenedRecords = DetectAndFixInvalidGeometries(featureType, segmentsByNode, structuralNodes, mergedResult.RoadSegments, mergedResult.ConsumedNodes, roadSegmentIdProvider, context, cancellationToken);

        return new UnflattenRoadSegmentsResult
        {
            RoadSegments = unflattenedRecords,
            ConsumedRoadNodeIds = mergedResult.ConsumedNodes.ToList(),
            Problems = problems
        };
    }

    private Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildSegmentNodeGraph(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> flatRoadSegments,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var nodeRecords = context.GetRoadNodeRecords(featureType).NotRemoved().ToList();

        // Build spatial index for nodes for faster lookups
        var nodesByLocation = new Dictionary<Coordinate, RoadNodeFeatureCompareRecord>(new CoordinateEqualityComparer(context.Tolerances));
        foreach (var node in nodeRecords)
        {
            nodesByLocation[node.Attributes.Geometry.Coordinate] = node;
        }

        // Process segments in parallel to find their start/end nodes
        var segmentNodePairs = flatRoadSegments
            .AsParallel()
            .WithCancellation(cancellationToken)
            .SelectMany(flatRoadSegment =>
            {
                var geometry = flatRoadSegment.Attributes.Geometry;
                var startPoint = geometry.Coordinates.First();
                var endPoint = geometry.Coordinates.Last();

                var pairs = new List<(RoadNodeId NodeId, Point NodeGeometry, Feature<RoadSegmentFeatureCompareWithFlatAttributes> Segment)>(2);

                // Find nodes at start and end points using spatial index
                if (nodesByLocation.TryGetValue(startPoint, out var startNode))
                {
                    pairs.Add((startNode.Id, startNode.Attributes.Geometry, flatRoadSegment));
                }

                if (nodesByLocation.TryGetValue(endPoint, out var endNode))
                {
                    pairs.Add((endNode.Id, endNode.Attributes.Geometry, flatRoadSegment));
                }

                return pairs;
            })
            .ToList();

        // Group segments by node (sequential, but the heavy lifting was done in parallel)
        var segmentsByNode = new Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();
        foreach (var pair in segmentNodePairs)
        {
            var key = (pair.NodeId, pair.NodeGeometry);
            if (!segmentsByNode.TryGetValue(key, out var list))
            {
                list = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
                segmentsByNode[key] = list;
            }
            list.Add(pair.Segment);
        }

        return segmentsByNode;
    }

    private sealed class CoordinateEqualityComparer : IEqualityComparer<Coordinate>
    {
        private readonly VerificationContextTolerances _tolerances;

        public CoordinateEqualityComparer(VerificationContextTolerances tolerances)
        {
            _tolerances = tolerances;
        }

        public bool Equals(Coordinate? x, Coordinate? y)
        {
            if (x is null || y is null)
            {
                return x == y;
            }

            return x.IsReasonablyEqualTo(y, _tolerances);
        }

        public int GetHashCode(Coordinate obj)
        {
            // Use rounded coordinates for hash to ensure nearby points hash to same bucket
            var tolerance = _tolerances.GeometryTolerance;
            var bucketSize = tolerance * 10; // Larger bucket to handle tolerance range
            var xBucket = (int)(obj.X / bucketSize);
            var yBucket = (int)(obj.Y / bucketSize);
            return HashCode.Combine(xBucket, yBucket);
        }
    }

    private Dictionary<RoadNodeId, RoadNodeTypeV2> ClassifyStructuralNodes(
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

    private (List<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, HashSet<RoadNodeId> ConsumedNodes, ZipArchiveProblems Problems) MergeSegmentsAtNonStructuralNodes(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> structuralNodes,
        VerificationContextTolerances tolerances,
        OgcFeaturesCache ogcFeaturesCache,
        CancellationToken cancellationToken)
    {
        // Build connected component groups that can be processed independently
        var componentGroups = BuildConnectedComponents(records, segmentsByNode, structuralNodes, tolerances);

        // Process each component group in parallel
        var processedComponents = componentGroups
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(componentRecords =>
            {
                var componentResult = new List<RoadSegmentFeatureWithDynamicAttributes>(componentRecords.Count);
                var componentProcessedSegments = new HashSet<RoadSegmentTempId>(componentRecords.Count);
                var componentConsumedNodes = new HashSet<RoadNodeId>();
                var componentProblems = ZipArchiveProblems.None;

                foreach (var record in componentRecords)
                {
                    if (componentProcessedSegments.Contains(record.Attributes.TempId))
                    {
                        continue;
                    }

                    try
                    {
                        var segmentChain = BuildSegmentChain(featureType, record, segmentsByNode, structuralNodes, componentProcessedSegments, componentConsumedNodes, tolerances);
                        componentProblems += segmentChain.Problems;

                        var dynamicRecord = BuildFeatureWithDynamicAttributes(segmentChain.FlatRoadSegments, roadSegmentIdProvider, ogcFeaturesCache, tolerances);
                        componentResult.Add(dynamicRecord);
                    }
                    catch (ZipArchiveValidationException ex)
                    {
                        componentProblems += ex.Problems;
                    }
                }

                return (Result: componentResult, ConsumedNodes: componentConsumedNodes, Problems: componentProblems);
            })
            .ToList();

        // Merge all component results
        var result = new List<RoadSegmentFeatureWithDynamicAttributes>(records.Count);
        var consumedNodes = new HashSet<RoadNodeId>();
        var problems = ZipArchiveProblems.None;

        foreach (var component in processedComponents)
        {
            result.AddRange(component.Result);
            consumedNodes.UnionWith(component.ConsumedNodes);
            problems += component.Problems;
        }

        return (result, consumedNodes, problems);
    }

    private List<List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildConnectedComponents(
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> structuralNodes,
        VerificationContextTolerances tolerances)
    {
        var visited = new HashSet<RoadSegmentTempId>();
        var components = new List<List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();

        foreach (var record in records)
        {
            if (visited.Contains(record.Attributes.TempId))
            {
                continue;
            }

            // Find all segments in this connected component using BFS
            var component = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
            var queue = new Queue<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>();
            queue.Enqueue(record);
            visited.Add(record.Attributes.TempId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                component.Add(current);

                // Find all connected segments through shared nodes
                var geometry = current.Attributes.Geometry;
                var startPoint = geometry.Coordinates.First();
                var endPoint = geometry.Coordinates.Last();

                foreach (var nodeKey in segmentsByNode.Keys)
                {
                    if (!nodeKey.Item2.IsReasonablyEqualTo(startPoint, tolerances) && !nodeKey.Item2.IsReasonablyEqualTo(endPoint, tolerances))
                    {
                        continue;
                    }

                    // If this is a structural node, don't traverse through it to other components
                    if (structuralNodes.ContainsKey(nodeKey.Item1))
                    {
                        continue;
                    }

                    foreach (var connectedSegment in segmentsByNode[nodeKey]
                                 .Where(x => !visited.Contains(x.Attributes.TempId)))
                    {
                        visited.Add(connectedSegment.Attributes.TempId);
                        queue.Enqueue(connectedSegment);
                    }
                }
            }

            components.Add(component);
        }

        return components;
    }

    private (List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> FlatRoadSegments, ZipArchiveProblems Problems) BuildSegmentChain(
        FeatureType featureType,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> startSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> structuralNodes,
        HashSet<RoadSegmentTempId> processedSegments,
        HashSet<RoadNodeId> consumedNodes,
        VerificationContextTolerances tolerances)
    {
        var chain = new List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> { startSegment };
        processedSegments.Add(startSegment.Attributes.TempId);

        // Traverse forward from end point
        var problems = TraverseChain(featureType, startSegment, segmentsByNode, structuralNodes, chain, processedSegments, consumedNodes, isForward: true, tolerances);

        // Traverse backward from start point
        problems += TraverseChain(featureType, startSegment, segmentsByNode, structuralNodes, chain, processedSegments, consumedNodes, isForward: false, tolerances);

        return (chain, problems);
    }

    private ZipArchiveProblems TraverseChain(
        FeatureType featureType,
        Feature<RoadSegmentFeatureCompareWithFlatAttributes> currentSegment,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> structuralNodes,
        List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> chain,
        HashSet<RoadSegmentTempId> processedSegments,
        HashSet<RoadNodeId> consumedNodes,
        bool isForward,
        VerificationContextTolerances tolerances)
    {
        while (true)
        {
            var nextNodeCoordinate = isForward
                ? currentSegment.Attributes.Geometry.Coordinates.Last()
                : currentSegment.Attributes.Geometry.Coordinates.First();

            // Find node at point using faster lookup
            var nodeAtPoint = segmentsByNode
                .Where(kvp => kvp.Value.Any(rs => currentSegment.Attributes.TempId == rs.Attributes.TempId) && kvp.Key.Item2.IsReasonablyEqualTo(nextNodeCoordinate, tolerances))
                .Select(x => x.Key)
                .FirstOrDefault();

            if (nodeAtPoint == default)
            {
                return ZipArchiveProblems.None;
            }

            // Stop if this node is a structural node (Eindknoop, EchteKnoop, or Grensknoop/Validatieknoop)
            if (structuralNodes.ContainsKey(nodeAtPoint.Item1))
            {
                return ZipArchiveProblems.None;
            }

            // Find the other segment connected to this schijnknoop
            var connectedSegments = segmentsByNode[nodeAtPoint];
            Feature<RoadSegmentFeatureCompareWithFlatAttributes>? nextSegment = null;
            foreach (var s in connectedSegments)
            {
                if (s.Attributes.TempId != currentSegment.Attributes.TempId && !processedSegments.Contains(s.Attributes.TempId))
                {
                    nextSegment = s;
                    break;
                }
            }

            if (nextSegment is null)
            {
                return ZipArchiveProblems.None;
            }

            Feature<RoadSegmentFeatureCompareWithFlatAttributes>? previousSegment;
            bool appendAtEnd, appendReverse;
            var lastCoordinateInChain = chain[^1].Attributes.Geometry.Coordinates.Last();
            var nextSegmentCoords = nextSegment.Attributes.Geometry.Coordinates;

            if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegmentCoords.First(), tolerances))
            {
                previousSegment = chain[^1];
                appendAtEnd = true;
                appendReverse = false;
            }
            else if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegmentCoords.Last(), tolerances))
            {
                previousSegment = chain[^1];
                appendAtEnd = true;
                appendReverse = true;
            }
            else
            {
                var firstCoordinateInChain = chain[0].Attributes.Geometry.Coordinates.First();
                if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegmentCoords.Last(), tolerances))
                {
                    previousSegment = chain[0];
                    appendAtEnd = false;
                    appendReverse = false;
                }
                else if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegmentCoords.First(), tolerances))
                {
                    previousSegment = chain[0];
                    appendAtEnd = false;
                    appendReverse = true;
                }
                else
                {
                    return ZipArchiveProblems.None;
                }
            }

            if (previousSegment.Attributes.Status != nextSegment.Attributes.Status)
            {
                var recordContext = ExtractFileName.Wegknoop.AtDbaseRecord(featureType);
                return ZipArchiveProblems.Single(recordContext.Error(new RoadNodeIsNotAllowed().WithContext(ProblemContext.For(nodeAtPoint.Item1))));
            }

            if (appendReverse)
            {
                nextSegment = Reverse(nextSegment);
            }

            if (appendAtEnd)
            {
                chain.Add(nextSegment);
            }
            else
            {
                chain.Insert(0, nextSegment);
            }

            processedSegments.Add(nextSegment.Attributes.TempId);
            consumedNodes.Add(nodeAtPoint.Item1);

            // Continue traversing (tail recursion converted to loop)
            currentSegment = nextSegment;
        }
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
        var methodFromSegments = RoadSegmentGeometryHelper.DetermineMethod(segments.Select(x => (x.Attributes.Geometry, x.Attributes.Method)).ToArray(), mergedGeometry);
        var method = DetermineMethodFromOgcOverlap(methodFromSegments, status, mergedGeometry, ogcFeaturesCache);
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

    private RoadSegmentGeometryDrawMethodV2 DetermineMethodFromOgcOverlap(RoadSegmentGeometryDrawMethodV2? method, RoadSegmentStatusV2 status, MultiLineString geometry, OgcFeaturesCache ogcFeaturesCache)
    {
        if (method is not null)
        {
            return method;
        }

        if (status == RoadSegmentStatusV2.Gepland
            || !ogcFeaturesCache.HasOverlapWithFeatures(geometry, RoadSegmentConstants.MinimumPercentageForIngemeten))
        {
            return RoadSegmentGeometryDrawMethodV2.Ingeschetst;
        }

        return RoadSegmentGeometryDrawMethodV2.Ingemeten;
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> DetectAndFixInvalidGeometries(
        FeatureType featureType,
        Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> segmentsByNode,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<RoadSegmentFeatureWithDynamicAttributes> mergedRecords,
        HashSet<RoadNodeId> consumedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var nonClassifiedNodes = context.GetRoadNodeRecords(featureType)
            .NotRemoved()
            .Where(x => !nodeClassifications.ContainsKey(x.Id))
            .ToList();

        // Phase 1: Fix self-intersecting and same start/end node issues in parallel
        // These don't require checking other segments, so they can be parallelized
        var phase1Results = mergedRecords
            .AsParallel()
            .WithCancellation(cancellationToken)
            .SelectMany(segment =>
            {
                var processedSegments = new List<RoadSegmentFeatureWithDynamicAttributes> { segment };
                var localConsumedNodes = new HashSet<RoadNodeId>();
                var segmentsQueue = new Queue<RoadSegmentFeatureWithDynamicAttributes>();
                segmentsQueue.Enqueue(segment);

                while (segmentsQueue.Count > 0)
                {
                    var currentSegment = segmentsQueue.Dequeue();

                    // Condition 1: Self-intersecting segment
                    var tryFixSelfIntersectingResult = TryFixSelfIntersecting(currentSegment, localConsumedNodes, nonClassifiedNodes, roadSegmentIdProvider, context);
                    if (tryFixSelfIntersectingResult.Count > 0)
                    {
                        processedSegments.Remove(currentSegment);
                        processedSegments.AddRange(tryFixSelfIntersectingResult);
                        foreach (var splitSegment in tryFixSelfIntersectingResult)
                        {
                            segmentsQueue.Enqueue(splitSegment);
                        }
                        continue;
                    }

                    // Condition 2: Same start and end node
                    var tryFixSameStartEndNodeResult = TryFixSameStartEndNode(currentSegment, localConsumedNodes, segmentsByNode, nodeClassifications, nonClassifiedNodes, roadSegmentIdProvider, context);
                    if (tryFixSameStartEndNodeResult.Count > 0)
                    {
                        processedSegments.Remove(currentSegment);
                        processedSegments.AddRange(tryFixSameStartEndNodeResult);
                        foreach (var splitSegment in tryFixSameStartEndNodeResult)
                        {
                            segmentsQueue.Enqueue(splitSegment);
                        }
                        continue;
                    }
                }

                return processedSegments.Select(s => (Segment: s, ConsumedNodes: localConsumedNodes));
            })
            .ToList();

        // Merge consumed nodes from parallel processing
        foreach (var (_, nodes) in phase1Results)
        {
            consumedNodes.UnionWith(nodes);
        }

        var result = phase1Results.Select(x => x.Segment).ToList();

        // Phase 2: Fix multiple intersections with other segments using spatial index
        var spatialIndex = BuildSpatialIndex(result);
        var segmentsQueue = new Queue<RoadSegmentFeatureWithDynamicAttributes>(result);
        result = result.ToList(); // Create a new list to track current state

        while (segmentsQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var segment = segmentsQueue.Dequeue();

            var tryFixMultipleIntersectionsWithOtherSegmentsResult = TryFixMultipleIntersectionsWithOtherSegmentsUsingSpatialIndex(
                segment,
                consumedNodes,
                spatialIndex,
                nonClassifiedNodes,
                roadSegmentIdProvider,
                context);

            if (tryFixMultipleIntersectionsWithOtherSegmentsResult.Count > 0)
            {
                // Remove old segment from spatial index
                RemoveFromSpatialIndex(spatialIndex, segment);
                result.Remove(segment);

                // Add new segments to result and spatial index
                foreach (var splitSegment in tryFixMultipleIntersectionsWithOtherSegmentsResult)
                {
                    result.Add(splitSegment);
                    AddToSpatialIndex(spatialIndex, splitSegment);
                    segmentsQueue.Enqueue(splitSegment);
                }
            }
        }

        return result.OrderBy(x => x.RecordNumber.ToInt32()).ToList();
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> TryFixSelfIntersecting(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedNodes,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var intersection = GetSelfIntersectingIntersectionGeometry(segment.Attributes.Geometry, context.Tolerances);
        if (intersection is null)
        {
            return [];
        }

        var splitResult = TrySplitAtValidatieknopen(segment, consumedNodes, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
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
        HashSet<RoadNodeId> consumedNodes,
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
            consumedNodes.Remove(wantedStartLocation.Item1);
            return splitResult2;
        }

        var splitResult = TrySplitAtValidatieknopen(segment, consumedNodes, sameStartEndIntersectionGeometry, nonClassifiedNodes, roadSegmentIdProvider, context);
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

    private Dictionary<(int GridX, int GridY), List<RoadSegmentFeatureWithDynamicAttributes>> BuildSpatialIndex(
        List<RoadSegmentFeatureWithDynamicAttributes> segments)
    {
        var spatialIndex = new Dictionary<(int, int), List<RoadSegmentFeatureWithDynamicAttributes>>();
        const double gridSize = 100.0; // Adjust based on your coordinate system scale

        foreach (var segment in segments)
        {
            var envelope = segment.Attributes.Geometry.EnvelopeInternal;
            var minGridX = (int)(envelope.MinX / gridSize);
            var maxGridX = (int)(envelope.MaxX / gridSize);
            var minGridY = (int)(envelope.MinY / gridSize);
            var maxGridY = (int)(envelope.MaxY / gridSize);

            for (var gx = minGridX; gx <= maxGridX; gx++)
            {
                for (var gy = minGridY; gy <= maxGridY; gy++)
                {
                    var key = (gx, gy);
                    if (!spatialIndex.TryGetValue(key, out var list))
                    {
                        list = new List<RoadSegmentFeatureWithDynamicAttributes>();
                        spatialIndex[key] = list;
                    }
                    list.Add(segment);
                }
            }
        }

        return spatialIndex;
    }

    private void AddToSpatialIndex(
        Dictionary<(int GridX, int GridY), List<RoadSegmentFeatureWithDynamicAttributes>> spatialIndex,
        RoadSegmentFeatureWithDynamicAttributes segment)
    {
        const double gridSize = 100.0;
        var envelope = segment.Attributes.Geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / gridSize);
        var maxGridX = (int)(envelope.MaxX / gridSize);
        var minGridY = (int)(envelope.MinY / gridSize);
        var maxGridY = (int)(envelope.MaxY / gridSize);

        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (!spatialIndex.TryGetValue(key, out var list))
                {
                    list = new List<RoadSegmentFeatureWithDynamicAttributes>();
                    spatialIndex[key] = list;
                }
                list.Add(segment);
            }
        }
    }

    private void RemoveFromSpatialIndex(
        Dictionary<(int GridX, int GridY), List<RoadSegmentFeatureWithDynamicAttributes>> spatialIndex,
        RoadSegmentFeatureWithDynamicAttributes segment)
    {
        const double gridSize = 100.0;
        var envelope = segment.Attributes.Geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / gridSize);
        var maxGridX = (int)(envelope.MaxX / gridSize);
        var minGridY = (int)(envelope.MinY / gridSize);
        var maxGridY = (int)(envelope.MaxY / gridSize);

        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (spatialIndex.TryGetValue(key, out var list))
                {
                    list.Remove(segment);
                }
            }
        }
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> TryFixMultipleIntersectionsWithOtherSegmentsUsingSpatialIndex(
        RoadSegmentFeatureWithDynamicAttributes segment,
        HashSet<RoadNodeId> consumedNodes,
        Dictionary<(int GridX, int GridY), List<RoadSegmentFeatureWithDynamicAttributes>> spatialIndex,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        // Find candidate segments using spatial index
        const double gridSize = 100.0;
        var envelope = segment.Attributes.Geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / gridSize);
        var maxGridX = (int)(envelope.MaxX / gridSize);
        var minGridY = (int)(envelope.MinY / gridSize);
        var maxGridY = (int)(envelope.MaxY / gridSize);

        var candidateSegments = new HashSet<RoadSegmentFeatureWithDynamicAttributes>();
        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (spatialIndex.TryGetValue(key, out var list))
                {
                    foreach (var candidate in list
                                 .Where(candidate => candidate != segment))
                    {
                        candidateSegments.Add(candidate);
                    }
                }
            }
        }

        // Check only candidate segments for intersections
        foreach (var otherSegment in candidateSegments)
        {
            var intersection = GetFirstMultipleIntersectionsIntersectionGeometry(
                segment.Attributes.Geometry,
                otherSegment.Attributes.Geometry,
                context.Tolerances);

            if (intersection is null)
            {
                continue;
            }

            var splitResult = TrySplitAtValidatieknopen(segment, consumedNodes, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
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
        HashSet<RoadNodeId> consumedNodes,
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
        consumedNodes.Remove(nearestNode.Id);
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
