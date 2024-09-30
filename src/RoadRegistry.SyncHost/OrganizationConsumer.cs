namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.Organizations;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Sync.OrganizationRegistry.Extensions;
using SqlStreamStore;
using Sync.OrganizationRegistry;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extensions;
using Sync.OrganizationRegistry.Exceptions;
using Organization = Sync.OrganizationRegistry.Models.Organization;

public class OrganizationConsumer : RoadRegistryBackgroundService
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public const string ProjectionStateName = "roadregistry-sync-organization";

    private readonly ILifetimeScope _container;
    private readonly OrganizationConsumerOptions _options;
    private readonly IOrganizationReader _organizationReader;
    private readonly OrganizationDbaseRecordReader _organizationRecordReader;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IStreamStore _store;

    public OrganizationConsumer(
        ILifetimeScope container,
        OrganizationConsumerOptions options,
        IOrganizationReader organizationReader,
        IStreamStore store,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<OrganizationConsumer>())
    {
        _container = container.ThrowIfNull();
        _options = options.ThrowIfNull();
        _organizationReader = organizationReader.ThrowIfNull();
        _store = store.ThrowIfNull();
        _roadNetworkCommandQueue = roadNetworkCommandQueue.ThrowIfNull();
        _organizationRecordReader = new OrganizationDbaseRecordReader(manager, fileEncoding);
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        var consecutiveExceptionsCount = 0;
        var serviceIsUnavailable = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            ILookup<OrganizationOvoCode, OrganizationId> ovoCodeToClassicIdMapping = null;

            await using var scope = _container.BeginLifetimeScope();
            await using var dbContext = scope.Resolve<OrganizationConsumerContext>();

            var projectionState = await dbContext.InitializeProjectionState(ProjectionStateName, cancellationToken);
            projectionState.ErrorMessage = null;

            try
            {
                var map = _container.Resolve<EventSourcedEntityMap>();
                var organizationsContext = new Organizations(map, _store, SerializerSettings, EventMapping);

                await _organizationReader.ReadAsync(projectionState.Position + 1, async organization =>
                {
                    if (serviceIsUnavailable)
                    {
                        Logger.LogError("Organization registry is available, continuing sync");
                        serviceIsUnavailable = false;
                    }

                    projectionState.Position = organization.ChangeId;
                    var idempotenceKey = $"organization-{organization.ChangeId}-{organization.OvoNumber}".ToSha512();

                    var messageAlreadyProcessed = await dbContext.ProcessedMessages
                        .AsNoTracking()
                        .AnyAsync(x => x.IdempotenceKey == idempotenceKey, cancellationToken)
                        .ConfigureAwait(false);

                    if (messageAlreadyProcessed)
                    {
                        Logger.LogWarning($"Skipping already processed message at position '{projectionState.Position}' with idempotenceKey '{idempotenceKey}'.");
                        await dbContext.SaveChangesAsync(cancellationToken);
                        return;
                    }

                    Logger.LogInformation("Organization {OvoNumber}: processing...", organization.OvoNumber);

                    //TODO-rik drop this cache once OrganizationsV2 table is there, then only query to whats needed
                    ovoCodeToClassicIdMapping ??= await BuildOvoCodeToClassicIdMapping(scope, cancellationToken);

                    await CreateOrUpdateOrganization(ovoCodeToClassicIdMapping, organizationsContext, organization, cancellationToken);

                    await dbContext.ProcessedMessages.AddAsync(new ProcessedMessage(idempotenceKey, DateTimeOffset.UtcNow), cancellationToken);
                    await dbContext.SaveChangesAsync(cancellationToken);

                    Logger.LogInformation("Organization {OvoNumber}: processed", organization.OvoNumber);
                }, cancellationToken);

                consecutiveExceptionsCount = 0;
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.LogError(ex.Message);
                return;
            }
            catch (OrganizationRegistryTemporarilyUnavailableException ex)
            {
                if (!serviceIsUnavailable)
                {
                    Logger.LogError("Organization registry is temporarily unavailable");
                    serviceIsUnavailable = true;

                    projectionState.ErrorMessage = "Organization registry service unavailable";
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                consecutiveExceptionsCount++;

                if (consecutiveExceptionsCount > 3)
                {
                    Logger.LogCritical(ex, "Error consuming organizations, trying again in {seconds} seconds (attempt #{ConsecutiveExceptionsCount})", _options.ConsumerDelaySeconds, consecutiveExceptionsCount);
                }

                projectionState.ErrorMessage = ex.Message;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            if (_options.ConsumerDelaySeconds == -1)
            {
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ConsumerDelaySeconds), cancellationToken);
        }
    }

    private async Task<ILookup<OrganizationOvoCode, OrganizationId>> BuildOvoCodeToClassicIdMapping(ILifetimeScope scope, CancellationToken cancellationToken)
    {
        await using var editorContext = scope.Resolve<EditorContext>();

        Logger.LogInformation("Fetching all organizations...");
        var organizationRecords = await editorContext.Organizations
            .IncludeLocalToListAsync(q => q.Where(org => !org.Code.StartsWith(OrganizationOvoCode.Prefix)), cancellationToken);

        var orgIdMapping = organizationRecords
            .Select(x => _organizationRecordReader.Read(x.DbaseRecord, x.DbaseSchemaVersion))
            .Where(x => x.OvoCode is not null)
            .ToLookup(x => x.OvoCode!.Value, x => x.Code);
        Logger.LogInformation("{Count} organizations have a link to another OVO-code", orgIdMapping.Count);

        return orgIdMapping;
    }

    private async Task CreateOrUpdateOrganization(ILookup<OrganizationOvoCode, OrganizationId> ovoCodeToClassicIdMapping, Organizations organizationsContext, Organization organization, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ovoCode = new OrganizationOvoCode(organization.OvoNumber);
        var kboNumber = OrganizationKboNumber.FromValue(organization.KboNumber);

        //TODO-rik stop using idmapping and use sql directly
        var classicOrganizationIds = ovoCodeToClassicIdMapping[ovoCode].ToList();
        if (classicOrganizationIds.Count > 1)
        {
            Logger.LogError($"Multiple Organizations found with a link to {ovoCode}, not proceeding with automatic rename to '{organization.Name}' (Ids: {string.Join(", ", classicOrganizationIds)}");
            return;
        }

        var organizationId = classicOrganizationIds.Any() ? classicOrganizationIds.Single() : new OrganizationId(ovoCode);

        var existingOrganization = await organizationsContext.FindAsync(organizationId, cancellationToken);
        if (existingOrganization is null)
        {
            Logger.LogInformation("Organization {OvoNumber}: creating new", organization.OvoNumber);
            var command = new CreateOrganization
            {
                Code = ovoCode,
                Name = organization.Name,
                OvoCode = ovoCode,
                KboNumber = kboNumber
            };
            await _roadNetworkCommandQueue.WriteAsync(new Command(command), cancellationToken);
        }
        else
        {
            Logger.LogInformation("Organization {OvoNumber}: updating", organization.OvoNumber);
            var command = new ChangeOrganization
            {
                Code = ovoCode,
                Name = organization.Name,
                OvoCode = ovoCode,
                KboNumber = kboNumber
            };
            await _roadNetworkCommandQueue.WriteAsync(new Command(command), cancellationToken);
        }
    }
}
