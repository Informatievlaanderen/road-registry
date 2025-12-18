namespace RoadRegistry.BackOffice.Uploads;

using Framework;
using Messages;
using System;
using System.Linq;
using RoadRegistry.Extracts.Uploads;

public class RoadNetworkChangesArchive : EventSourcedEntity
{
    public static readonly Func<RoadNetworkChangesArchive> Factory = () => new RoadNetworkChangesArchive();

    private RoadNetworkChangesArchive()
    {
        On<RoadNetworkChangesArchiveUploaded>(e =>
        {
            Id = new ArchiveId(e.ArchiveId);
            Description = !string.IsNullOrEmpty(e.Description)
                ? new ExtractDescription(e.Description)
                : new ExtractDescription();
        });
    }

    public ArchiveId Id { get; private set; }
    public ExtractDescription Description { get; private set; }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, ExtractDescription extractDescription, DownloadId downloadId, TicketId? ticketId)
    {
        var instance = new RoadNetworkChangesArchive();

        instance.Apply(new RoadNetworkChangesArchiveUploaded
        {
            ArchiveId = id,
            Description = extractDescription,
            DownloadId = downloadId,
            TicketId = ticketId
        });

        return instance;
    }

    public void AcceptOrReject(ZipArchiveProblems problems, ExtractRequestId extractRequestId, DownloadId downloadId, TicketId? ticketId)
    {
        if (!problems.HasError())
        {
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    ExtractRequestId = extractRequestId,
                    DownloadId = downloadId,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                });
        }
        else
        {
            Apply(
                new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                });
        }
    }
}
