namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class SqsLambdaExtensions
{
    public static IServiceCollection AddSqsLambdaHandlerOptions(this IServiceCollection services)
    {
        return services
            .AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var options = new SqsLambdaHandlerOptions();
                configuration.Bind(options);

                return options;
            });
    }
}
