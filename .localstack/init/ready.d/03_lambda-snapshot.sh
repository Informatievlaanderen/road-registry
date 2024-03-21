#!/bin/bash
awslocal iam create-role \
    --role-name rl-vbr-basisregisters-lam-wr-sqssnapshothandlerfunction \
    --assume-role-policy-document lambda-snapshot-rule-policy.json \
     --region eu-west-1

awslocal lambda create-function \
    --function-name snapshot-lambda-proxy \
    --zip-file fileb:///etc/localstack/lambda/snapshot.zip \
    --handler RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy::RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy.Function::FunctionHandler \
    --role arn:aws:iam:eu-west-1:000000000000:role/rl-vbr-basisregisters-lam-wr-sqssnapshothandlerfunction \
    --runtime dotnet8 \
    --package-type zip \
    --timeout 10 \
    --memory-size 128 \
    --region eu-west-1 \
    --publish 

awslocal lambda create-event-source-mapping \
    --function-name snapshot-lambda-proxy \
    --event-source-arn arn:aws:sqs:eu-west-1:000000000000:road-registry-snapshot.fifo \
     --region eu-west-1 \
    --batch-size 1

