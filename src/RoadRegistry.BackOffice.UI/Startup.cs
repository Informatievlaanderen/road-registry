namespace RoadRegistry.BackOffice.UI
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                if (IsIndexPath(context))
                {
                    var parentUri = GetParentUriString(GetUri(context.Request));
                    context.Response.Redirect(parentUri, true);
                }
                else
                {
                    await next();
                }
            });

            app.UseWhen(IsConfigPath, builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.ContentType = "application/javascript";
                    await context.Response.WriteAsync(BuildConfigJavascript(env));
                });
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        public IConfiguration Configuration { get; }

        private static bool IsIndexPath(HttpContext context)
        {
            return context.Request.Path.HasValue &&
                   context.Request.Path.Value.ToLowerInvariant().Contains("/index.html");
        }

        private static Uri GetUri(HttpRequest request)
        {
            var builder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Path = request.Path,
                Query = request.QueryString.ToUriComponent()
            };

            if (request.Host.Port.HasValue)
                builder.Port = request.Host.Port.Value;

            return builder.Uri;
        }

        private static string GetParentUriString(Uri uri)
        {
            return uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length);
        }

        private static bool IsConfigPath(HttpContext context)
        {
            return context.Request.Path.HasValue &&
                (
                   string.Equals(context.Request.Path.Value.ToLowerInvariant(), "/config.js", StringComparison.OrdinalIgnoreCase)
                || string.Equals(context.Request.Path.Value.ToLowerInvariant(), "/config.f84e1103.js", StringComparison.OrdinalIgnoreCase)
                );
        }

        private string BuildConfigJavascript(IWebHostEnvironment env)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            return new StringBuilder()
                .AppendLine($"window.wegenregisterVersion = '{version}';")
                .AppendLine($"window.wegenregisterApiEndpoint = '{Configuration["Api:Endpoint"]}';")
                .ToString();
        }
    }
}
