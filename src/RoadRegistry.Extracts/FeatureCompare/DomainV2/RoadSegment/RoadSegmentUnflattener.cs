namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Valid;
using RoadNode;
using RoadRegistry.Extracts.Schemas.Inwinning.RoadSegments;
using RoadRegistry.Extracts.Uploads;

public class RoadSegmentUnflattener
{
    public static List<RoadSegmentFeatureWithDynamicAttributes> Unflatten(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        return new RoadSegmentUnflattener().UnflattenRoadSegments(featureType, records, roadSegmentIdProvider, ogcFeaturesCache, context, cancellationToken);
    }

    private List<RoadSegmentFeatureWithDynamicAttributes> UnflattenRoadSegments(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        OgcFeaturesCache ogcFeaturesCache,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        // Step 1: Build a graph of road segments and nodes
        var segmentsByNode = BuildSegmentNodeGraph(featureType, records, context, cancellationToken);

        // Step 2: Classify only nodes that are NOT used for validation (Eindknoop, EchteKnoop, Grensknoop)
        var nodeClassifications = ClassifyNonValidationNodes(featureType, segmentsByNode, context, cancellationToken);

        // Step 3: Merge ALL segments connected by nodes that aren't Eindknoop/EchteKnoop/Grensknoop
        var mergedRecords = MergeSegmentsAtNonStructuralNodes(records, roadSegmentIdProvider, segmentsByNode, nodeClassifications, context.Tolerances, ogcFeaturesCache, cancellationToken);

        // Step 4: Detect invalid geometry conditions and insert Validatieknopen where needed
        var unflattenedRecords = DetectAndFixInvalidGeometries(featureType, nodeClassifications, mergedRecords, roadSegmentIdProvider, context, cancellationToken);

        return unflattenedRecords;
    }

    private Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>> BuildSegmentNodeGraph(
        FeatureType featureType,
        IReadOnlyCollection<Feature<RoadSegmentFeatureCompareWithFlatAttributes>> records,
        ZipArchiveEntryFeatureCompareTranslateContext context,
        CancellationToken cancellationToken)
    {
        var segmentsByNode = new Dictionary<(RoadNodeId, Point), List<Feature<RoadSegmentFeatureCompareWithFlatAttributes>>>();
        var nodeRecords = context.GetRoadNodeRecords(featureType).NotRemoved().ToList();

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var geometry = record.Attributes.Geometry;
            var startPoint = geometry.Coordinates.First();
            var endPoint = geometry.Coordinates.Last();

            // Find nodes at start and end points
            var startNode = FindNodeAtPoint(startPoint, nodeRecords, context.Tolerances);
            var endNode = FindNodeAtPoint(endPoint, nodeRecords, context.Tolerances);

            if (startNode is not null)
            {
                var key = (startNode.Id, startNode.Attributes.Geometry);
                segmentsByNode.TryAdd(key, []);
                segmentsByNode[key].Add(record);
            }

            if (endNode is not null)
            {
                var key = (endNode.Id, endNode.Attributes.Geometry);
                segmentsByNode.TryAdd(key, []);
                segmentsByNode[key].Add(record);
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

    private List<RoadSegmentFeatureWithDynamicAttributes> MergeSegmentsAtNonStructuralNodes(
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

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (processedSegments.Contains(record.Attributes.TempId))
            {
                continue;
            }

            // Find all segments connected through non-structural nodes (not Eindknoop/EchteKnoop/Grensknoop)
            var segmentChain = BuildSegmentChain(record, segmentsByNode, nodeClassifications, processedSegments, tolerances);

            var dynamicRecord = BuildFeatureWithDynamicAttributes(segmentChain, roadSegmentIdProvider, ogcFeaturesCache, tolerances);
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

        // Stop if this node is a structural node (Eindknoop, EchteKnoop, or Grensknoop/Validatieknoop)
        if (nodeClassifications.TryGetValue(nodeAtPoint.Item1, out _))
        {
            // Stop at structural nodes
            return;
        }

        // Otherwise, continue through this non-structural node

        // Find the other segment connected to this schijnknoop
        var connectedSegments = segmentsByNode[nodeAtPoint];
        var nextSegment = connectedSegments.FirstOrDefault(s =>
            s.Attributes.TempId != currentSegment.Attributes.TempId && !processedSegments.Contains(s.Attributes.TempId));
        if (nextSegment is null)
        {
            return;
        }

        processedSegments.Add(nextSegment.Attributes.TempId);

        var lastCoordinateInChain = chain.Last().Attributes.Geometry.Coordinates.Last();
        if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.First(), tolerances))
        {
            chain.Add(nextSegment);
        }
        else if (lastCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.Last(), tolerances))
        {
            chain.Add(Reverse(nextSegment));
        }
        else
        {
            var firstCoordinateInChain = chain.First().Attributes.Geometry.Coordinates.First();
            if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.Last(), tolerances))
            {
                chain.Insert(0, nextSegment);
            }
            else if (firstCoordinateInChain.IsReasonablyEqualTo(nextSegment.Attributes.Geometry.Coordinates.First(), tolerances))
            {
                chain.Insert(0, Reverse(nextSegment));
            }
        }

        // Continue traversing
        TraverseChain(nextSegment, segmentsByNode, nodeClassifications, chain, processedSegments, isForward, tolerances);
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
            ? longestSegment.Attributes.Geometry.Factory.CreateMultiLineString([new LineString(MergeSegmentsCoordinates(segments.Select(x => x.Attributes.Geometry.Coordinates), tolerances))])
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

    private List<RoadSegmentFeatureWithDynamicAttributes> DetectAndFixInvalidGeometries(
        FeatureType featureType,
        Dictionary<RoadNodeId, RoadNodeTypeV2> nodeClassifications,
        List<RoadSegmentFeatureWithDynamicAttributes> mergedRecords,
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
        var invalidSegments = new List<RoadSegmentFeatureWithDynamicAttributes>();
        var segmentsQueue = new Queue<RoadSegmentFeatureWithDynamicAttributes>(mergedRecords);

        while (segmentsQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var segment = segmentsQueue.Dequeue();

            // Condition 1: Self-intersecting segment
            var tryFixSelfIntersectingResult = TryFixSelfIntersecting(segment, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixSelfIntersectingResult))
            {
                continue;
            }

            // Condition 2: Same start and end node
            var tryFixSameStartEndNodeResult = TryFixSameStartEndNode(segment, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixSameStartEndNodeResult))
            {
                continue;
            }

            // Condition 3: Multiple intersections with other segments
            var tryFixMultipleIntersectionsWithOtherSegmentsResult = TryFixMultipleIntersectionsWithOtherSegments(segment, result, nonClassifiedNodes, roadSegmentIdProvider, context);
            if (EnqueueSplitResult(segment, tryFixMultipleIntersectionsWithOtherSegmentsResult))
            {
                continue;
            }
        }

        problems.ThrowIfError();

        return result.Except(invalidSegments).OrderBy(x => x.RecordNumber.ToInt32()).ToList();

        bool EnqueueSplitResult(RoadSegmentFeatureWithDynamicAttributes segment, (IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, ZipArchiveProblems Problems) splitResult)
        {
            // When segments are split, add the split segments to the queue to be processed again
            if (splitResult.RoadSegments.Count > 0)
            {
                result.Remove(segment);
                result.AddRange(splitResult.RoadSegments);

                foreach (var splitSegment in splitResult.RoadSegments)
                {
                    segmentsQueue.Enqueue(splitSegment);
                }
                return true;
            }

            if (splitResult.Problems.Count > 0)
            {
                problems += splitResult.Problems;
                invalidSegments.Add(segment);
                return true;
            }

            return false;
        }
    }

    private (IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, ZipArchiveProblems Problems) TryFixSelfIntersecting(
        RoadSegmentFeatureWithDynamicAttributes segment,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var (intersection, problems) = GetSelfIntersectingIntersectionGeometry(segment.Attributes.Geometry, context.Tolerances);
        if (intersection is null)
        {
            return ([], ZipArchiveProblems.None);
        }

        if (problems.Count > 0)
        {
            return ([], problems);
        }

        var splitResult = TrySplitAtValidatieknopen(segment, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
        if (splitResult is null)
        {
            throw new InvalidOperationException($"Impossible scenario: at this moment a node should always be found because of earlier validations. TempIds: {string.Join(",", segment.FlatFeatures.Select(x => x.Attributes.TempId.ToInt32()))}, IntersectionLocation: {intersection.Intersection.X.ToRoundedMeasurementString()}{intersection.Intersection.Y.ToRoundedMeasurementString()}");
        }

        return (splitResult, ZipArchiveProblems.None);
    }

    private (IntersectionGeometry? Intersection, ZipArchiveProblems Problems) GetSelfIntersectingIntersectionGeometry(
        MultiLineString geometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.SelfIntersects())
        {
            return (null, ZipArchiveProblems.None);
        }

        var isValidOp = new IsSimpleOp(geometry);
        if (isValidOp.IsSimple())
        {
            throw new InvalidOperationException("IsSimpleOp.IsSimple() returns true for self-intersecting geometry, but IsValidOp.ValidationError is null. This should not happen.");
        }

        var indexedLine = new LengthIndexedLine(geometry);
        var startPosition = indexedLine.IndexOf(isValidOp.NonSimpleLocation);
        var endPosition = indexedLine.IndexOfAfter(isValidOp.NonSimpleLocation, startPosition);

        var geometryWhichMustContainNode = (LineString)indexedLine.ExtractLine(startPosition + tolerances.GeometryTolerance, endPosition - tolerances.GeometryTolerance);
        var middlePoint = indexedLine.ExtractPoint((endPosition - startPosition) / 2.0);

        return (new IntersectionGeometry(isValidOp.NonSimpleLocation, geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint)), ZipArchiveProblems.None);
    }

    private (IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, ZipArchiveProblems Problems) TryFixSameStartEndNode(
        RoadSegmentFeatureWithDynamicAttributes segment,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var sameStartEndIntersectionGeometry = DetectSameStartEndNode(segment.Attributes.Geometry, context.Tolerances);
        if (sameStartEndIntersectionGeometry is null)
        {
            return ([], ZipArchiveProblems.None);
        }

        var splitResult = TrySplitAtValidatieknopen(segment, sameStartEndIntersectionGeometry, nonClassifiedNodes, roadSegmentIdProvider, context);
        if (splitResult is null)
        {
            throw new InvalidOperationException($"Impossible scenario: at this moment a node should always be found because of earlier validations. TempIds: {string.Join(",", segment.FlatFeatures.Select(x => x.Attributes.TempId.ToInt32()))}, IntersectionLocation: {sameStartEndIntersectionGeometry.Intersection.X.ToRoundedMeasurementString()}{sameStartEndIntersectionGeometry.Intersection.Y.ToRoundedMeasurementString()}");
        }

        return (splitResult, ZipArchiveProblems.None);
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

    private (IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> RoadSegments, ZipArchiveProblems Problems) TryFixMultipleIntersectionsWithOtherSegments(
        RoadSegmentFeatureWithDynamicAttributes segment,
        IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> segments,
        IReadOnlyCollection<RoadNodeFeatureCompareRecord> nonClassifiedNodes,
        IRoadSegmentIdProvider roadSegmentIdProvider,
        ZipArchiveEntryFeatureCompareTranslateContext context)
    {
        var otherSegments = segments.Where(x => x != segment).ToList();

        foreach (var otherSegment in otherSegments)
        {
            var (intersection, problems) = GetFirstMultipleIntersectionsIntersectionGeometry(segment.Attributes.Geometry, otherSegment.Attributes.Geometry, context.Tolerances);
            if (intersection is null)
            {
                continue;
            }

            if (problems.Count > 0)
            {
                return ([], problems);
            }

            var splitResult = TrySplitAtValidatieknopen(segment, intersection, nonClassifiedNodes, roadSegmentIdProvider, context);
            return (splitResult ?? [], ZipArchiveProblems.None);
        }

        return ([], ZipArchiveProblems.None);
    }

    private (IntersectionGeometry? Intersection, ZipArchiveProblems Problems) GetFirstMultipleIntersectionsIntersectionGeometry(
        MultiLineString geometry,
        MultiLineString otherGeometry,
        VerificationContextTolerances tolerances)
    {
        if (!geometry.Intersects(otherGeometry))
        {
            return (null, ZipArchiveProblems.None);
        }

        var startEndCoordinates = new[] { geometry.Coordinates.First(), geometry.Coordinates.Last() };
        var intersectionCoordinates = geometry.Intersection(otherGeometry).Coordinates
            .Where(x => startEndCoordinates.All(startEndCoordinate => !startEndCoordinate.IsReasonablyEqualTo(x, tolerances))) // exclude start-end coordinates
            .ToArray();
        if (intersectionCoordinates.Length < 2)
        {
            return (null, ZipArchiveProblems.None);
        }

        var geometryLengthIndexedLine = new LengthIndexedLine(geometry);
        var intersectionPositions = intersectionCoordinates
            .Select(p => geometryLengthIndexedLine.Project(p))
            .OrderBy(x => x)
            .ToList();

        var startPosition = intersectionPositions[0];
        var endPosition = intersectionPositions[1];

        var geometryWhichMustContainNode = (LineString)geometryLengthIndexedLine.ExtractLine(startPosition + tolerances.GeometryTolerance, endPosition - tolerances.GeometryTolerance);
        var middlePoint = geometryLengthIndexedLine.ExtractPoint((endPosition - startPosition) / 2.0);

        return (new IntersectionGeometry(intersectionCoordinates[0], geometryWhichMustContainNode, geometry.Factory.CreatePoint(middlePoint)), ZipArchiveProblems.None);
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes>? TrySplitAtValidatieknopen(
        RoadSegmentFeatureWithDynamicAttributes segment,
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
        return splitResult;
    }

    private IReadOnlyCollection<RoadSegmentFeatureWithDynamicAttributes> SplitGeometryAtNode(
        RoadSegmentFeatureWithDynamicAttributes segment,
        Coordinate splitPoint,
        IRoadSegmentIdProvider roadSegmentIdProvider)
    {
        var segmentGeometry = segment.Attributes.Geometry;

        var indexedLine = new LengthIndexedLine(segmentGeometry);
        var splitIndex = indexedLine.Project(splitPoint);

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
                .Where(x => x.Attributes.Geometry.CalculateOverlapPercentage(geometry, 0.001) >= 0.99)
                .ToList();

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
