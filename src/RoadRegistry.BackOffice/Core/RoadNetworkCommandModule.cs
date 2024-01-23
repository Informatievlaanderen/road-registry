namespace RoadRegistry.BackOffice.Core;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using SqlStreamStore.Streams;
using Uploads;

public class RoadNetworkCommandModule : CommandHandlerModule
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
    private readonly IExtractUploadFailedEmailClient _emailClient;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;
    private readonly IOrganizationEventWriter _organizationEventWriter;
    private readonly ILogger _logger;

    public RoadNetworkCommandModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
        IExtractUploadFailedEmailClient emailClient,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(emailClient);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _lifetimeScope = lifetimeScope;
        _useOvoCodeInChangeRoadNetworkFeatureToggle = useOvoCodeInChangeRoadNetworkFeatureToggle;
        _emailClient = emailClient;
        _logger = loggerFactory.CreateLogger<RoadNetworkCommandModule>();
        _roadNetworkEventWriter = roadNetworkEventWriter;

        var enricher = EnrichEvent.WithTime(clock);
        _organizationEventWriter = new OrganizationEventWriter(store, enricher);

        For<ChangeRoadNetwork>()
            .UseValidator(new ChangeRoadNetworkValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(ChangeRoadNetwork);

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(CreateOrganization);
        
        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(DeleteOrganization);
        
        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(RenameOrganization);

        For<ChangeOrganization>()
            .UseValidator(new ChangeOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(ChangeOrganization);

        For<UnlinkRoadSegmentsFromStreetName>()
            .UseValidator(new UnlinkRoadSegmentsFromStreetNameValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(UnlinkRoadSegmentsFromStreetName);
    }

    private async Task ChangeRoadNetwork(IRoadRegistryContext context, Command<ChangeRoadNetwork> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);
        
        var request = ChangeRequestId.FromString(command.Body.RequestId);
        var downloadId = DownloadId.FromValue(command.Body.DownloadId);
        var @operator = new OperatorName(command.Body.Operator);
        var reason = new Reason(command.Body.Reason);

        var sw = Stopwatch.StartNew();
        var organizationId = new OrganizationId(command.Body.OrganizationId);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);
        _logger.LogInformation("TIMETRACKING changeroadnetwork: finding organization took {Elapsed}", sw.Elapsed);

        var organizationTranslation = ToDutchTranslation(organization, organizationId);

        sw.Restart();

        var successChangedMessages = new Dictionary<IEventSourcedEntity, List<IMessage>>();
        var failedChangedMessages = new Dictionary<IEventSourcedEntity, List<IMessage>>();

        using (var container = _lifetimeScope.BeginLifetimeScope())
        {
            var idGenerator = container.Resolve<IRoadNetworkIdGenerator>();

            var roadNetworkStreamChanges = await SplitChangesByRoadNetworkStream(idGenerator, command.Body.Changes);

            foreach (var roadNetworkStreamChange in roadNetworkStreamChanges)
            {
                var streamName = roadNetworkStreamChange.Key;
                var changes = roadNetworkStreamChange.Value;

                var network = await context.RoadNetworks.Get(streamName, cancellationToken);
                _logger.LogInformation("TIMETRACKING changeroadnetwork: loading RoadNetwork [{StreamName}] took {Elapsed}", streamName, sw.Elapsed);

                var translator = new RequestedChangeTranslator(
                    network.CreateIdProvider(idGenerator),
                    network.ProvidesNextRoadNodeVersion(),
                    network.ProvidesNextRoadSegmentVersion(),
                    network.ProvidesNextRoadSegmentGeometryVersion()
                );
                sw.Restart();
                var requestedChanges = await translator.Translate(changes, context.Organizations, cancellationToken);
                _logger.LogInformation("TIMETRACKING changeroadnetwork: translating command changes to RequestedChanges took {Elapsed}", sw.Elapsed);

                sw.Restart();
                var changedMessage = await network.Change(request, downloadId, reason, @operator, organizationTranslation, requestedChanges, _emailClient, cancellationToken);
                _logger.LogInformation("TIMETRACKING changeroadnetwork: applying RequestedChanges to RoadNetwork took {Elapsed}", sw.Elapsed);

                if (changedMessage is RoadNetworkChangesRejected)
                {
                    failedChangedMessages.TryAdd(network, new List<IMessage>());
                    failedChangedMessages[network].Add(changedMessage);
                }
                else
                {
                    successChangedMessages.TryAdd(network, new List<IMessage>());
                    successChangedMessages[network].Add(changedMessage);
                }
            }
        }
        
        if (failedChangedMessages.Any() && successChangedMessages.Any())
        {
            foreach (var item in successChangedMessages)
            foreach (var @event in item.Value)
            {
                context.EventFilter.Exclude(item.Key, @event);
            }
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task CreateOrganization(IRoadRegistryContext context, Command<CreateOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            var rejectedEvent = new CreateOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            };
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }
        else
        {
            var acceptedEvent = new Event(new CreateOrganizationAccepted
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            }).WithMessageId(command.MessageId);
            await _organizationEventWriter.WriteAsync(organizationId, acceptedEvent, cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task DeleteOrganization(IRoadRegistryContext context, Command<DeleteOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);
        
        if (organization != null)
        {
            organization.Delete();
        }
        else
        {
            var rejectedEvent = new DeleteOrganizationRejected
            {
                Code = command.Body.Code
            };
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task RenameOrganization(IRoadRegistryContext context, Command<RenameOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            organization.Rename(new OrganizationName(command.Body.Name));
        }
        else
        {
            var rejectedEvent = new RenameOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name
            };
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task ChangeOrganization(IRoadRegistryContext context, Command<ChangeOrganization> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization is not null)
        {
            organization.Change(
                command.Body.Name is not null ? OrganizationName.WithoutExcessLength(command.Body.Name) : null,
                OrganizationOvoCode.FromValue(command.Body.OvoCode)
            );
        }
        else
        {
            var rejectedEvent = new ChangeOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            };
            await _roadNetworkEventWriter.WriteAsync(RoadNetworkStreamNameProvider.Default, ExpectedVersion.Any, new Event(
                rejectedEvent
            ).WithMessageId(command.MessageId), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task UnlinkRoadSegmentsFromStreetName(IRoadRegistryContext context, Command<UnlinkRoadSegmentsFromStreetName> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var roadNetwork = await context.RoadNetworks.Get(cancellationToken);

        var streetNameId = new CrabStreetNameId(command.Body.Id);

        var segments = roadNetwork.FindRoadSegments(x => x.AttributeHash.LeftStreetNameId == streetNameId || x.AttributeHash.RightStreetNameId == streetNameId);
        if (segments.Any())
        {
            var reason = $"Wegsegmenten ontkoppelen van straatnaam {command.Body.Id}";
            var organizationId = new OrganizationId("AGIV");

            var translatedChanges = TranslatedChanges.Empty
                .WithOrganization(organizationId)
                .WithOperatorName(new OperatorName(organizationId))
                .WithReason(new Reason(reason));

            var recordNumber = RecordNumber.Initial;
            var attributeId = AttributeId.Initial;

            foreach (var roadSegment in segments)
            {
                var modifyRoadSegment = new Uploads.ModifyRoadSegment(
                    recordNumber,
                    roadSegment.Id,
                    roadSegment.Start,
                    roadSegment.End,
                    roadSegment.AttributeHash.OrganizationId,
                    roadSegment.AttributeHash.GeometryDrawMethod,
                    roadSegment.AttributeHash.Morphology,
                    roadSegment.AttributeHash.Status,
                    roadSegment.AttributeHash.Category,
                    roadSegment.AttributeHash.AccessRestriction,
                    roadSegment.AttributeHash.LeftStreetNameId == streetNameId ? CrabStreetNameId.NotApplicable : roadSegment.AttributeHash.LeftStreetNameId,
                    roadSegment.AttributeHash.RightStreetNameId == streetNameId ? CrabStreetNameId.NotApplicable : roadSegment.AttributeHash.RightStreetNameId
                ).WithGeometry(roadSegment.Geometry);

                foreach (var lane in roadSegment.Lanes)
                {
                    modifyRoadSegment = modifyRoadSegment.WithLane(new Uploads.RoadSegmentLaneAttribute(attributeId, lane.Count, lane.Direction, lane.From, lane.To));
                    attributeId = attributeId.Next();
                }
                foreach (var surface in roadSegment.Surfaces)
                {
                    modifyRoadSegment = modifyRoadSegment.WithSurface(new Uploads.RoadSegmentSurfaceAttribute(attributeId, surface.Type, surface.From, surface.To));
                    attributeId = attributeId.Next();
                }
                foreach (var width in roadSegment.Widths)
                {
                    modifyRoadSegment = modifyRoadSegment.WithWidth(new Uploads.RoadSegmentWidthAttribute(attributeId, width.Width, width.From, width.To));
                    attributeId = attributeId.Next();
                }

                translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);
            }
            
            var requestedChanges = translatedChanges.Select(change =>
            {
                var requestedChange = new RequestedChange();
                change.TranslateTo(requestedChange);
                return requestedChange;
            }).ToList();
            
            var changeRoadNetwork = new ChangeRoadNetwork
            {
                RequestId = ChangeRequestId.FromArchiveId(ArchiveId.FromGuid(command.MessageId)),
                Changes = requestedChanges.ToArray(),
                Reason = translatedChanges.Reason,
                Operator = translatedChanges.Operator,
                OrganizationId = translatedChanges.Organization
            };
            
            await ChangeRoadNetwork(context, new Command<ChangeRoadNetwork>(
                    new Command(changeRoadNetwork).WithMessageId(command.MessageId)
                ), applicationMetadata, cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }
    
    private async Task FillMissingPermanentIdsForAddedOutlineRoadSegments(IRoadNetworkIdGenerator idGenerator, RequestedChange[] changes)
    {
        foreach (var change in changes
                     .Where(x => x.AddRoadSegment?.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                                 && x.AddRoadSegment.PermanentId is null))
        {
            change.AddRoadSegment.PermanentId = await idGenerator.NewRoadSegmentId();
        }
    }

    private async Task<Dictionary<StreamName, RequestedChange[]>> SplitChangesByRoadNetworkStream(IRoadNetworkIdGenerator idGenerator, RequestedChange[] changes)
    {
        await FillMissingPermanentIdsForAddedOutlineRoadSegments(idGenerator, changes);
        
        var roadNetworkStreamChanges = changes
            .Select(change => new
            {
                RoadSegmentId = change.AddRoadSegment?.PermanentId
                                ?? change.ModifyRoadSegment?.Id
                                ?? change.RemoveRoadSegment?.Id
                                ?? change.ModifyRoadSegmentAttributes?.Id
                                ?? change.ModifyRoadSegmentGeometry?.Id
                                ?? change.RemoveOutlinedRoadSegment?.Id,
                GeometryDrawMethod = change.AddRoadSegment?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegment?.GeometryDrawMethod
                                     ?? change.RemoveRoadSegment?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegmentAttributes?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegmentGeometry?.GeometryDrawMethod
                                     ?? (change.RemoveOutlinedRoadSegment is not null ? RoadSegmentGeometryDrawMethod.Outlined : null),
                Change = change
            })
            .GroupBy(x =>
                x.RoadSegmentId is not null && x.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                    ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(x.RoadSegmentId.Value))
                    : RoadNetworkStreamNameProvider.Default, x => x.Change)
            .ToDictionary(x => x.Key, x => x.ToArray());

        if (!roadNetworkStreamChanges.Any())
        {
            roadNetworkStreamChanges.Add(RoadNetworkStreamNameProvider.Default, changes);
        }

        return roadNetworkStreamChanges;
    }
    
    private Organization.DutchTranslation ToDutchTranslation(Organization organization, OrganizationId organizationId)
    {
        if (organization is null)
        {
            return Organization.PredefinedTranslations.FromSystemValue(organizationId);
        }

        if (_useOvoCodeInChangeRoadNetworkFeatureToggle.FeatureEnabled && organization.OvoCode is not null)
        {
            return new Organization.DutchTranslation(new OrganizationId(organization.OvoCode.Value), organization.Translation.Name);
        }

        return organization.Translation
               ?? ToDutchTranslation(null, organizationId);
    }
}
