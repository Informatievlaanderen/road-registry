namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using CommandHandling;
using CommandHandling.DutchTranslations;
using CommandHandling.Extracts;
using DutchTranslations;
using Extracts;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NodaTime;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.ValueObjects;
using SqlStreamStore;
using TicketingService.Abstractions;
using RoadNetworkChangesSummary = Messages.RoadNetworkChangesSummary;

public class RoadNetworkCommandModule : CommandHandlerModule
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly IExtractUploadFailedEmailClient _emailClient;
    private readonly ILogger _logger;

    public RoadNetworkCommandModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        IExtractUploadFailedEmailClient emailClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(emailClient);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _lifetimeScope = lifetimeScope;
        _emailClient = emailClient;
        _logger = loggerFactory.CreateLogger<RoadNetworkCommandModule>();

        var enricher = EnrichEvent.WithTime(clock);

        For<ChangeRoadNetwork>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, enricher)
            .Handle(ChangeRoadNetwork);
    }

    private async Task ChangeRoadNetwork(IRoadRegistryContext context, Command<ChangeRoadNetwork> command, ApplicationMetadata applicationMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var request = ChangeRequestId.FromString(command.Body.RequestId);
        var downloadId = DownloadId.FromValue(command.Body.DownloadId);
        var @operator = new OperatorName(command.Body.Operator);
        var reason = new Reason(command.Body.Reason);
        var ticketId = TicketId.FromValue(command.Body.TicketId);

        await using (var container = _lifetimeScope.BeginLifetimeScope())
        {
            if (ticketId is not null)
            {
                var ticketing = container.Resolve<ITicketing>();
                var ticket = await ticketing.Get(ticketId.Value, cancellationToken);
                if (ticket?.Status == TicketStatus.Created)
                {
                    await ticketing.Pending(ticketId.Value, cancellationToken);
                }
            }

            var sw = Stopwatch.StartNew();
            var organizationId = new OrganizationId(command.Body.OrganizationId);
            var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);
            _logger.LogInformation("TIMETRACKING changeroadnetwork: finding organization took {Elapsed}", sw.Elapsed);

            var organizationTranslation = Organization.ToDutchTranslation(organization, organizationId);

            sw.Restart();

            var successChangedMessages = new Dictionary<IEventSourcedEntity, List<IMessage>>();
            var failedChangedMessages = new Dictionary<IEventSourcedEntity, List<RoadNetworkChangesRejected>>();

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
                var changedMessage = await network.Change(request, downloadId, reason, @operator, organizationTranslation, requestedChanges, ticketId, _emailClient, context.Organizations, cancellationToken);
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
                {
                    foreach (var @event in item.Value)
                    {
                        context.EventFilter.Exclude(item.Key, @event);
                    }
                }
            }

            if (command.Body.UseExtractsV2)
            {
                var extractsRequests = container.Resolve<IExtractRequests>();

                if (failedChangedMessages.Any())
                {
                    await extractsRequests.UploadRejectedAsync(downloadId!.Value, cancellationToken);
                }
                else
                {
                    await extractsRequests.UploadAcceptedAsync(downloadId!.Value, cancellationToken);
                }
            }
            else
            {
                if (!failedChangedMessages.Any() && command.Body.ExtractRequestId is not null)
                {
                    var extractRequestId = ExtractRequestId.FromString(command.Body.ExtractRequestId);

                    var extract = await context.RoadNetworkExtracts.Get(extractRequestId, cancellationToken);
                    extract.Close(RoadNetworkExtractCloseReason.UploadAccepted);
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
                                    Text = ProblemTranslator.Dutch(problem).Message
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

    private async Task FillMissingPermanentIdsForAddedOutlineRoadSegments(IRoadNetworkIdGenerator idGenerator, RequestedChange[] changes)
    {
        foreach (var change in changes
                     .Where(x => x.AddRoadSegment?.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                                 && x.AddRoadSegment!.PermanentId is null))
        {
            change.AddRoadSegment.PermanentId = await idGenerator.NewRoadSegmentIdAsync();
        }
    }

    private async Task<Dictionary<StreamName, RequestedChange[]>> SplitChangesByRoadNetworkStream(IRoadNetworkIdGenerator idGenerator, RequestedChange[] changes)
    {
        await FillMissingPermanentIdsForAddedOutlineRoadSegments(idGenerator, changes);

        return RequestedChangesConverter.SplitChangesByRoadNetworkStream(changes);
    }
}

public static class RequestedChangesConverter
{
    public static Dictionary<StreamName, RequestedChange[]> SplitChangesByRoadNetworkStream(RequestedChange[] changes)
    {
        var changesWithStream = changes
            .SelectMany(change =>
            {
                if (change.RemoveRoadSegments is not null)
                {
                    if (change.RemoveRoadSegments.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
                    {
                        return change.RemoveRoadSegments.Ids.Select(roadSegmentId =>
                            new
                            {
                                Change = new RequestedChange
                                {
                                    RemoveRoadSegments = new Messages.RemoveRoadSegments
                                    {
                                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
                                        Ids = [roadSegmentId]
                                    }
                                },
                                StreamName = RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(roadSegmentId))
                            });
                    }

                    return
                    [
                        new
                        {
                            Change = change,
                            StreamName = RoadNetworkStreamNameProvider.Default
                        }
                    ];
                }

                var id =
                    change.AddRoadSegment?.PermanentId
                    ?? change.ModifyRoadSegment?.Id
                    ?? change.RemoveRoadSegment?.Id
                    ?? change.RemoveOutlinedRoadSegment?.Id
                    ?? change.RemoveOutlinedRoadSegmentFromRoadNetwork?.Id
                    ?? change.AddRoadSegmentToEuropeanRoad?.SegmentId
                    ?? change.AddRoadSegmentToNationalRoad?.SegmentId
                    ?? change.AddRoadSegmentToNumberedRoad?.SegmentId
                    ?? change.RemoveRoadSegmentFromEuropeanRoad?.SegmentId
                    ?? change.RemoveRoadSegmentFromNationalRoad?.SegmentId
                    ?? change.RemoveRoadSegmentFromNumberedRoad?.SegmentId;

                var geometryDrawMethod = change.AddRoadSegment?.GeometryDrawMethod
                                         ?? change.ModifyRoadSegment?.GeometryDrawMethod
                                         ?? change.RemoveRoadSegment?.GeometryDrawMethod
                                         ?? (change.RemoveOutlinedRoadSegment is not null ? RoadSegmentGeometryDrawMethod.Outlined.ToString() : null)
                                         ?? (change.RemoveOutlinedRoadSegmentFromRoadNetwork is not null ? RoadSegmentGeometryDrawMethod.Measured.ToString() : null)
                                         ?? change.AddRoadSegmentToEuropeanRoad?.SegmentGeometryDrawMethod
                                         ?? change.AddRoadSegmentToNationalRoad?.SegmentGeometryDrawMethod
                                         ?? change.AddRoadSegmentToNumberedRoad?.SegmentGeometryDrawMethod
                                         ?? change.RemoveRoadSegmentFromEuropeanRoad?.SegmentGeometryDrawMethod
                                         ?? change.RemoveRoadSegmentFromNationalRoad?.SegmentGeometryDrawMethod
                                         ?? change.RemoveRoadSegmentFromNumberedRoad?.SegmentGeometryDrawMethod;

                if (geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined && id is null)
                {
                    throw new InvalidOperationException($"No outlined road segment ID found for change {change.Flatten().GetType().Name}");
                }

                return
                [
                    new
                    {
                        Change = change,
                        StreamName = geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                            ? RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(
                                new RoadSegmentId(id.Value))
                            : RoadNetworkStreamNameProvider.Default
                    }
                ];
            })
            .GroupBy(x => x.StreamName, x => x.Change)
            .ToDictionary(x => x.Key, x => x.ToArray());

        if (!changesWithStream.Any())
        {
            changesWithStream.Add(RoadNetworkStreamNameProvider.Default, changes);
        }

        return changesWithStream;
    }
}
