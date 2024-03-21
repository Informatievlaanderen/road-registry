#!/bin/bash
awslocal iam create-role \
    --role-name rl-vbr-basisregisters-lam-wr-sqsbackofficehandlerfunction \
    --assume-role-policy-document lambda-backoffice-rule-policy.json \
     --region eu-west-1

awslocal lambda create-function \
    --function-name backoffice-lambda-proxy \
    --zip-file fileb:///etc/localstack/lambda/backoffice.zip \
    --handler RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy::RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy.Function::FunctionHandler \
    --role arn:aws:iam:eu-west-1:000000000000:role/rl-vbr-basisregisters-lam-wr-sqsbackofficehandlerfunction \
    --runtime dotnet8 \
    --package-type zip \
    --timeout 10 \
    --memory-size 128 \
    --region eu-west-1 \
    --publish

awslocal lambda create-event-source-mapping \
    --function-name backoffice-lambda-proxy \
    --event-source-arn arn:aws:sqs:eu-west-1:000000000000:road-registry-backoffice.fifo \
     --region eu-west-1 \
    --batch-size 1

awslocal lambda create-event-source-mapping \
    --function-name backoffice-lambda-proxy \
    --event-source-arn arn:aws:sqs:eu-west-1:000000000000:road-registry-attributes \
     --region eu-west-1 \
    --batch-size 1
