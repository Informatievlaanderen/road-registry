namespace RoadRegistry.BackOffice.Api.Infrastructure.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class IdentityPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityPipelineBehavior(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext is not null)
            {
                switch (request)
                {
                    case EndpointRequest endpointRequest:
                        endpointRequest.Metadata = GetMetadata();
                        endpointRequest.ProvenanceData = CreateProvenanceData();
                        break;
                    case SqsRequest sqsRequest:
                        sqsRequest.Metadata = GetMetadata();
                        sqsRequest.ProvenanceData = CreateProvenanceData();
                        break;
                }
            }

            return await next();
        }

        private ProvenanceData CreateProvenanceData()
        {
            return new RoadRegistryProvenanceData(Modification.Unknown, _httpContextAccessor.HttpContext!.GetOperator());
        }

        private IDictionary<string, object> GetMetadata()
        {
            var userId = _httpContextAccessor.HttpContext!.User.FindFirst("urn:be:vlaanderen:roadregistry:acmid")?.Value;
            var correlationId = _httpContextAccessor.HttpContext.User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId ?? Guid.NewGuid().ToString() }
            };
        }
    }
}
