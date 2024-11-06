namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messages;
using NetTopologySuite.Geometries;

internal class RequestedChangeTranslator
{
    private readonly IRoadNetworkIdProvider _roadNetworkIdProvider;
    private readonly RoadNetworkVersionProvider _roadNetworkVersionProvider;

    public RequestedChangeTranslator(
        IRoadNetworkIdProvider roadNetworkIdProvider,
        Func<RoadNodeId, RoadNodeVersion> nextRoadNodeVersion,
        Func<NextRoadSegmentVersionArgs, CancellationToken, Task<RoadSegmentVersion>> nextRoadSegmentVersion,
        Func<NextRoadSegmentVersionArgs, MultiLineString, CancellationToken, Task<GeometryVersion>> nextRoadSegmentGeometryVersion)
    {
        _roadNetworkVersionProvider = new RoadNetworkVersionProvider(nextRoadNodeVersion, nextRoadSegmentVersion, nextRoadSegmentGeometryVersion);
        _roadNetworkIdProvider = roadNetworkIdProvider.ThrowIfNull();
    }

    public async Task<RequestedChanges> Translate(IReadOnlyCollection<RequestedChange> changes, IOrganizations organizations, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(organizations);

        var translated = RequestedChanges.Start(await _roadNetworkIdProvider.NextTransactionId());
        foreach (var change in changes.Flatten()
                     .Select((change, ordinal) => new SortableChange(change, ordinal))
                     .OrderBy(x => x, new RankChangeBeforeTranslation())
                     .Select(x => x.Change))
        {
            switch (change)
            {
                case Messages.AddRoadNode command:
                    translated = translated.Append(await Translate(command));
                    break;
                case Messages.ModifyRoadNode command:
                    translated = translated.Append(await Translate(command));
                    break;
                case Messages.RemoveRoadNode command:
                    translated = translated.Append(Translate(command));
                    break;
                case Messages.AddRoadSegment command:
                    translated = translated.Append(await Translate(command, translated, organizations, ct));
                    break;
                case Messages.ModifyRoadSegment command:
                    translated = translated.Append(await Translate(command, translated, organizations, ct));
                    break;
                case Messages.ModifyRoadSegmentAttributes command:
                    translated = translated.Append(await Translate(command, organizations, ct));
                    break;
                case Messages.ModifyRoadSegmentGeometry command:
                    translated = translated.Append(await Translate(command, ct));
                    break;
                case Messages.RemoveRoadSegment command:
                    translated = translated.Append(Translate(command));
                    break;
                case Messages.RemoveOutlinedRoadSegment command:
                    translated = translated.Append(Translate(command));
                    break;
                case Messages.RemoveOutlinedRoadSegmentFromRoadNetwork command:
                    translated = translated.Append(Translate(command));
                    break;
                case Messages.AddRoadSegmentToEuropeanRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.RemoveRoadSegmentFromEuropeanRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.AddRoadSegmentToNationalRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.RemoveRoadSegmentFromNationalRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.AddRoadSegmentToNumberedRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.RemoveRoadSegmentFromNumberedRoad command:
                    translated = translated.Append(await Translate(command, translated, ct));
                    break;
                case Messages.AddGradeSeparatedJunction command:
                    translated = translated.Append(await Translate(command, translated));
                    break;
                case Messages.ModifyGradeSeparatedJunction command:
                    translated = translated.Append(Translate(command, translated));
                    break;
                case Messages.RemoveGradeSeparatedJunction command:
                    translated = translated.Append(Translate(command));
                    break;
            }
        }

        return translated;
    }

    private async Task<AddRoadNode> Translate(Messages.AddRoadNode command)
    {
        var permanent = await _roadNetworkIdProvider.NextRoadNodeId();
        var temporaryId = new RoadNodeId(command.TemporaryId);
        var originalId = RoadNodeId.FromValue(command.OriginalId);

        return new AddRoadNode
        (
            permanent,
            temporaryId,
            originalId,
            RoadNodeType.Parse(command.Type),
            GeometryTranslator.Translate(command.Geometry)
        );
    }

    private async Task<ModifyRoadNode> Translate(Messages.ModifyRoadNode command)
    {
        var permanent = new RoadNodeId(command.Id);
        var version = _roadNetworkVersionProvider.NextRoadNodeVersion(permanent);

        return new ModifyRoadNode
        (
            permanent,
            version,
            RoadNodeType.Parse(command.Type),
            GeometryTranslator.Translate(command.Geometry)
        );
    }

    private RemoveRoadNode Translate(Messages.RemoveRoadNode command)
    {
        var permanent = new RoadNodeId(command.Id);
        return new RemoveRoadNode
        (
            permanent
        );
    }

    private async Task<RoadSegmentLaneAttribute[]?> Translate(RequestedRoadSegmentLaneAttribute[]? attributes, RoadSegmentId roadSegmentId)
    {
        if (attributes is null)
        {
            return null;
        }

        var attributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentLaneAttributeIdProvider(roadSegmentId);

        var result = new List<RoadSegmentLaneAttribute>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentLaneAttribute(
                await attributeIdProvider(),
                new AttributeId(item.AttributeId),
                new RoadSegmentLaneCount(item.Count),
                RoadSegmentLaneDirection.Parse(item.Direction),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition),
                new GeometryVersion(0)
            ));
        }

        return result.ToArray();
    }

    private async Task<RoadSegmentWidthAttribute[]> Translate(RequestedRoadSegmentWidthAttribute[] attributes, RoadSegmentId roadSegmentId)
    {
        if (attributes is null)
        {
            return null;
        }

        var attributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentWidthAttributeIdProvider(roadSegmentId);

        var result = new List<RoadSegmentWidthAttribute>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentWidthAttribute(
                await attributeIdProvider(),
                new AttributeId(item.AttributeId),
                new RoadSegmentWidth(item.Width),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition),
                new GeometryVersion(0)
            ));
        }

        return result.ToArray();
    }
    private async Task<RoadSegmentSurfaceAttribute[]> Translate(RequestedRoadSegmentSurfaceAttribute[] attributes, RoadSegmentId roadSegmentId)
    {
        if (attributes is null)
        {
            return null;
        }

        var attributeIdProvider = _roadNetworkIdProvider.NextRoadSegmentSurfaceAttributeIdProvider(roadSegmentId);

        var result = new List<RoadSegmentSurfaceAttribute>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentSurfaceAttribute(
                await attributeIdProvider(),
                new AttributeId(item.AttributeId),
                RoadSegmentSurfaceType.Parse(item.Type),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition),
                new GeometryVersion(0)
            ));
        }

        return result.ToArray();
    }

    private async Task<AddRoadSegment> Translate(Messages.AddRoadSegment command, IRequestedChangeIdentityTranslator translator, IOrganizations organizations, CancellationToken ct)
    {
        var commandPermanentId = RoadSegmentId.FromValue(command.PermanentId);

        var permanent = commandPermanentId ?? await _roadNetworkIdProvider.NextRoadSegmentId();
        var temporaryId = new RoadSegmentId(command.TemporaryId);
        var originalId = RoadSegmentId.FromValue(command.OriginalId);

        var startNodeId = new RoadNodeId(command.StartNodeId);
        RoadNodeId? temporaryStartNodeId;
        if (translator.TryTranslateToPermanent(startNodeId, out var permanentStartNodeId))
        {
            temporaryStartNodeId = startNodeId;
            startNodeId = permanentStartNodeId;
        }
        else
        {
            temporaryStartNodeId = null;
        }

        var endNodeId = new RoadNodeId(command.EndNodeId);
        RoadNodeId? temporaryEndNodeId;
        if (translator.TryTranslateToPermanent(endNodeId, out var permanentEndNodeId))
        {
            temporaryEndNodeId = endNodeId;
            endNodeId = permanentEndNodeId;
        }
        else
        {
            temporaryEndNodeId = null;
        }

        var geometry = GeometryTranslator.Translate(command.Geometry);
        var maintainerId = new OrganizationId(command.MaintenanceAuthority);
        var maintainer = await organizations.FindAsync(maintainerId, ct);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        var morphology = RoadSegmentMorphology.Parse(command.Morphology);
        var status = RoadSegmentStatus.Parse(command.Status);
        var category = RoadSegmentCategory.Parse(command.Category);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(command.AccessRestriction);
        var leftSideStreetNameId = StreetNameLocalId.FromValue(command.LeftSideStreetNameId);
        var rightSideStreetNameId = StreetNameLocalId.FromValue(command.RightSideStreetNameId);

        var laneAttributes = await Translate(command.Lanes, permanent);
        var widthAttributes = await Translate(command.Widths, permanent);
        var surfaceAttributes = await Translate(command.Surfaces, permanent);

        return new AddRoadSegment
        (
            permanent,
            temporaryId,
            originalId,
            commandPermanentId,
            startNodeId,
            temporaryStartNodeId,
            endNodeId,
            temporaryEndNodeId,
            geometry,
            maintainerId,
            maintainer?.Translation.Name,
            geometryDrawMethod,
            morphology,
            status,
            category,
            accessRestriction,
            leftSideStreetNameId,
            rightSideStreetNameId,
            laneAttributes,
            widthAttributes,
            surfaceAttributes
        );
    }

    private async Task<ModifyRoadSegment> Translate(Messages.ModifyRoadSegment command, IRequestedChangeIdentityTranslator translator, IOrganizations organizations, CancellationToken ct)
    {
        var permanent = new RoadSegmentId(command.Id);
        var originalId = RoadSegmentId.FromValue(command.OriginalId);

        var startNodeId = new RoadNodeId(command.StartNodeId);
        RoadNodeId? temporaryStartNodeId;
        if (translator.TryTranslateToPermanent(startNodeId, out var permanentStartNodeId))
        {
            temporaryStartNodeId = startNodeId;
            startNodeId = permanentStartNodeId;
        }
        else
        {
            temporaryStartNodeId = null;
        }

        var endNodeId = new RoadNodeId(command.EndNodeId);
        RoadNodeId? temporaryEndNodeId;
        if (translator.TryTranslateToPermanent(endNodeId, out var permanentEndNodeId))
        {
            temporaryEndNodeId = endNodeId;
            endNodeId = permanentEndNodeId;
        }
        else
        {
            temporaryEndNodeId = null;
        }

        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        var nextRoadSegmentVersionArgs = new NextRoadSegmentVersionArgs(permanent, geometryDrawMethod, command.ConvertedFromOutlined);
        var version = RoadSegmentVersion.FromValue(command.Version)
                      ?? await _roadNetworkVersionProvider.NextRoadSegmentVersion(nextRoadSegmentVersionArgs, ct);
        var geometry = GeometryTranslator.Translate(command.Geometry);
        var geometryVersion = GeometryVersion.FromValue(command.GeometryVersion)
                              ?? await _roadNetworkVersionProvider.NextRoadSegmentGeometryVersion(nextRoadSegmentVersionArgs, geometry, ct);

        var maintainerId = new OrganizationId(command.MaintenanceAuthority);
        var maintainer = await organizations.FindAsync(maintainerId, ct);
        var morphology = RoadSegmentMorphology.Parse(command.Morphology);
        var status = RoadSegmentStatus.Parse(command.Status);
        var category = RoadSegmentCategory.Parse(command.Category);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(command.AccessRestriction);
        var leftSideStreetNameId = StreetNameLocalId.FromValue(command.LeftSideStreetNameId);
        var rightSideStreetNameId = StreetNameLocalId.FromValue(command.RightSideStreetNameId);

        var laneAttributes = await Translate(command.Lanes, permanent);
        var widthAttributes = await Translate(command.Widths, permanent);
        var surfaceAttributes = await Translate(command.Surfaces, permanent);

        return new ModifyRoadSegment
        (
            permanent,
            originalId,
            version,
            startNodeId,
            temporaryStartNodeId,
            endNodeId,
            temporaryEndNodeId,
            geometry,
            geometryVersion,
            maintainerId,
            maintainer?.Translation.Name,
            geometryDrawMethod,
            morphology,
            status,
            category,
            command.CategoryModified,
            accessRestriction,
            leftSideStreetNameId,
            rightSideStreetNameId,
            laneAttributes,
            widthAttributes,
            surfaceAttributes,
            command.ConvertedFromOutlined
        );
    }

    private async Task<ModifyRoadSegmentAttributes> Translate(Messages.ModifyRoadSegmentAttributes command, IOrganizations organizations, CancellationToken ct)
    {
        var permanent = new RoadSegmentId(command.Id);

        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        var version = await _roadNetworkVersionProvider.NextRoadSegmentVersion(new NextRoadSegmentVersionArgs(permanent, geometryDrawMethod, false), ct);

        OrganizationId? maintainerId = command.MaintenanceAuthority is not null
            ? new OrganizationId(command.MaintenanceAuthority)
            : null;
        var maintainer = maintainerId is not null ? await organizations.FindAsync(maintainerId.Value, ct) : null;
        var morphology = command.Morphology is not null
            ? RoadSegmentMorphology.Parse(command.Morphology)
            : null;
        var status = command.Status is not null
            ? RoadSegmentStatus.Parse(command.Status)
            : null;
        var category = command.Category is not null
            ? RoadSegmentCategory.Parse(command.Category)
            : null;
        var accessRestriction = command.AccessRestriction is not null
            ? RoadSegmentAccessRestriction.Parse(command.AccessRestriction)
            : null;
        var leftSide = command.LeftSide is not null
            ? new RoadSegmentSideAttributes(StreetNameLocalId.FromValue(command.LeftSide.StreetNameId))
            : null;
        var rightSide = command.RightSide is not null
            ? new RoadSegmentSideAttributes(StreetNameLocalId.FromValue(command.RightSide.StreetNameId))
            : null;

        var laneAttributes = await Translate(command.Lanes, permanent);
        var widthAttributes = await Translate(command.Widths, permanent);
        var surfaceAttributes = await Translate(command.Surfaces, permanent);

        return new ModifyRoadSegmentAttributes
        (
            permanent,
            version,
            geometryDrawMethod,
            maintainerId,
            maintainer?.Translation.Name,
            morphology,
            status,
            category,
            accessRestriction,
            leftSide,
            rightSide,
            laneAttributes,
            surfaceAttributes,
            widthAttributes
        );
    }

    private async Task<ModifyRoadSegmentGeometry> Translate(Messages.ModifyRoadSegmentGeometry command, CancellationToken ct)
    {
        var permanent = new RoadSegmentId(command.Id);

        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        var nextRoadSegmentVersionArgs = new NextRoadSegmentVersionArgs(permanent, geometryDrawMethod, false);
        var version = await _roadNetworkVersionProvider.NextRoadSegmentVersion(nextRoadSegmentVersionArgs, ct);

        var geometry = GeometryTranslator.Translate(command.Geometry);
        var geometryVersion = await _roadNetworkVersionProvider.NextRoadSegmentGeometryVersion(nextRoadSegmentVersionArgs, geometry, ct);

        var laneAttributes = await Translate(command.Lanes, permanent);
        var widthAttributes = await Translate(command.Widths, permanent);
        var surfaceAttributes = await Translate(command.Surfaces, permanent);

        return new ModifyRoadSegmentGeometry
        (
            permanent,
            version,
            geometryVersion,
            geometryDrawMethod,
            geometry,
            laneAttributes,
            surfaceAttributes,
            widthAttributes
        );
    }

    private RemoveRoadSegment Translate(Messages.RemoveRoadSegment command)
    {
        var permanent = new RoadSegmentId(command.Id);
        return new RemoveRoadSegment
        (
            permanent,
            command.GeometryDrawMethod is not null
                ? RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod)
                : null
        );
    }

    private RemoveOutlinedRoadSegment Translate(Messages.RemoveOutlinedRoadSegment command)
    {
        var permanent = new RoadSegmentId(command.Id);
        return new RemoveOutlinedRoadSegment
        (
            permanent
        );
    }

    private RemoveOutlinedRoadSegmentFromRoadNetwork Translate(Messages.RemoveOutlinedRoadSegmentFromRoadNetwork command)
    {
        var permanent = new RoadSegmentId(command.Id);
        return new RemoveOutlinedRoadSegmentFromRoadNetwork
        (
            permanent
        );
    }

    private async Task<AddRoadSegmentToEuropeanRoad> Translate(Messages.AddRoadSegmentToEuropeanRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = await _roadNetworkIdProvider.NextEuropeanRoadAttributeId();
        var temporary = new AttributeId(command.TemporaryAttributeId);

        var segmentId = new RoadSegmentId(command.SegmentId);
        RoadSegmentId? temporarySegmentId;
        if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
        {
            temporarySegmentId = segmentId;
            segmentId = permanentSegmentId;
        }
        else
        {
            temporarySegmentId = null;
        }

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = EuropeanRoadNumber.Parse(command.Number);
        return new AddRoadSegmentToEuropeanRoad
        (
            permanent,
            temporary,
            segmentGeometryDrawMethod,
            segmentId,
            temporarySegmentId,
            number,
            version
        );
    }

    private async Task<RemoveRoadSegmentFromEuropeanRoad> Translate(Messages.RemoveRoadSegmentFromEuropeanRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = new AttributeId(command.AttributeId);
        var segmentId = new RoadSegmentId(command.SegmentId);

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = EuropeanRoadNumber.Parse(command.Number);
        return new RemoveRoadSegmentFromEuropeanRoad
        (
            permanent,
            segmentGeometryDrawMethod,
            segmentId,
            number,
            version
        );
    }

    private async Task<AddRoadSegmentToNationalRoad> Translate(Messages.AddRoadSegmentToNationalRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = await _roadNetworkIdProvider.NextNationalRoadAttributeId();
        var temporary = new AttributeId(command.TemporaryAttributeId);

        var segmentId = new RoadSegmentId(command.SegmentId);
        RoadSegmentId? temporarySegmentId;
        if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
        {
            temporarySegmentId = segmentId;
            segmentId = permanentSegmentId;
        }
        else
        {
            temporarySegmentId = null;
        }

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = NationalRoadNumber.Parse(command.Number);
        return new AddRoadSegmentToNationalRoad
        (
            permanent,
            temporary,
            segmentGeometryDrawMethod,
            segmentId,
            temporarySegmentId,
            number,
            version
        );
    }

    private async Task<RemoveRoadSegmentFromNationalRoad> Translate(Messages.RemoveRoadSegmentFromNationalRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = new AttributeId(command.AttributeId);
        var segmentId = new RoadSegmentId(command.SegmentId);

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = NationalRoadNumber.Parse(command.Number);

        return new RemoveRoadSegmentFromNationalRoad
        (
            permanent,
            segmentGeometryDrawMethod,
            segmentId,
            number,
            version
        );
    }

    private async Task<AddRoadSegmentToNumberedRoad> Translate(Messages.AddRoadSegmentToNumberedRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = await _roadNetworkIdProvider.NextNumberedRoadAttributeId();
        var temporary = new AttributeId(command.TemporaryAttributeId);

        var segmentId = new RoadSegmentId(command.SegmentId);
        RoadSegmentId? temporarySegmentId;
        if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
        {
            temporarySegmentId = segmentId;
            segmentId = permanentSegmentId;
        }
        else
        {
            temporarySegmentId = null;
        }

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = NumberedRoadNumber.Parse(command.Number);
        var direction = RoadSegmentNumberedRoadDirection.Parse(command.Direction);
        var ordinal = new RoadSegmentNumberedRoadOrdinal(command.Ordinal);

        return new AddRoadSegmentToNumberedRoad
        (
            permanent,
            temporary,
            segmentGeometryDrawMethod,
            segmentId,
            temporarySegmentId,
            number,
            direction,
            ordinal,
            version
        );
    }

    private async Task<RemoveRoadSegmentFromNumberedRoad> Translate(Messages.RemoveRoadSegmentFromNumberedRoad command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    {
        var permanent = new AttributeId(command.AttributeId);
        var segmentId = new RoadSegmentId(command.SegmentId);

        var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
        var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);

        var number = NumberedRoadNumber.Parse(command.Number);

        return new RemoveRoadSegmentFromNumberedRoad
        (
            permanent,
            segmentGeometryDrawMethod,
            segmentId,
            number,
            version
        );
    }

    private async Task<AddGradeSeparatedJunction> Translate(Messages.AddGradeSeparatedJunction command, IRequestedChangeIdentityTranslator translator)
    {
        var permanent = await _roadNetworkIdProvider.NextGradeSeparatedJunctionId();
        var temporary = new GradeSeparatedJunctionId(command.TemporaryId);

        var upperSegmentId = new RoadSegmentId(command.UpperSegmentId);
        RoadSegmentId? temporaryUpperSegmentId;
        if (translator.TryTranslateToPermanent(upperSegmentId, out var permanentUpperSegmentId))
        {
            temporaryUpperSegmentId = upperSegmentId;
            upperSegmentId = permanentUpperSegmentId;
        }
        else
        {
            temporaryUpperSegmentId = null;
        }

        var lowerSegmentId = new RoadSegmentId(command.LowerSegmentId);
        RoadSegmentId? temporaryLowerSegmentId;
        if (translator.TryTranslateToPermanent(lowerSegmentId, out var permanentLowerSegmentId))
        {
            temporaryLowerSegmentId = lowerSegmentId;
            lowerSegmentId = permanentLowerSegmentId;
        }
        else
        {
            temporaryLowerSegmentId = null;
        }

        return new AddGradeSeparatedJunction(
            permanent,
            temporary,
            GradeSeparatedJunctionType.Parse(command.Type),
            upperSegmentId,
            temporaryUpperSegmentId,
            lowerSegmentId,
            temporaryLowerSegmentId);
    }

    private ModifyGradeSeparatedJunction Translate(Messages.ModifyGradeSeparatedJunction command, IRequestedChangeIdentityTranslator translator)
    {
        var permanent = new GradeSeparatedJunctionId(command.Id);

        var upperSegmentId = new RoadSegmentId(command.UpperSegmentId);
        RoadSegmentId? temporaryUpperSegmentId;
        if (translator.TryTranslateToPermanent(upperSegmentId, out var permanentUpperSegmentId))
        {
            temporaryUpperSegmentId = upperSegmentId;
            upperSegmentId = permanentUpperSegmentId;
        }
        else
        {
            temporaryUpperSegmentId = null;
        }

        var lowerSegmentId = new RoadSegmentId(command.LowerSegmentId);
        RoadSegmentId? temporaryLowerSegmentId;
        if (translator.TryTranslateToPermanent(lowerSegmentId, out var permanentLowerSegmentId))
        {
            temporaryLowerSegmentId = lowerSegmentId;
            lowerSegmentId = permanentLowerSegmentId;
        }
        else
        {
            temporaryLowerSegmentId = null;
        }

        return new ModifyGradeSeparatedJunction(
            permanent,
            GradeSeparatedJunctionType.Parse(command.Type),
            upperSegmentId,
            temporaryUpperSegmentId,
            lowerSegmentId,
            temporaryLowerSegmentId);
    }

    private RemoveGradeSeparatedJunction Translate(Messages.RemoveGradeSeparatedJunction command)
    {
        var permanent = new GradeSeparatedJunctionId(command.Id);

        return new RemoveGradeSeparatedJunction(permanent);
    }

    private async Task<RoadSegmentVersion> GetNextRoadSegmentVersionIfNotIncrementedYet(
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentId roadSegmentId,
        IRequestedChangeIdentityTranslator translator,
        CancellationToken cancellationToken)
    {
        if (translator.IsSegmentAdded(roadSegmentId))
        {
            return RoadSegmentVersion.Initial;
        }

        var version = _roadNetworkVersionProvider.GetLastProvidedRoadSegmentVersion(roadSegmentId);

        return version
               ?? await _roadNetworkVersionProvider.NextRoadSegmentVersion(
                   new NextRoadSegmentVersionArgs(roadSegmentId, geometryDrawMethod, false), cancellationToken);
    }

    private sealed class RankChangeBeforeTranslation : IComparer<SortableChange>
    {
        private static readonly Type[] SequenceByTypeOfChange =
        {
            typeof(Messages.AddRoadNode),
            typeof(Messages.AddRoadSegment),
            typeof(Messages.AddRoadSegmentToEuropeanRoad),
            typeof(Messages.AddRoadSegmentToNationalRoad),
            typeof(Messages.AddRoadSegmentToNumberedRoad),
            typeof(Messages.AddGradeSeparatedJunction),
            typeof(Messages.ModifyRoadNode),
            typeof(Messages.ModifyRoadSegment),
            typeof(Messages.ModifyGradeSeparatedJunction),
            typeof(Messages.RemoveRoadSegmentFromEuropeanRoad),
            typeof(Messages.RemoveRoadSegmentFromNationalRoad),
            typeof(Messages.RemoveRoadSegmentFromNumberedRoad),
            typeof(Messages.RemoveGradeSeparatedJunction),
            typeof(Messages.RemoveRoadSegment),
            typeof(Messages.RemoveRoadNode)
        };

        public int Compare(SortableChange left, SortableChange right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var leftRank = Array.IndexOf(SequenceByTypeOfChange, left.Change.GetType());
            var rightRank = Array.IndexOf(SequenceByTypeOfChange, right.Change.GetType());
            var comparison = leftRank.CompareTo(rightRank);
            return comparison != 0
                ? comparison
                : left.Ordinal.CompareTo(right.Ordinal);
        }
    }

    private sealed class SortableChange
    {
        public SortableChange(object change, int ordinal)
        {
            Ordinal = ordinal;
            Change = change;
        }

        public object Change { get; }
        public int Ordinal { get; }
    }

    private sealed class RoadNetworkVersionProvider
    {
        private readonly Func<RoadNodeId, RoadNodeVersion> _nextRoadNodeVersion;
        private readonly Func<NextRoadSegmentVersionArgs, CancellationToken, Task<RoadSegmentVersion>> _nextRoadSegmentVersion;
        private readonly Func<NextRoadSegmentVersionArgs, MultiLineString, CancellationToken, Task<GeometryVersion>> _nextRoadSegmentGeometryVersion;
        private readonly Dictionary<RoadSegmentId, RoadSegmentVersion> _roadSegmentVersions = [];

        public RoadNetworkVersionProvider(
            Func<RoadNodeId, RoadNodeVersion> nextRoadNodeVersion,
            Func<NextRoadSegmentVersionArgs, CancellationToken, Task<RoadSegmentVersion>> nextRoadSegmentVersion,
            Func<NextRoadSegmentVersionArgs, MultiLineString, CancellationToken, Task<GeometryVersion>> nextRoadSegmentGeometryVersion)
        {
            _nextRoadNodeVersion = nextRoadNodeVersion;
            _nextRoadSegmentVersion = nextRoadSegmentVersion;
            _nextRoadSegmentGeometryVersion = nextRoadSegmentGeometryVersion;
        }

        public RoadNodeVersion NextRoadNodeVersion(RoadNodeId roadNodeId)
        {
            return _nextRoadNodeVersion(roadNodeId);
        }

        public async Task<RoadSegmentVersion> NextRoadSegmentVersion(NextRoadSegmentVersionArgs args, CancellationToken cancellationToken)
        {
            var version = await _nextRoadSegmentVersion(args, cancellationToken);
            _roadSegmentVersions[args.Id] = version;
            return version;
        }

        public async Task<GeometryVersion> NextRoadSegmentGeometryVersion(NextRoadSegmentVersionArgs args, MultiLineString geometry, CancellationToken cancellationToken)
        {
            return await _nextRoadSegmentGeometryVersion(args, geometry, cancellationToken);
        }

        public RoadSegmentVersion? GetLastProvidedRoadSegmentVersion(RoadSegmentId roadSegmentId)
        {
            if (_roadSegmentVersions.TryGetValue(roadSegmentId, out var version))
            {
                return version;
            }

            return null;
        }
    }
}
