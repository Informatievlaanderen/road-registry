namespace RoadRegistry.Sync.MunicipalityRegistry;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.MunicipalityRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Models;
using NetTopologySuite.IO;
using Municipality = Models.Municipality;

public class MunicipalityEventProjection : ConnectedProjection<MunicipalityEventConsumerContext>
{
    private readonly WKBReader _wkbReader = new WKBReader
    {
        HandleSRID = true,
        IsStrict = false
    };

    public MunicipalityEventProjection()
    {
        When<MunicipalityWasRegistered>(Handle);
        When<MunicipalityWasDrawn>(Handle);
        When<MunicipalityGeometryWasCorrected>(Handle);
        When<MunicipalityNisCodeWasCorrected>(Handle);
        When<MunicipalityBecameCurrent>(Handle);
        When<MunicipalityWasCorrectedToCurrent>(Handle);
        When<MunicipalityWasRetired>(Handle);
        When<MunicipalityWasCorrectedToRetired>(Handle);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityWasRegistered @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.NisCode = @event.NisCode;
            municipality.Status = MunicipalityStatus.Proposed;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityGeometryWasCorrected @event, CancellationToken cancellationToken)
    {
        var geometry = _wkbReader.Read(Convert.FromHexString(@event.ExtendedWkbGeometry));

        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Geometry = geometry;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityNisCodeWasCorrected @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.NisCode = @event.NisCode;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityWasDrawn @event, CancellationToken cancellationToken)
    {
        var geometry = _wkbReader.Read(Convert.FromHexString(@event.ExtendedWkbGeometry));

        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Geometry = geometry;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityBecameCurrent @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Status = MunicipalityStatus.Current;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityWasCorrectedToCurrent @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Status = MunicipalityStatus.Current;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityWasRetired @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Status = MunicipalityStatus.Retired;
        }, cancellationToken);
    }

    private async Task Handle(MunicipalityEventConsumerContext context, MunicipalityWasCorrectedToRetired @event, CancellationToken cancellationToken)
    {
        await Update(context, @event.MunicipalityId, municipality =>
        {
            municipality.Status = MunicipalityStatus.Retired;
        }, cancellationToken);
    }

    private async Task Update(MunicipalityEventConsumerContext context, string municipalityId, Action<Municipality> configure, CancellationToken cancellationToken)
    {
        var municipality = await context.Municipalities.FindAsync([municipalityId], cancellationToken);
        if (municipality is null)
        {
            municipality = new Municipality
            {
                MunicipalityId = municipalityId
            };
            context.Municipalities.Add(municipality);
        }

        configure(municipality);
    }
}
