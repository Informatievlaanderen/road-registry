namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using Amazon.Lambda;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Model;

internal class LambdaHealthCheck : IHealthCheck
{
    private readonly LambdaHealthCheckOptions _options;

    public LambdaHealthCheck(LambdaHealthCheckOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            var nextMarker = "";
            var functions = new List<FunctionConfiguration>();

            do
            {
                var listFunctionsRequest = new ListFunctionsRequest { Marker = nextMarker };
                var listFunctionResponse = await _options.LambdaClient.ListFunctionsAsync(listFunctionsRequest, cancellationToken);
                var roadFunctions = listFunctionResponse.Functions
                    .Where(f => f.FunctionName.Contains("basisregisters-rr"));
                if (roadFunctions is not null && roadFunctions.Any()) functions.AddRange(roadFunctions);
                nextMarker = listFunctionResponse.NextMarker;
            } while (nextMarker is not null);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
