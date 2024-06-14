namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Editor.Schema;
using Editor.Schema.Extensions;
using Exceptions;
using Hosts;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using TicketingService.Abstractions;
using AddRoadSegmentToEuropeanRoad = BackOffice.Uploads.AddRoadSegmentToEuropeanRoad;
using AddRoadSegmentToNationalRoad = BackOffice.Uploads.AddRoadSegmentToNationalRoad;
using AddRoadSegmentToNumberedRoad = BackOffice.Uploads.AddRoadSegmentToNumberedRoad;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;
using RemoveRoadSegmentFromEuropeanRoad = BackOffice.Uploads.RemoveRoadSegmentFromEuropeanRoad;
using RemoveRoadSegmentFromNationalRoad = BackOffice.Uploads.RemoveRoadSegmentFromNationalRoad;
using RemoveRoadSegmentFromNumberedRoad = BackOffice.Uploads.RemoveRoadSegmentFromNumberedRoad;

public sealed class ChangeRoadSegmentAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentAttributesSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly IOrganizationCache _organizationCache;

    public ChangeRoadSegmentAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        IOrganizationCache organizationCache,
        ILogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _editorContext = editorContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _organizationCache = organizationCache;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Attributen wijzigen", async translatedChanges =>
        {
            var problems = Problems.None;

            foreach (var change in request.Request.ChangeRequests)
            {
                var roadSegmentId = new RoadSegmentId(change.Id);

                var editorRoadSegment = await _editorContext.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == change.Id, cancellationToken);
                if (editorRoadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                var roadSegmentDbaseRecord = new RoadSegmentDbaseRecord().FromBytes(editorRoadSegment.DbaseRecord, _manager, _fileEncoding);
                var geometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord.METHODE.Value];

                var networkRoadSegment = await RoadRegistryContext.RoadNetworks.FindRoadSegment(roadSegmentId, geometryDrawMethod, cancellationToken);
                if (networkRoadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                (translatedChanges, problems) = await AppendChange(translatedChanges, problems, networkRoadSegment, change, cancellationToken);
            }

            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            return translatedChanges;
        }, cancellationToken);

        return new ChangeRoadSegmentAttributesResponse();
    }

    private async Task<(TranslatedChanges, Problems)> AppendChange(TranslatedChanges changes, Problems problems, RoadSegment roadSegment, ChangeRoadSegmentAttributeRequest change, CancellationToken cancellationToken)
    {
        var maintenanceAuthority = change.MaintenanceAuthority;
        if (maintenanceAuthority is not null)
        {
            (maintenanceAuthority, var maintenanceAuthorityProblems) = await FindOrganizationId(maintenanceAuthority.Value, cancellationToken);
            problems += maintenanceAuthorityProblems;
        }

        if (new object?[] { maintenanceAuthority, change.Morphology, change.Status, change.Category, change.AccessRestriction }.Any(x => x is not null))
        {
            changes = changes.AppendChange(new ModifyRoadSegmentAttributes(RecordNumber.Initial, roadSegment.Id, roadSegment.AttributeHash.GeometryDrawMethod)
            {
                MaintenanceAuthority = maintenanceAuthority,
                Morphology = change.Morphology,
                Status = change.Status,
                Category = change.Category,
                AccessRestriction = change.AccessRestriction
            });
        }
        
        changes = AppendChange(changes, roadSegment, change.EuropeanRoads);
        changes = AppendChange(changes, roadSegment, change.NationalRoads);
        changes = AppendChange(changes, roadSegment, change.NumberedRoads);

        return (changes, problems);
    }

    private TranslatedChanges AppendChange(TranslatedChanges changes, RoadSegment roadSegment, ICollection<EuropeanRoadNumber>? europeanRoadNumbers)
    {
        if (europeanRoadNumbers is null)
        {
            return changes;
        }

        var recordNumberProvider = new NextRecordNumberProvider(RecordNumber.Initial);
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        var existingRoads = roadSegment.EuropeanRoadAttributes.Values.ToList();
        foreach (var europeanRoadNumber in europeanRoadNumbers)
        {
            var recordNumber = recordNumberProvider.Next();

            var matches = existingRoads
                .Where(x => x.Number == europeanRoadNumber)
                .ToList();
            foreach (var match in matches)
            {
                existingRoads.Remove(match);

                changes = changes.AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                    recordNumber,
                    match.AttributeId,
                    roadSegment.AttributeHash.GeometryDrawMethod,
                    roadSegment.Id,
                    match.Number
                ));
            }

            changes = changes.AppendChange(new AddRoadSegmentToEuropeanRoad(
                recordNumber,
                attributeIdProvider.Next(),
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.Id,
                europeanRoadNumber
            ));
        }

        foreach (var europeanRoad in existingRoads)
        {
            changes = changes.AppendChange(new RemoveRoadSegmentFromEuropeanRoad(
                recordNumberProvider.Next(),
                europeanRoad.AttributeId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.Id,
                europeanRoad.Number
            ));
        }

        return changes;
    }

    private TranslatedChanges AppendChange(TranslatedChanges changes, RoadSegment roadSegment, ICollection<NationalRoadNumber>? nationalRoadNumbers)
    {
        if (nationalRoadNumbers is null)
        {
            return changes;
        }

        var recordNumberProvider = new NextRecordNumberProvider(RecordNumber.Initial);
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        var existingRoads = roadSegment.NationalRoadAttributes.Values.ToList();
        foreach (var nationalRoadNumber in nationalRoadNumbers)
        {
            var recordNumber = recordNumberProvider.Next();

            var matches = existingRoads
                .Where(x => x.Number == nationalRoadNumber)
                .ToList();
            foreach (var match in matches)
            {
                existingRoads.Remove(match);

                changes = changes.AppendChange(new RemoveRoadSegmentFromNationalRoad(
                    recordNumber,
                    match.AttributeId,
                    roadSegment.AttributeHash.GeometryDrawMethod,
                    roadSegment.Id,
                    match.Number
                ));
            }

            changes = changes.AppendChange(new AddRoadSegmentToNationalRoad(
                recordNumber,
                attributeIdProvider.Next(),
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.Id,
                nationalRoadNumber
            ));
        }

        foreach (var nationalRoad in existingRoads)
        {
            changes = changes.AppendChange(new RemoveRoadSegmentFromNationalRoad(
                recordNumberProvider.Next(),
                nationalRoad.AttributeId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.Id,
                nationalRoad.Number
            ));
        }

        return changes;
    }

    private TranslatedChanges AppendChange(TranslatedChanges changes, RoadSegment roadSegment, ICollection<ChangeRoadSegmentNumberedRoadAttribute>? numberedRoads)
    {
        if (numberedRoads is null)
        {
            return changes;
        }

        var recordNumberProvider = new NextRecordNumberProvider(RecordNumber.Initial);
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        var existingRoads = roadSegment.NumberedRoadAttributes.Values.ToList();
        foreach (var numberedRoad in numberedRoads)
        {
            var recordNumber = recordNumberProvider.Next();

            var matches = existingRoads
                .Where(x => x.Number == numberedRoad.Number)
                .Select(match => new
                {
                    Attribute = match,
                    IsExactMatch = numberedRoad.Ordinal == match.Ordinal && numberedRoad.Direction == match.Direction
                })
                .ToList();

            foreach (var match in matches)
            {
                existingRoads.Remove(match.Attribute);

                if (!match.IsExactMatch)
                {
                    changes = changes.AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                        recordNumber,
                        match.Attribute.AttributeId,
                        roadSegment.AttributeHash.GeometryDrawMethod,
                        roadSegment.Id,
                        match.Attribute.Number
                    ));
                }
            }

            if (!matches.Any(x => x.IsExactMatch))
            {
                changes = changes.AppendChange(new AddRoadSegmentToNumberedRoad(
                    recordNumber,
                    attributeIdProvider.Next(),
                    roadSegment.AttributeHash.GeometryDrawMethod,
                    roadSegment.Id,
                    numberedRoad.Number,
                    numberedRoad.Direction,
                    numberedRoad.Ordinal
                ));
            }
        }

        foreach (var numberedRoad in existingRoads)
        {
            changes = changes.AppendChange(new RemoveRoadSegmentFromNumberedRoad(
                recordNumberProvider.Next(),
                numberedRoad.AttributeId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.Id,
                numberedRoad.Number
            ));
        }

        return changes;
    }

    private async Task<(OrganizationId, Problems)> FindOrganizationId(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var problems = Problems.None;

        var maintenanceAuthorityOrganization = await _organizationCache.FindByIdOrOvoCodeAsync(organizationId, cancellationToken);
        if (maintenanceAuthorityOrganization is not null)
        {
            return (maintenanceAuthorityOrganization.Code, problems);
        }

        if (OrganizationOvoCode.AcceptsValue(organizationId))
        {
            problems = problems.Add(new MaintenanceAuthorityNotKnown(organizationId));
        }

        return (organizationId, problems);
    }
}
