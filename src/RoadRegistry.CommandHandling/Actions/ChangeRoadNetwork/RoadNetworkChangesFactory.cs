namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using RoadNetwork;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;
using RoadSegment.ValueObjects;
using GeometryTranslator = BackOffice.GeometryTranslator;

public class RoadNetworkChangesFactory
{
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public RoadNetworkChangesFactory(IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    public async Task<RoadNetworkChanges> Build(ChangeRoadNetworkCommand roadNetworkCommand)
    {
        ArgumentNullException.ThrowIfNull(roadNetworkCommand);

        //TODO-pr is transactionid nog nodig?
        var translated = RoadNetworkChanges.Start(await _roadNetworkIdGenerator.NewTransactionIdAsync());

        foreach (var change in roadNetworkCommand.Changes.Flatten()
                     .Select((change, ordinal) => new SortableChange(change, ordinal))
                     .OrderBy(x => x, new RankChangeBeforeTranslation())
                     .Select(x => x.Change))
        {
            switch (change)
            {
                // case AddRoadNode command:
                //     translated = translated.Append(await Translate(command));
                //     break;
                // case ModifyRoadNode command:
                //     translated = translated.Append(await Translate(command));
                //     break;
                // case RemoveRoadNode command:
                //     translated = translated.Append(Translate(command));
                //     break;
                case AddRoadSegmentCommand command:
                    translated.Add(Translate(command));
                    break;
                case ModifyRoadSegmentCommand command:
                    translated.Add(Translate(command));
                    break;
                case RemoveRoadSegmentCommand command:
                    translated.Add(Translate(command));
                    break;
                // case RemoveRoadSegments command:
                //     translated = translated.Append(Translate(command, organizations));
                //     break;
                // case RemoveOutlinedRoadSegment command:
                //     translated = translated.Append(Translate(command));
                //     break;
                // case RemoveOutlinedRoadSegmentFromRoadNetwork command:
                //     translated = translated.Append(Translate(command));
                //     break;
                // case AddRoadSegmentToEuropeanRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromEuropeanRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case AddRoadSegmentToNationalRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromNationalRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case AddRoadSegmentToNumberedRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case RemoveRoadSegmentFromNumberedRoad command:
                //     translated = translated.Append(await Translate(command, translated, ct));
                //     break;
                // case AddGradeSeparatedJunction command:
                //     translated = translated.Append(await Translate(command, translated));
                //     break;
                // case ModifyGradeSeparatedJunction command:
                //     translated = translated.Append(Translate(command, translated));
                //     break;
                // case RemoveGradeSeparatedJunction command:
                //     translated = translated.Append(Translate(command));
                //     break;
            }
        }

        return translated;
    }

    // private async Task<AddRoadNodeChange> Translate(AddRoadNode command)
    // {
    //     var permanent = await _roadNetworkIdProvider.NextRoadNodeId();
    //     var temporaryId = new RoadNodeId(command.TemporaryId);
    //     var originalId = RoadNodeId.FromValue(command.OriginalId);
    //
    //     return new AddRoadNode
    //     (
    //         permanent,
    //         temporaryId,
    //         originalId,
    //         RoadNodeType.Parse(command.Type),
    //         GeometryTranslator.Translate(command.Geometry)
    //     );
    // }
    //
    // private Task<ModifyRoadNodeChange> Translate(ModifyRoadNode command)
    // {
    //     var permanent = new RoadNodeId(command.Id);
    //     var version = _roadNetworkVersionProvider.NextRoadNodeVersion(permanent);
    //
    //     return Task.FromResult(new ModifyRoadNode
    //     (
    //         permanent,
    //         version,
    //         command.Type is not null ? RoadNodeType.Parse(command.Type) : null,
    //         command.Geometry is not null ? GeometryTranslator.Translate(command.Geometry) : null
    //     ));
    // }
    //
    // private RemoveRoadNodeChange Translate(RemoveRoadNode command)
    // {
    //     var permanent = new RoadNodeId(command.Id);
    //     return new RemoveRoadNode
    //     (
    //         permanent
    //     );
    // }

    private AddRoadSegmentChange Translate(AddRoadSegmentCommand command)
    {
        var commandPermanentId = RoadSegmentId.FromValue(command.PermanentId); //TODO-pr achteraf bekijken of dit nog wel nodig is

        //var permanent = commandPermanentId ?? await _roadNetworkIdGenerator.NewRoadSegmentId();
        var temporaryId = new RoadSegmentId(command.TemporaryId);
        var originalId = RoadSegmentId.FromValue(command.OriginalId);

        var startNodeId = new RoadNodeId(command.StartNodeId);
        // RoadNodeId? temporaryStartNodeId;
        // if (translator.TryTranslateToPermanent(startNodeId, out var permanentStartNodeId))
        // {
        //     temporaryStartNodeId = startNodeId;
        //     startNodeId = permanentStartNodeId;
        // }
        // else
        // {
        //     temporaryStartNodeId = null;
        // }

        var endNodeId = new RoadNodeId(command.EndNodeId);
        // RoadNodeId? temporaryEndNodeId;
        // if (translator.TryTranslateToPermanent(endNodeId, out var permanentEndNodeId))
        // {
        //     temporaryEndNodeId = endNodeId;
        //     endNodeId = permanentEndNodeId;
        // }
        // else
        // {
        //     temporaryEndNodeId = null;
        // }

        var geometry = GeometryTranslator.Translate(command.Geometry);
        var maintainerId = new OrganizationId(command.MaintenanceAuthority);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        var morphology = RoadSegmentMorphology.Parse(command.Morphology);
        var status = RoadSegmentStatus.Parse(command.Status);
        var category = RoadSegmentCategory.Parse(command.Category);
        var accessRestriction = RoadSegmentAccessRestriction.Parse(command.AccessRestriction);
        var leftSideStreetNameId = StreetNameLocalId.FromValue(command.LeftSideStreetNameId);
        var rightSideStreetNameId = StreetNameLocalId.FromValue(command.RightSideStreetNameId);

        var laneAttributes = Translate(command.Lanes);
        var widthAttributes = Translate(command.Widths);
        var surfaceAttributes = Translate(command.Surfaces);

        return new AddRoadSegmentChange
        {
            //Id = permanent,
            TemporaryId = temporaryId,
            OriginalId = originalId,
            PermanentId = commandPermanentId,
            StartNodeId = startNodeId,
            //TemporaryStartNodeId = temporaryStartNodeId,
            EndNodeId = endNodeId,
            //TemporaryEndNodeId = temporaryEndNodeId,
            Geometry = geometry,
            MaintenanceAuthorityId = maintainerId,
            GeometryDrawMethod = geometryDrawMethod,
            Morphology = morphology,
            Status = status,
            Category = category,
            AccessRestriction = accessRestriction,
            LeftSideStreetNameId = leftSideStreetNameId,
            RightSideStreetNameId = rightSideStreetNameId,
            Lanes = laneAttributes!,
            Widths = widthAttributes!,
            Surfaces = surfaceAttributes!
        };
    }

    private ModifyRoadSegmentChange Translate(ModifyRoadSegmentCommand command)
    {
        var permanent = new RoadSegmentId(command.Id);
        var originalId = RoadSegmentId.FromValue(command.OriginalId);

        var startNodeId = RoadNodeId.FromValue(command.StartNodeId);
        // RoadNodeId? temporaryStartNodeId;
        // if (startNodeId is not null && translator.TryTranslateToPermanent(startNodeId.Value, out var permanentStartNodeId))
        // {
        //     temporaryStartNodeId = startNodeId;
        //     startNodeId = permanentStartNodeId;
        // }
        // else
        // {
        //     temporaryStartNodeId = null;
        // }

        var endNodeId = RoadNodeId.FromValue(command.EndNodeId);
        // RoadNodeId? temporaryEndNodeId;
        // if (endNodeId is not null && translator.TryTranslateToPermanent(endNodeId.Value, out var permanentEndNodeId))
        // {
        //     temporaryEndNodeId = endNodeId;
        //     endNodeId = permanentEndNodeId;
        // }
        // else
        // {
        //     temporaryEndNodeId = null;
        // }

        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
        // var nextRoadSegmentVersionArgs = new NextRoadSegmentVersionArgs(permanent, geometryDrawMethod, command.ConvertedFromOutlined);
        // var version = RoadSegmentVersion.FromValue(command.Version)
        //               ?? await _roadNetworkVersionProvider.NextRoadSegmentVersion(nextRoadSegmentVersionArgs, ct);
        var geometry = command.Geometry is not null ? GeometryTranslator.Translate(command.Geometry) : null;
        // GeometryVersion? geometryVersion = geometry is not null
        //     ? GeometryVersion.FromValue(command.GeometryVersion) ?? await _roadNetworkVersionProvider.NextRoadSegmentGeometryVersion(nextRoadSegmentVersionArgs, geometry, ct)
        //     : null;

        var maintainerId = OrganizationId.FromValue(command.MaintenanceAuthority);
        var morphology = command.Morphology is not null ? RoadSegmentMorphology.Parse(command.Morphology) : null;
        var status = command.Status is not null ? RoadSegmentStatus.Parse(command.Status) : null;
        var category = command.Category is not null ? RoadSegmentCategory.Parse(command.Category) : null;
        var accessRestriction = command.AccessRestriction is not null ? RoadSegmentAccessRestriction.Parse(command.AccessRestriction) : null;
        var leftSideStreetNameId = StreetNameLocalId.FromValue(command.LeftSideStreetNameId);
        var rightSideStreetNameId = StreetNameLocalId.FromValue(command.RightSideStreetNameId);

        var laneAttributes = Translate(command.Lanes);
        var widthAttributes = Translate(command.Widths);
        var surfaceAttributes = Translate(command.Surfaces);

        return new ModifyRoadSegmentChange
        {
            Id = permanent,
            OriginalId = originalId,
            //version,
            StartNodeId = startNodeId,
            //TemporaryStartNodeId = temporaryStartNodeId,
            EndNodeId = endNodeId,
            //TemporaryEndNodeId = temporaryEndNodeId,
            Geometry = geometry,
            //geometryVersion,
            MaintenanceAuthorityId = maintainerId,
            GeometryDrawMethod = geometryDrawMethod,
            Morphology = morphology,
            Status = status,
            Category = category,
            AccessRestriction = accessRestriction,
            LeftSideStreetNameId = leftSideStreetNameId,
            RightSideStreetNameId = rightSideStreetNameId,
            Lanes = laneAttributes,
            Widths = widthAttributes,
            Surfaces = surfaceAttributes,
            ConvertedFromOutlined = command.ConvertedFromOutlined,
            CategoryModified = command.CategoryModified
        };
    }

    private RemoveRoadSegmentChange Translate(RemoveRoadSegmentCommand command)
    {
        var permanent = new RoadSegmentId(command.Id);

        return new RemoveRoadSegmentChange
        {
            Id = permanent
        };
    }
    //
    // private RemoveRoadSegmentsChange Translate(RemoveRoadSegmentsCommand command, IOrganizations organizations)
    // {
    //     return new RemoveRoadSegments
    //     (
    //         command.Ids.Select(x => new RoadSegmentId(x)).ToArray(),
    //         RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod),
    //         _roadNetworkVersionProvider,
    //         _roadNetworkIdProvider,
    //         organizations
    //     );
    // }
    //
    // private RemoveOutlinedRoadSegmentChange Translate(RemoveOutlinedRoadSegmentCommand command)
    // {
    //     var permanent = new RoadSegmentId(command.Id);
    //     return new RemoveOutlinedRoadSegment
    //     (
    //         permanent
    //     );
    // }
    //
    // private RemoveOutlinedRoadSegmentFromRoadNetworkChange Translate(RemoveOutlinedRoadSegmentFromRoadNetworkCommand command)
    // {
    //     var permanent = new RoadSegmentId(command.Id);
    //     return new RemoveOutlinedRoadSegmentFromRoadNetwork
    //     (
    //         permanent
    //     );
    // }
    //
    // private async Task<AddRoadSegmentToEuropeanRoadChange> Translate(AddRoadSegmentToEuropeanRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = await _roadNetworkIdProvider.NextEuropeanRoadAttributeId();
    //     var temporary = new AttributeId(command.TemporaryAttributeId);
    //
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //     RoadSegmentId? temporarySegmentId;
    //     if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
    //     {
    //         temporarySegmentId = segmentId;
    //         segmentId = permanentSegmentId;
    //     }
    //     else
    //     {
    //         temporarySegmentId = null;
    //     }
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = EuropeanRoadNumber.Parse(command.Number);
    //     return new AddRoadSegmentToEuropeanRoad
    //     (
    //         permanent,
    //         temporary,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         temporarySegmentId,
    //         number,
    //         version
    //     );
    // }
    //
    // private async Task<RemoveRoadSegmentFromEuropeanRoadChange> Translate(RemoveRoadSegmentFromEuropeanRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = new AttributeId(command.AttributeId);
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = EuropeanRoadNumber.Parse(command.Number);
    //     return new RemoveRoadSegmentFromEuropeanRoad
    //     (
    //         permanent,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         number,
    //         version
    //     );
    // }
    //
    // private async Task<AddRoadSegmentToNationalRoadChange> Translate(AddRoadSegmentToNationalRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = await _roadNetworkIdProvider.NextNationalRoadAttributeId();
    //     var temporary = new AttributeId(command.TemporaryAttributeId);
    //
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //     RoadSegmentId? temporarySegmentId;
    //     if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
    //     {
    //         temporarySegmentId = segmentId;
    //         segmentId = permanentSegmentId;
    //     }
    //     else
    //     {
    //         temporarySegmentId = null;
    //     }
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = NationalRoadNumber.Parse(command.Number);
    //     return new AddRoadSegmentToNationalRoad
    //     (
    //         permanent,
    //         temporary,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         temporarySegmentId,
    //         number,
    //         version
    //     );
    // }
    //
    // private async Task<RemoveRoadSegmentFromNationalRoadChange> Translate(RemoveRoadSegmentFromNationalRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = new AttributeId(command.AttributeId);
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = NationalRoadNumber.Parse(command.Number);
    //
    //     return new RemoveRoadSegmentFromNationalRoad
    //     (
    //         permanent,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         number,
    //         version
    //     );
    // }
    //
    // private async Task<AddRoadSegmentToNumberedRoadChange> Translate(AddRoadSegmentToNumberedRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = await _roadNetworkIdProvider.NextNumberedRoadAttributeId();
    //     var temporary = new AttributeId(command.TemporaryAttributeId);
    //
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //     RoadSegmentId? temporarySegmentId;
    //     if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
    //     {
    //         temporarySegmentId = segmentId;
    //         segmentId = permanentSegmentId;
    //     }
    //     else
    //     {
    //         temporarySegmentId = null;
    //     }
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = NumberedRoadNumber.Parse(command.Number);
    //     var direction = RoadSegmentNumberedRoadDirection.Parse(command.Direction);
    //     var ordinal = new RoadSegmentNumberedRoadOrdinal(command.Ordinal);
    //
    //     return new AddRoadSegmentToNumberedRoad
    //     (
    //         permanent,
    //         temporary,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         temporarySegmentId,
    //         number,
    //         direction,
    //         ordinal,
    //         version
    //     );
    // }
    //
    // private async Task<RemoveRoadSegmentFromNumberedRoadChange> Translate(RemoveRoadSegmentFromNumberedRoadCommand command, IRequestedChangeIdentityTranslator translator, CancellationToken cancellationToken)
    // {
    //     var permanent = new AttributeId(command.AttributeId);
    //     var segmentId = new RoadSegmentId(command.SegmentId);
    //
    //     var segmentGeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.SegmentGeometryDrawMethod);
    //     var version = await GetNextRoadSegmentVersionIfNotIncrementedYet(segmentGeometryDrawMethod, segmentId, translator, cancellationToken);
    //
    //     var number = NumberedRoadNumber.Parse(command.Number);
    //
    //     return new RemoveRoadSegmentFromNumberedRoad
    //     (
    //         permanent,
    //         segmentGeometryDrawMethod,
    //         segmentId,
    //         number,
    //         version
    //     );
    // }
    //
    // private async Task<AddGradeSeparatedJunctionChange> Translate(AddGradeSeparatedJunctionCommand command, IRequestedChangeIdentityTranslator translator)
    // {
    //     var permanent = await _roadNetworkIdProvider.NextGradeSeparatedJunctionId();
    //     var temporary = new GradeSeparatedJunctionId(command.TemporaryId);
    //
    //     var upperSegmentId = new RoadSegmentId(command.UpperSegmentId);
    //     RoadSegmentId? temporaryUpperSegmentId;
    //     if (translator.TryTranslateToPermanent(upperSegmentId, out var permanentUpperSegmentId))
    //     {
    //         temporaryUpperSegmentId = upperSegmentId;
    //         upperSegmentId = permanentUpperSegmentId;
    //     }
    //     else
    //     {
    //         temporaryUpperSegmentId = null;
    //     }
    //
    //     var lowerSegmentId = new RoadSegmentId(command.LowerSegmentId);
    //     RoadSegmentId? temporaryLowerSegmentId;
    //     if (translator.TryTranslateToPermanent(lowerSegmentId, out var permanentLowerSegmentId))
    //     {
    //         temporaryLowerSegmentId = lowerSegmentId;
    //         lowerSegmentId = permanentLowerSegmentId;
    //     }
    //     else
    //     {
    //         temporaryLowerSegmentId = null;
    //     }
    //
    //     return new AddGradeSeparatedJunction(
    //         permanent,
    //         temporary,
    //         GradeSeparatedJunctionType.Parse(command.Type),
    //         upperSegmentId,
    //         temporaryUpperSegmentId,
    //         lowerSegmentId,
    //         temporaryLowerSegmentId);
    // }
    //
    // private ModifyGradeSeparatedJunctionChange Translate(ModifyGradeSeparatedJunctionCommand command, IRequestedChangeIdentityTranslator translator)
    // {
    //     var permanent = new GradeSeparatedJunctionId(command.Id);
    //
    //     var upperSegmentId = new RoadSegmentId(command.UpperSegmentId);
    //     RoadSegmentId? temporaryUpperSegmentId;
    //     if (translator.TryTranslateToPermanent(upperSegmentId, out var permanentUpperSegmentId))
    //     {
    //         temporaryUpperSegmentId = upperSegmentId;
    //         upperSegmentId = permanentUpperSegmentId;
    //     }
    //     else
    //     {
    //         temporaryUpperSegmentId = null;
    //     }
    //
    //     var lowerSegmentId = new RoadSegmentId(command.LowerSegmentId);
    //     RoadSegmentId? temporaryLowerSegmentId;
    //     if (translator.TryTranslateToPermanent(lowerSegmentId, out var permanentLowerSegmentId))
    //     {
    //         temporaryLowerSegmentId = lowerSegmentId;
    //         lowerSegmentId = permanentLowerSegmentId;
    //     }
    //     else
    //     {
    //         temporaryLowerSegmentId = null;
    //     }
    //
    //     return new ModifyGradeSeparatedJunction(
    //         permanent,
    //         GradeSeparatedJunctionType.Parse(command.Type),
    //         upperSegmentId,
    //         temporaryUpperSegmentId,
    //         lowerSegmentId,
    //         temporaryLowerSegmentId);
    // }
    //
    // private RemoveGradeSeparatedJunctionChange Translate(RemoveGradeSeparatedJunctionCommand command)
    // {
    //     var permanent = new GradeSeparatedJunctionId(command.Id);
    //
    //     return new RemoveGradeSeparatedJunction(permanent);
    // }

    private RoadSegmentLaneAttributeChange[]? Translate(RequestedRoadSegmentLaneAttribute[]? attributes)
    {
        if (attributes is null)
        {
            return null;
        }

        var result = new List<RoadSegmentLaneAttributeChange>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentLaneAttributeChange(
                new AttributeId(item.AttributeId),
                new RoadSegmentLaneCount(item.Count),
                RoadSegmentLaneDirection.Parse(item.Direction),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition)
            ));
        }

        return result.ToArray();
    }

    private RoadSegmentWidthAttributeChange[]? Translate(RequestedRoadSegmentWidthAttribute[]? attributes)
    {
        if (attributes is null)
        {
            return null;
        }

        var result = new List<RoadSegmentWidthAttributeChange>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentWidthAttributeChange(
                new AttributeId(item.AttributeId),
                new RoadSegmentWidth(item.Width),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition)
            ));
        }

        return result.ToArray();
    }

    private RoadSegmentSurfaceAttributeChange[]? Translate(RequestedRoadSegmentSurfaceAttribute[]? attributes)
    {
        if (attributes is null)
        {
            return null;
        }

        var result = new List<RoadSegmentSurfaceAttributeChange>();

        foreach (var item in attributes)
        {
            result.Add(new RoadSegmentSurfaceAttributeChange(
                new AttributeId(item.AttributeId),
                RoadSegmentSurfaceType.Parse(item.Type),
                new RoadSegmentPosition(item.FromPosition),
                new RoadSegmentPosition(item.ToPosition)
            ));
        }

        return result.ToArray();
    }

    private sealed class RankChangeBeforeTranslation : IComparer<SortableChange>
    {
        private static readonly Type[] SequenceByTypeOfChange =
        {
            //typeof(AddRoadNodeCommand),
            typeof(AddRoadSegmentCommand),
            // typeof(AddRoadSegmentToEuropeanRoadCommand),
            // typeof(AddRoadSegmentToNationalRoadCommand),
            // typeof(AddRoadSegmentToNumberedRoadCommand),
            // typeof(AddGradeSeparatedJunctionCommand),
            // typeof(ModifyRoadNodeCommand),
            typeof(ModifyRoadSegmentCommand),
            // typeof(ModifyGradeSeparatedJunctionCommand),
            // typeof(RemoveRoadSegmentFromEuropeanRoadCommand),
            // typeof(RemoveRoadSegmentFromNationalRoadCommand),
            // typeof(RemoveRoadSegmentFromNumberedRoadCommand),
            // typeof(RemoveGradeSeparatedJunctionCommand),
            // typeof(RemoveRoadSegmentCommand),
            // typeof(RemoveRoadNodeCommand)
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
}
