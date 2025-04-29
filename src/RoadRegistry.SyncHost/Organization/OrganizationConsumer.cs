namespace RoadRegistry.SyncHost.Organization;

using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Sync.OrganizationRegistry;
using RoadRegistry.Sync.OrganizationRegistry.Exceptions;
using RoadRegistry.Sync.OrganizationRegistry.Extensions;
using SqlStreamStore;
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
    private readonly IOrganizationCommandQueue _organizationCommandQueue;
    private readonly IStreamStore _store;

    public OrganizationConsumer(
        ILifetimeScope container,
        OrganizationConsumerOptions options,
        IOrganizationReader organizationReader,
        IStreamStore store,
        IOrganizationCommandQueue organizationCommandQueue,
        ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<OrganizationConsumer>())
    {
        _container = container.ThrowIfNull();
        _options = options.ThrowIfNull();
        _organizationReader = organizationReader.ThrowIfNull();
        _store = store.ThrowIfNull();
        _organizationCommandQueue = organizationCommandQueue.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        var consecutiveExceptionsCount = 0;
        var serviceIsUnavailable = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = _container.BeginLifetimeScope();
            await using var dbContext = scope.Resolve<OrganizationConsumerContext>();
            await using var editorContext = scope.Resolve<EditorContext>();

            var projectionState = await dbContext.InitializeProjectionState(ProjectionStateName, cancellationToken);
            projectionState.ErrorMessage = null;

            try
            {
                var organizationsContext = new Lazy<Organizations>(() =>
                {
                    var map = _container.Resolve<EventSourcedEntityMap>();
                    return new Organizations(map, _store, SerializerSettings, EventMapping);
                });

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

                    if (!_options.DisableWaitForEditorContextProjection)
                    {
                        await editorContext.WaitForProjectionToBeAtStoreHeadPosition(_store, WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost, Logger, cancellationToken);
                    }

                    await CreateOrUpdateOrganization(editorContext, organizationsContext.Value, organization, cancellationToken);

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
            catch (OrganizationRegistryTemporarilyUnavailableException)
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

    private async Task CreateOrUpdateOrganization(
        EditorContext editorContext,
        Organizations organizationsContext,
        Organization organization,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingOrganizations = await editorContext.OrganizationsV2
            .Where(x => x.OvoCode == organization.OvoNumber)
            .ToListAsync(cancellationToken);

        var classicOrganizationIds = existingOrganizations
            .Where(x => x.Code != x.OvoCode)
            .Select(x => x.Code)
            .Distinct()
            .ToList();
        if (classicOrganizationIds.Count > 1)
        {
            Logger.LogError($"Multiple Organizations found with a link to {organization.OvoNumber}, not proceeding with automatic rename to '{organization.Name}' (Ids: {string.Join(", ", classicOrganizationIds)})");
            return;
        }

        var ovoCode = new OrganizationOvoCode(organization.OvoNumber);
        var kboNumber = OrganizationKboNumber.FromValue(organization.KboNumber);

        var organizationId = classicOrganizationIds.Any() ? new OrganizationId(classicOrganizationIds.Single()) : new OrganizationId(ovoCode);

        var existingOrganization = await organizationsContext.FindAsync(organizationId, cancellationToken);
        if (existingOrganization is null)
        {
            Logger.LogInformation("Organization {OvoNumber}: creating new", organization.OvoNumber);
            var command = new CreateOrganization
            {
                Code = organizationId,
                Name = organization.Name,
                OvoCode = ovoCode,
                KboNumber = kboNumber
            };
            await _organizationCommandQueue.WriteAsync(new Command(command), cancellationToken);
        }
        else
        {
            Logger.LogInformation("Organization {OvoNumber}: updating", organization.OvoNumber);
            var command = new ChangeOrganization
            {
                Code = organizationId,
                Name = organization.Name,
                OvoCode = ovoCode,
                KboNumber = kboNumber
            };
            await _organizationCommandQueue.WriteAsync(new Command(command), cancellationToken);
        }
    }
}
