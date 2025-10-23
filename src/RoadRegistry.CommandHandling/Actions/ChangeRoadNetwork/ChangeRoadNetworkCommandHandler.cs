namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using BackOffice;
using BackOffice.Core;
using BackOffice.Extracts;
using BackOffice.Messages;
using RoadNetwork;
using TicketingService.Abstractions;

public class ChangeRoadNetworkCommandHandler
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly RoadNetworkChangesFactory _roadNetworkChangeFactory;
    private readonly IExtractRequests _extractRequests;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ITicketing _ticketing;

    public ChangeRoadNetworkCommandHandler(
        IRoadNetworkRepository roadNetworkRepository,
        RoadNetworkChangesFactory roadNetworkChangeFactory,
        IExtractRequests extractRequests,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ITicketing ticketing)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkChangeFactory = roadNetworkChangeFactory;
        _extractRequests = extractRequests;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _ticketing = ticketing;
    }

    public async Task Handle(ChangeRoadNetworkCommand command, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(command.DownloadId);

        await _ticketing.Pending(command.TicketId, cancellationToken); //TODO-pr op het einde bekijken, is dit nog nodig door lambda?

        var roadNetworkChanges = await _roadNetworkChangeFactory.Build(command);

        var roadNetwork = await _roadNetworkRepository.Load(roadNetworkChanges, cancellationToken);
        var changeResult = roadNetwork.Change(roadNetworkChanges, _roadNetworkIdGenerator);

        var hasError = changeResult.Problems.HasError();
        if (hasError)
        {
            var errors = changeResult.Problems
                .Select(problem => problem.Translate().ToTicketError())
                .ToArray();

            await _ticketing.Error(command.TicketId, new TicketError(errors), cancellationToken);
            await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);
        }
        else
        {
            await _roadNetworkRepository.Save(roadNetwork, cancellationToken);
            //TODO-pr ook resultaat van changes meegeven, bvb welke IDs zijn aangemaakt/gewijzigd/verwijderd, per entiteit
            //is al zeker nodig voor E2E testen
            await _ticketing.Complete(command.TicketId, new TicketResult(), cancellationToken);
            //TODO-pr aparte projectie voorzien voor resultaat dat extract details kan opvragen
            await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);
        }
    }
}
