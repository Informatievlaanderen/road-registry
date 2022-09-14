using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Newtonsoft.Json;
using Uploads;

public class Function
{
    /// <summary>
    ///     Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda
    ///     environment
    ///     the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    ///     region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
    }


    /// <summary>
    ///     This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
    ///     to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records) await ProcessMessageAsync(message, context);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        //
        context.Logger.LogInformation($"Processed message {message.Body}");

        // Retrieve the actual archive identifier
        var archiveId = message.Body;

        /*
         *  1.  Run feature compare inside Docker container
         *
         *      1.1 Pickup arguments inside Docker container
         *      1.2 Download input from S3
         *      1.2 Run feature compare
         *      1.3 Upload output to S3
         *      1.4 Dispatcher sends command
         *      1.5 Terminate feature compare and shutdown container
         *
         *  2.  Detect if there is still an active container running
         *
         *  3.  Catch output from Docker container
         *
         *      3.1 Continue with normal flow through Dispatch
         */


        //TODO-rik download archive from S3: messageEvent.Id
        //TODO-rik start feature-compare docker container
        //docker run -e "INPUTBLOBNAME=foo" -e "OUTPUTBLOBNAME=bar" feature-compare-console
        //TODO-rik zip result and upload to S3
        //TODO-rik de originele upload logica moet worden uitgevoerd, launch event?
        //POST /v1/upload/afterfeaturecompare (met archiveid ipv file te uploaden) of UploadExtractRequest toevoegen aan SQS?
        //var request = new UploadExtractRequest() { ArchiveId = "" };

        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }
}
