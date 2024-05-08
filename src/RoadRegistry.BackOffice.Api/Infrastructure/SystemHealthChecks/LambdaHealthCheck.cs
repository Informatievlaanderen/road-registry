namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    internal class LambdaHealthCheck: ISystemHealthCheck
    {
        public LambdaHealthCheck()
        {
            
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            //TODO-rik
            /*Start een dummy lambda request om te zien of de lambda wordt opgestart en hij kan:               
               Aan de DB               
               Aan de ticketing service               
               Aan de S3 sqs-messages*/

            return HealthCheckResult.Healthy();
        }
    }
}
