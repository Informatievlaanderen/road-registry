namespace RoadRegistry.BackOffice.Api.Infrastructure.Behaviors
{
    using System;
    using System.Collections.Generic;
    using Abstractions;
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Microsoft.AspNetCore.Http;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public class IdentityPipelineBehavior<TRequest,TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityPipelineBehavior(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            switch (request)
            {
                case EndpointRequest endpointRequest:
                    //TODO-rik confirm: als er extract wordt aangevraagd, zit die ProvenanceData al in de DB bij het command? zou moeten
                    endpointRequest.Metadata = GetMetadata();
                    endpointRequest.ProvenanceData = CreateProvenanceData();
                    break;
                case SqsRequest sqsRequest:
                    sqsRequest.Metadata = GetMetadata();
                    sqsRequest.ProvenanceData = CreateProvenanceData();
                    break;
            }

            return await next();
        }

        private ProvenanceData CreateProvenanceData()
        {
            //TODO-pr read safe api-key
            return new RoadRegistryProvenanceData(Modification.Insert, _httpContextAccessor.HttpContext.User.FindFirst(AcmIdmClaimTypes.VoOrgCode)?.Value);
        }

        private IDictionary<string, object> GetMetadata()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst("urn:be:vlaanderen:roadregistry:acmid")?.Value;
            var correlationId = _httpContextAccessor.HttpContext.User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId ?? Guid.NewGuid().ToString() }
            };
        }

    }
}
