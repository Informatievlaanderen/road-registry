namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Editor.Schema;
using Exceptions;
using Hosts;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using StreetName;
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
    private readonly IOrganizationCache _organizationCache;
    private readonly IStreetNameClient _streetNameClient;

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
        IStreetNameClient streetNameClient,
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
        _organizationCache = organizationCache;
        _streetNameClient = streetNameClient;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Attributen wijzigen", async translatedChanges =>
        {
            var problems = Problems.None;

            foreach (var change in request.Request.ChangeRequests)
            {
                var roadSegmentId = change.Id;

                var editorRoadSegment = await _editorContext.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == roadSegmentId, cancellationToken);
                if (editorRoadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                var geometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[editorRoadSegment.MethodId];

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

    private async Task<(TranslatedChanges, Problems)> AppendChange(
        TranslatedChanges changes,
        Problems problems,
        RoadSegment roadSegment,
        ChangeRoadSegmentAttributeRequest change,
        CancellationToken cancellationToken)
    {
        (changes, problems) = await AppendAttributeChanges(changes, problems, roadSegment, change, cancellationToken);

        changes = AppendEuropeanRoadChanges(changes, roadSegment, change.EuropeanRoads);
        changes = AppendNationalRoadChanges(changes, roadSegment, change.NationalRoads);
        changes = AppendNumberedRoadChanges(changes, roadSegment, change.NumberedRoads);

        return (changes, problems);
    }

    private async Task<(TranslatedChanges changes, Problems problems)> AppendAttributeChanges(
        TranslatedChanges changes,
        Problems problems,
        RoadSegment roadSegment,
        ChangeRoadSegmentAttributeRequest change,
        CancellationToken cancellationToken)
    {
        var maintenanceAuthority = change.MaintenanceAuthority;
        if (maintenanceAuthority is not null)
        {
            (maintenanceAuthority, var maintenanceAuthorityProblems) = await FindOrganizationId(maintenanceAuthority.Value, cancellationToken);
            problems += maintenanceAuthorityProblems;
        }

        if (new object?[]
            {
                maintenanceAuthority, change.Morphology, change.Status, change.Category, change.AccessRestriction,
                change.LeftSideStreetNameId, change.RightSideStreetNameId,
            }.All(x => x is null))
        {
            return (changes, problems);
        }

        if (change.Category is not null
            && roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Measured
            && RoadSegmentCategory.IsUpgraded(roadSegment.AttributeHash.Category))
        {
            var allowedCategories = RoadSegmentCategory.All.Except(RoadSegmentCategory.Obsolete).ToArray();
            if (!allowedCategories.Contains(change.Category))
            {
                problems += new RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(roadSegment.Id);
            }
        }

        if (change.LeftSideStreetNameId is not null)
        {
            problems = await ValidateStreetName(change.Id, change.LeftSideStreetNameId.Value, problems, cancellationToken);
        }

        if (change.RightSideStreetNameId is not null)
        {
            problems = await ValidateStreetName(change.Id, change.RightSideStreetNameId.Value, problems, cancellationToken);
        }

        changes = changes.AppendChange(new ModifyRoadSegmentAttributes(RecordNumber.Initial, roadSegment.Id, roadSegment.AttributeHash.GeometryDrawMethod)
        {
            MaintenanceAuthority = maintenanceAuthority,
            Morphology = change.Morphology,
            Status = change.Status,
            Category = change.Category,
            AccessRestriction = change.AccessRestriction,
            LeftSideStreetNameId = change.LeftSideStreetNameId,
            RightSideStreetNameId = change.RightSideStreetNameId
        });

        return (changes, problems);
    }

    private TranslatedChanges AppendEuropeanRoadChanges(TranslatedChanges changes, RoadSegment roadSegment, ICollection<EuropeanRoadNumber>? europeanRoadNumbers)
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

    private static TranslatedChanges AppendNationalRoadChanges(TranslatedChanges changes, RoadSegment roadSegment, ICollection<NationalRoadNumber>? nationalRoadNumbers)
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

    private TranslatedChanges AppendNumberedRoadChanges(TranslatedChanges changes, RoadSegment roadSegment, ICollection<ChangeRoadSegmentNumberedRoadAttribute>? numberedRoads)
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

        var maintenanceAuthorityOrganization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, cancellationToken);
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

    private async Task<Problems> ValidateStreetName(
        RoadSegmentId roadSegmentId,
        StreetNameLocalId streetNameId,
        Problems problems,
        CancellationToken cancellationToken)
    {
        try
        {
            var streetName = await _streetNameClient.GetAsync(streetNameId, cancellationToken);
            if (streetName is null)
            {
                return problems.Add(new StreetNameNotFound(roadSegmentId));
            }

            string[] activeStreetNameStatus = [StreetNameStatus.Current, StreetNameStatus.Proposed];
            if (activeStreetNameStatus.All(status => !string.Equals(streetName.Status, status, StringComparison.InvariantCultureIgnoreCase)))
            {
                return problems.Add(new RoadSegmentStreetNameNotProposedOrCurrent(roadSegmentId));
            }
        }
        catch (StreetNameRegistryUnexpectedStatusCodeException ex)
        {
            Logger.LogError(ex.Message);

            problems += new StreetNameRegistryUnexpectedError((int)ex.StatusCode);
        }

        return problems;
    }
}
