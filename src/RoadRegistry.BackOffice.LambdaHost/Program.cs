namespace RoadRegistry.BackOffice.LambdaHost;

using System;
using System.Threading.Tasks;
using Autofac;
using Handlers.Sqs;
using Hosts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var roadRegistryHost = new RoadRegistryHostBuilder<Program>(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<LambdaRunner>();
            })
            .ConfigureContainer((context, builder) =>
            {
                builder.RegisterModule(new SqsHandlersModule());
            })
            .ConfigureRunCommand(async (sp, stoppingToken) =>
            {
                var lambdaRunner = sp.GetRequiredService<LambdaRunner>();

                await Task.WhenAll(
                    lambdaRunner.ExecuteAsync(stoppingToken)
                );
            })
            .Build();

        await roadRegistryHost.RunAsync((sp, host, ct) =>
        {
            var hostEnvirionment = sp.GetRequiredService<IHostEnvironment>();

            if (!hostEnvirionment.IsDevelopment())
            {
                throw new InvalidOperationException($"Host is for development only, current environment: {hostEnvirionment.EnvironmentName}");
            }

            return Task.CompletedTask;
        });
    }
}
