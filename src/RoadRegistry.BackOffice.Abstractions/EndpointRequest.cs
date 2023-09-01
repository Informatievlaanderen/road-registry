namespace RoadRegistry.BackOffice.Abstractions;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using MediatR;

public abstract record EndpointRequest
{
    public IDictionary<string, object> Metadata { get; set; }
    public ProvenanceData ProvenanceData { get; set; }
}

public abstract record EndpointRequest<TResponse> : EndpointRequest, IRequest<TResponse> where TResponse : EndpointResponse
{
}
