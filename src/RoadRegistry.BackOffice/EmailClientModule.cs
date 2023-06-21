namespace RoadRegistry.BackOffice;

using Amazon.SimpleEmailV2;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;
using Module = Autofac.Module;

public class EmailClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(c =>
            {
                var configuration = c.Resolve<IConfiguration>();
                var emailClientOptions = configuration.GetOptions<EmailClientOptions>();
                return emailClientOptions;
            })
            .As<EmailClientOptions>()
            .SingleInstance();

        builder.Register(context => new AmazonSimpleEmailServiceV2Client());
        builder.Register<IExtractUploadFailedEmailClient>(context =>
        {
            var emailClient = context.Resolve<AmazonSimpleEmailServiceV2Client>();
            var emailClientOptions = context.Resolve<EmailClientOptions>();
            var logger = context.Resolve<ILogger<ExtractUploadFailedEmailClient>>();

            return new ExtractUploadFailedEmailClient(emailClient, emailClientOptions, logger);
        }).SingleInstance();
    }
}
