namespace RoadRegistry.Sync.MunicipalityRegistry;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

public class MunicipalityEventProjection : ConnectedProjection<MunicipalityEventConsumerContext>
{
    private readonly IMunicipalities _municipalities;

    public MunicipalityEventProjection(IMunicipalities municipalities)
    {
        _municipalities = municipalities.ThrowIfNull();

        When<MunicipalityWasRegistered>(MunicipalityWasRegistered);
    }

    private async Task MunicipalityWasRegistered(MunicipalityEventConsumerContext context, MunicipalityWasRegistered envelope, CancellationToken token)
    {
        //TODO-rik implement muni events

        // var municipality = await _municipalities.FindAsync(nisCode, cancellationToken);
        // if (municipality is null)
        // {
        //
        // }
        // else
        // {
        //
        // }
    }
}
