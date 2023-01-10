# Empty AWS Serverless Application Project

This starter project consists of:
* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* Function.cs - class file containing the C# method mapped to the single function declared in the template file
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS

You may also have a test project depending on the options selected.

The generated project contains a Serverless template declaration for a single AWS Lambda function that will be exposed through Amazon API Gateway as a HTTP *Get* operation. Edit the template to customize the function or add more functions and other resources needed by your application, and edit the function code in Function.cs. You can then deploy your Serverless application.

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "RoadRegistry.BackOffice.Handlers.Sqs.Lambda/test/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests"
    dotnet test
```

Deploy application
```
    cd "RoadRegistry.BackOffice.Handlers.Sqs.Lambda/src/RoadRegistry.BackOffice.Handlers.Sqs.Lambda"
    dotnet lambda deploy-serverless
```

# Sample request
```
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb78",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"type\":\"RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.LinkStreetNameSqsRequest\",\"data\":\"{\\\"Request\\\":{\\\"wegsegmentId\\\":2,\\\"linkerstraatnaamId\\\":\\\"https://data.vlaanderen.be/id/straatnaam/36847\\\"},\\\"ProvenanceData\\\":{\\\"Operator\\\":\\\"Me\\\"}}\"}",
      "attributes": {
           "MessageGroupId": "5"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d91d",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:{partition}:sqs:euwest1:123456789012:MyQueue",
      "awsRegion": "euwest1"
    }
  ]
}
```
