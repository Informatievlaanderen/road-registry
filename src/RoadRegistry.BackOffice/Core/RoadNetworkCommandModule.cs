namespace RoadRegistry.BackOffice.Core;

using Autofac;
using FeatureToggles;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RoadNetworkCommandModule : CommandHandlerModule
{
    private readonly IStreamStore _store;
    private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
    private readonly IExtractUploadFailedEmailClient _emailClient;
    private readonly ILogger _logger;
    private readonly EventEnricher _enricher;

    public RoadNetworkCommandModule(
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IClock clock,
        UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
        IExtractUploadFailedEmailClient emailClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _store = store;
        _useOvoCodeInChangeRoadNetworkFeatureToggle = useOvoCodeInChangeRoadNetworkFeatureToggle;
        _emailClient = emailClient;
        _logger = loggerFactory.CreateLogger<RoadNetworkCommandModule>();
        _enricher = EnrichEvent.WithTime(clock);
        
        For<ChangeRoadNetwork>()
            .UseValidator(new ChangeRoadNetworkValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, _enricher)
            .Handle(ChangeRoadNetwork);

        For<CreateOrganization>()
            .UseValidator(new CreateOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, _enricher)
            .Handle(CreateOrganization);

        For<CreateOrganizationRejected>()
            .Handle(Ignore);

        For<DeleteOrganization>()
            .UseValidator(new DeleteOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, _enricher)
            .Handle(DeleteOrganization);

        For<DeleteOrganizationRejected>()
            .Handle(Ignore);

        For<RenameOrganization>()
            .UseValidator(new RenameOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, _enricher)
            .Handle(RenameOrganization);

        For<RenameOrganizationRejected>()
            .Handle(Ignore);

        For<ChangeOrganization>()
            .UseValidator(new ChangeOrganizationValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, _enricher)
            .Handle(ChangeOrganization);

        For<ChangeOrganizationRejected>()
            .Handle(Ignore);
    }

    private async Task ChangeRoadNetwork(IRoadRegistryContext context, Command<ChangeRoadNetwork> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var request = ChangeRequestId.FromString(command.Body.RequestId);
        DownloadId? downloadId = command.Body.DownloadId is not null ? new DownloadId(command.Body.DownloadId.Value) : null;
        var @operator = new OperatorName(command.Body.Operator);
        var reason = new Reason(command.Body.Reason);

        var organizationId = new OrganizationId(command.Body.OrganizationId);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        Organization.DutchTranslation translation;
        if (organization is null)
        {
            translation = organizationId == OrganizationId.Other
                ? Organization.PredefinedTranslations.Other
                : Organization.PredefinedTranslations.Unknown;
        } else if (_useOvoCodeInChangeRoadNetworkFeatureToggle.FeatureEnabled && organization.OvoCode is not null)
        {
            translation = new Organization.DutchTranslation(new OrganizationId(organization.OvoCode.Value), organization.Translation.Name);
        }
        else
        {
            translation = organization.Translation;
        }

        var network = await context.RoadNetworks.Get(cancellationToken);
        var translator = new RequestedChangeTranslator(
            network.ProvidesNextTransactionId(),
            network.ProvidesNextRoadNodeId(),
            network.ProvidesNextRoadNodeVersion(),
            network.ProvidesNextRoadSegmentId(),
            network.ProvidesNextGradeSeparatedJunctionId(),
            network.ProvidesNextEuropeanRoadAttributeId(),
            network.ProvidesNextNationalRoadAttributeId(),
            network.ProvidesNextNumberedRoadAttributeId(),
            network.ProvidesNextRoadSegmentVersion(),
            network.ProvidesNextRoadSegmentGeometryVersion(),
            network.ProvidesNextRoadSegmentLaneAttributeId(),
            network.ProvidesNextRoadSegmentWidthAttributeId(),
            network.ProvidesNextRoadSegmentSurfaceAttributeId()
        );
        var requestedChanges = await translator.Translate(command.Body.Changes, context.Organizations, cancellationToken);

        await network.Change(request, downloadId, reason, @operator, translation, requestedChanges, _emailClient, cancellationToken);

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task CreateOrganization(IRoadRegistryContext context, Command<CreateOrganization> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            var rejectedCommand = new CreateOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            };
            _enricher(rejectedCommand);

            await new RoadNetworkCommandQueue(_store, commandMetadata)
                .Write(new Command(rejectedCommand), cancellationToken);
        }
        else
        {
            var acceptedCommand = new CreateOrganizationAccepted
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            };
            _enricher(acceptedCommand);

            await new OrganizationCommandQueue(_store)
                .Write(organizationId, new Command(acceptedCommand), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task DeleteOrganization(IRoadRegistryContext context, Command<DeleteOrganization> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
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
            var rejectedCommand = new DeleteOrganizationRejected
            {
                Code = command.Body.Code
            };
            _enricher(rejectedCommand);

            await new RoadNetworkCommandQueue(_store, commandMetadata)
                .Write(new Command(rejectedCommand), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task RenameOrganization(IRoadRegistryContext context, Command<RenameOrganization> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
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
            var rejectedCommand = new RenameOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name
            };
            _enricher(rejectedCommand);

            await new RoadNetworkCommandQueue(_store, commandMetadata)
                .Write(new Command(rejectedCommand), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private async Task ChangeOrganization(IRoadRegistryContext context, Command<ChangeOrganization> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command handler started for {CommandName}", command.Body.GetType().Name);

        var organizationId = new OrganizationId(command.Body.Code);
        var organization = await context.Organizations.FindAsync(organizationId, cancellationToken);

        if (organization != null)
        {
            organization.Change(
                command.Body.Name is not null ? new OrganizationName(command.Body.Name) : null,
                OrganizationOvoCode.FromValue(command.Body.OvoCode)
            );
        }
        else
        {
            var rejectedCommand = new ChangeOrganizationRejected
            {
                Code = command.Body.Code,
                Name = command.Body.Name,
                OvoCode = command.Body.OvoCode
            };
            _enricher(rejectedCommand);

            await new RoadNetworkCommandQueue(_store, commandMetadata)
                .Write(new Command(rejectedCommand), cancellationToken);
        }

        _logger.LogInformation("Command handler finished for {Command}", command.Body.GetType().Name);
    }

    private Task Ignore<TCommand>(Command<TCommand> command, ApplicationMetadata commandMetadata, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
