namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using BackOffice;
using BackOffice.Core;
using BackOffice.Extracts;
using BackOffice.Messages;
using NetTopologySuite.Geometries;
using RoadNetwork;
using TicketingService.Abstractions;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly RoadNetworkChangesFactory _roadNetworkChangeFactory;
    private readonly IExtractRequests _extractRequests;
    private readonly ITicketing _ticketing;

    public ChangeRoadNetworkCommandHandler(
        IRoadNetworkRepository roadNetworkRepository,
        RoadNetworkChangesFactory roadNetworkChangeFactory,
        IExtractRequests extractRequests,
        ITicketing ticketing)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkChangeFactory = roadNetworkChangeFactory;
        _extractRequests = extractRequests;
        _ticketing = ticketing;
    }

    public async Task Handle(ChangeRoadNetworkCommand command, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(command.DownloadId);

        await _ticketing.Pending(command.TicketId, cancellationToken); //TODO-pr op het einde bekijken, is dit nog nodig door lambda?

        var roadNetworkChanges = await _roadNetworkChangeFactory.Build(command);

        var roadNetwork = await _roadNetworkRepository.Load(roadNetworkChanges, cancellationToken);
        var changeResult = roadNetwork.Change(roadNetworkChanges);

        var hasError = changeResult.ChangeProblems.Select(x => x.Problems).Any(x => x.HasError());
        if (hasError)
        {
            //TODO-pr set ticket to error
            var errors = changeResult.ChangeProblems
                .SelectMany(x => x.Problems)
                .Select(problem => problem.Translate().ToTicketError())
                .ToArray();

            await _ticketing.Error(command.TicketId, new TicketError(errors), cancellationToken);
            await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);
        }
        else
        {
            await _roadNetworkRepository.Save(roadNetwork, cancellationToken);
            await _ticketing.Complete(command.TicketId, new TicketResult(), cancellationToken);
            //TODO-pr aparte projectie voorzien voor resultaat dat extract details kan opvragen
            await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);
        }
    }
}
