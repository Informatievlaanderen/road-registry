namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.EventHandling;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NodaTime;
using SqlStreamStore;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;
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

        For<CheckUploadHealth>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(CheckUploadHealth);

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
    }

    private async Task CheckUploadHealth(IRoadRegistryContext context, Command<CheckUploadHealth> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.Body.TicketId);

        await using var container = _lifetimeScope.BeginLifetimeScope();
        var ticketing = container.Resolve<ITicketing>();

        try
        {
            await context.RoadNetworks.Get(cancellationToken);

            var blobClient = container.Resolve<RoadNetworkUploadsBlobClient>();
            await blobClient.GetBlobAsync(new BlobName(command.Body.FileName), cancellationToken);

            await ticketing.Complete(ticketId, new TicketResult(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CheckUploadHealth)} failed");

            await ticketing.Error(ticketId, new TicketError(), cancellationToken);
        }
    }

    private async Task ChangeRoadNetwork(IRoadRegistryContext context, Command<ChangeRoadNetwork> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var request = ChangeRequestId.FromString(command.Body.RequestId);
        var downloadId = DownloadId.FromValue(command.Body.DownloadId);
        var @operator = new OperatorName(command.Body.Operator);
        var reason = new Reason(command.Body.Reason);
        var ticketId = TicketId.FromValue(command.Body.TicketId);

        var sw = Stopwatch.StartNew();
        var organizationId = new OrganizationId(command.Body.OrganizationId);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);
        _logger.LogInformation("TIMETRACKING changeroadnetwork: finding organization took {Elapsed}", sw.Elapsed);

        var organizationTranslation = ToDutchTranslation(organization, organizationId);

        sw.Restart();

        var successChangedMessages = new Dictionary<IEventSourcedEntity, List<IMessage>>();
        var failedChangedMessages = new Dictionary<IEventSourcedEntity, List<RoadNetworkChangesRejected>>();

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
                    (args, ct) => GetNextVersion(context, args, ct),
                    (args, geometry, ct) => GetNextGeometryVersion(context, args, geometry, ct)
                );
                sw.Restart();
                var requestedChanges = await translator.Translate(changes, context.Organizations, cancellationToken);
                _logger.LogInformation("TIMETRACKING changeroadnetwork: translating command changes to RequestedChanges took {Elapsed}", sw.Elapsed);

                sw.Restart();
                var changedMessage = await network.Change(request, downloadId, reason, @operator, organizationTranslation, requestedChanges, ticketId, _emailClient, cancellationToken);
                _logger.LogInformation("TIMETRACKING changeroadnetwork: applying RequestedChanges to RoadNetwork took {Elapsed}", sw.Elapsed);

                if (changedMessage is RoadNetworkChangesRejected rejectedChangedMessage)
                {
                    failedChangedMessages.TryAdd(network, new List<RoadNetworkChangesRejected>());
                    failedChangedMessages[network].Add(rejectedChangedMessage);
                }
                else
                {
                    successChangedMessages.TryAdd(network, new List<IMessage>());
                    successChangedMessages[network].Add(changedMessage);
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

            if (ticketId is not null)
            {
                var ticketing = container.Resolve<ITicketing>();
                if (failedChangedMessages.Any())
                {
                    var errors = failedChangedMessages
                        .SelectMany(x => x.Value)
                        .SelectMany(x => x.Changes)
                        .SelectMany(x => x.Problems)
                        .Select(problem => problem.ToTicketError())
                        .ToArray();

                    await ticketing.Error(ticketId.Value, new TicketError(errors), cancellationToken);
                }
                else
                {
                    var acceptedChanges = successChangedMessages
                        .SelectMany(x => x.Value)
                        .OfType<RoadNetworkChangesAccepted>()
                        .SelectMany(x => x.Changes)
                        .ToArray();

                    var changes = acceptedChanges
                        .Select(change => new
                        {
                            ChangeType = change.Flatten().GetType().Name,
                            Change = DutchTranslations.AcceptedChange.Translator(change),
                            Problems = change.Problems?
                                .Select(problem => new
                                {
                                    Severity = problem.Severity.ToString(),
                                    problem.Reason,
                                    Text = DutchTranslations.ProblemTranslator.Dutch(problem).Message
                                })
                                .ToArray()
                        })
                        .ToArray();

                    var summary = RoadNetworkChangesSummary.FromAcceptedChanges(acceptedChanges);

                    await ticketing.Complete(ticketId.Value, new TicketResult(new { Changes = changes, Summary = summary }), cancellationToken);
                }
            }
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task<RoadSegmentVersion> GetNextVersion(IRoadRegistryContext context, NextRoadSegmentVersionArgs args, CancellationToken cancellationToken)
    {
        var streamName = args.ConvertedFromOutlined
            ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(args.Id)
            : RoadNetworkStreamNameProvider.Get(args.Id, args.GeometryDrawMethod);

        var network = await context.RoadNetworks.Get(streamName, cancellationToken);
        return network.ProvidesNextRoadSegmentVersion()(args.Id);
    }

    private async Task<GeometryVersion> GetNextGeometryVersion(IRoadRegistryContext context, NextRoadSegmentVersionArgs args, MultiLineString geometry, CancellationToken cancellationToken)
    {
        var streamName = args.ConvertedFromOutlined
            ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(args.Id)
            : RoadNetworkStreamNameProvider.Get(args.Id, args.GeometryDrawMethod);

        var network = await context.RoadNetworks.Get(streamName, cancellationToken);
        return network.ProvidesNextRoadSegmentGeometryVersion()(args.Id, geometry);
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
                                ?? change.RemoveOutlinedRoadSegment?.Id
                                ?? change.RemoveOutlinedRoadSegmentFromRoadNetwork?.Id,
                GeometryDrawMethod = change.AddRoadSegment?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegment?.GeometryDrawMethod
                                     ?? change.RemoveRoadSegment?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegmentAttributes?.GeometryDrawMethod
                                     ?? change.ModifyRoadSegmentGeometry?.GeometryDrawMethod
                                     ?? (change.RemoveOutlinedRoadSegment is not null ? RoadSegmentGeometryDrawMethod.Outlined : null)
                                     ?? (change.RemoveOutlinedRoadSegmentFromRoadNetwork is not null ? RoadSegmentGeometryDrawMethod.Measured : null),
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
