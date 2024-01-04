namespace RoadRegistry.Sync.OrganizationRegistry;

using Autofac;
using BackOffice;
using BackOffice.Abstractions.Organizations;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
using Editor.Schema;
using Extensions;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using SqlStreamStore;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Organization = Models.Organization;

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

        while (!cancellationToken.IsCancellationRequested)
        {
            ILookup<OrganizationOvoCode, OrganizationId>? orgIdMapping = null;

            await using var scope = _container.BeginLifetimeScope();
            await using var context = scope.Resolve<OrganizationConsumerContext>();
            
            var projectionState = await context.InitializeProjectionState(ProjectionStateName, cancellationToken);
            projectionState.ErrorMessage = null;
            
            try
            {
                var map = _container.Resolve<EventSourcedEntityMap>();
                var organizationsContext = new Organizations(map, _store, SerializerSettings, EventMapping);

                var processedCount = 0;

                await _organizationReader.ReadAsync(projectionState.Position + 1, async organization =>
                {
                    Logger.LogInformation("Processing organization {OvoNumber}", organization.OvoNumber);
                    
                    if (processedCount > 0 && organization.ChangeId != projectionState.Position)
                    {
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    if (orgIdMapping is null)
                    {
                        await using var editorContext = scope.Resolve<EditorContext>();

                        Logger.LogInformation("Fetching all organizations...");
                        var organizationRecords = await editorContext.Organizations.ToListAsync(cancellationToken);
                        if (!organizationRecords.Any())
                        {
                            organizationRecords = editorContext.Organizations.Local.ToList();
                        }

                        orgIdMapping = organizationRecords
                            .Select(x => _organizationRecordReader.Read(x.DbaseRecord, x.DbaseSchemaVersion))
                            .Where(x => x.OvoCode is not null)
                            .ToLookup(x => x.OvoCode!.Value, x => x.Code);
                        Logger.LogInformation("{Count} organizations have an OVO-code", orgIdMapping.Count);
                    }
                    
                    await CreateOrUpdateOrganizationWithOvoCode(organizationsContext, organization, cancellationToken);
                    await UpdateOrganizationsWithOldOrganizationId(orgIdMapping, organization, cancellationToken);

                    projectionState.Position = organization.ChangeId;
                    processedCount++;

                    await context.ProcessedMessages.AddAsync(new ProcessedMessage($"organization-{projectionState.Position}-{organization.OvoNumber}".ToSha512(), DateTimeOffset.UtcNow), cancellationToken);

                    Logger.LogInformation("Processed organization {OvoNumber}", organization.OvoNumber);
                }, cancellationToken);

                if (processedCount > 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }

                consecutiveExceptionsCount = 0;
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.LogError(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                consecutiveExceptionsCount++;

                if (consecutiveExceptionsCount > 3)
                {
                    Logger.LogCritical(ex, "Error consuming organizations, trying again in {seconds} seconds (attempt #{ConsecutiveExceptionsCount})", _options.ConsumerDelaySeconds, consecutiveExceptionsCount);
                }

                projectionState.ErrorMessage = ex.Message;
                await context.SaveChangesAsync(cancellationToken);
            }

            if (_options.ConsumerDelaySeconds == -1)
            {
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ConsumerDelaySeconds), cancellationToken);
        }
    }

    private async Task CreateOrUpdateOrganizationWithOvoCode(Organizations organizationsContext, Organization organization, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ovoCode = new OrganizationOvoCode(organization.OvoNumber);

        var existingOrganization = await organizationsContext.FindAsync(new OrganizationId(ovoCode), cancellationToken);
        if (existingOrganization is null)
        {
            Logger.LogInformation("Creating new organization {OvoNumber}", organization.OvoNumber);
            var command = new CreateOrganization
            {
                Code = ovoCode,
                Name = organization.Name,
                OvoCode = ovoCode
            };
            await _roadNetworkCommandQueue
                .Write(new Command(command), cancellationToken);
        }
        else
        {
            Logger.LogInformation("Changing organization {OvoNumber}", organization.OvoNumber);
            var command = new ChangeOrganization
            {
                Code = ovoCode,
                Name = organization.Name,
                OvoCode = ovoCode
            };
            await _roadNetworkCommandQueue
                .Write(new Command(command), cancellationToken);
        }
    }

    private async Task UpdateOrganizationsWithOldOrganizationId(ILookup<OrganizationOvoCode, OrganizationId> orgIdMapping, Organization organization, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var ovoCode = new OrganizationOvoCode(organization.OvoNumber);

        foreach (var organizationId in orgIdMapping[ovoCode])
        {
            Logger.LogInformation("Changing organization {OrganizationId} to map to {OvoCode}", organizationId, ovoCode);
            var command = new ChangeOrganization
            {
                Code = organizationId,
                Name = organization.Name,
                OvoCode = ovoCode
            };
            await _roadNetworkCommandQueue
                .Write(new Command(command), cancellationToken);
        }
    }
}
