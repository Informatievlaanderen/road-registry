namespace RoadRegistry.UI
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
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run(async context =>
            {
                if (IsConfigPath(context))
                {
                    context.Response.ContentType = "application/javascript";
                    await context.Response.WriteAsync(BuildConfigJavascript(env));
                }
            });
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
                   string.Equals(context.Request.Path.Value.ToLowerInvariant(), "/config.js", StringComparison.OrdinalIgnoreCase);
        }

        private string BuildConfigJavascript(IHostingEnvironment env)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            return new StringBuilder()
                .AppendLine($"window.wegenregisterVersion = '{version}';")
                .AppendLine($"window.wegenregisterApiEndpoint = '{Configuration["Api:Endpoint"]}';")
                .ToString();
        }
    }
}
