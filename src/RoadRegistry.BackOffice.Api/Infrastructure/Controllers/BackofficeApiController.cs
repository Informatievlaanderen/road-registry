namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NodaTime;

public abstract class BackofficeApiController : ApiController
{
    protected Provenance CreateFakeProvenance()
    {
        return new Provenance(
            SystemClock.Instance.GetCurrentInstant(),
            Application.RoadRegistry,
            new Reason(""), // TODO: TBD
            new Operator(""), // TODO: from claims
            Modification.Insert,
            Organisation.Agiv // TODO: from claims
        );
    }

    protected IDictionary<string, object> GetMetadata()
    {
        var userId = User.FindFirst("urn:be:vlaanderen:roadregistry:acmid")?.Value;
        var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

        return new Dictionary<string, object>
        {
            { "UserId", userId },
            { "CorrelationId", correlationId }
        };
    }
}
