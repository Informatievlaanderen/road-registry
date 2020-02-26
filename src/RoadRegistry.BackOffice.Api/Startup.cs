namespace RoadRegistry.Api
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc;
    using Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Logging;
    using Serilog;

    public class Startup
    {
        private const string AllowSpecificOrigin = "AllowSpecificOrigin";

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime appLifetime,
            ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
                app
                    .UseDeveloperExceptionPage()
                    .UseDatabaseErrorPage()
                    .UseBrowserLink();

            app
                .UseCors(policyName: AllowSpecificOrigin)

                .UseApiExceptionHandler(loggerFactory, AllowSpecificOrigin)

                //.UseIdempotencyDatabaseMigrations()

                .UseMiddleware<EnableRequestRewindMiddleware>()
                .UseMiddleware<AddCorrelationIdToLogContextMiddleware>()
                .UseMiddleware<AddCorrelationIdToResponseMiddleware>()
                .UseMiddleware<AddCorrelationIdMiddleware>()
                .UseMiddleware<AddHttpSecurityHeadersMiddleware>()
                .UseMiddleware<AddRemoteIpAddressMiddleware>()
                .UseMiddleware<AddVersionHeaderMiddleware>()
                .UseMiddleware<AddNoCacheHeadersMiddleware>()

                .UseMiddleware<DefaultResponseCompressionQualityMiddleware>(new Dictionary<string, double>
                {
                    { "br", 1.0 },
                    { "gzip", 0.9 }
                })
                .UseResponseCompression()

                .UseDefaultFiles()
                .UseStaticFiles()

                .UseMvc()

                .UseSwaggerDocumentation(provider, groupName => $"Basisregisters.Vlaanderen - Wegenregister API {groupName}");

            RegisterApplicationLifetimeHandling(appLifetime);
        }

        private void RegisterApplicationLifetimeHandling(IApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(() => Log.Information("Application started."));

            appLifetime.ApplicationStopping.Register(() =>
            {
                Log.Information("Application stopping.");
                Log.CloseAndFlush();
            });

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                appLifetime.StopApplication();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }
    }
}
