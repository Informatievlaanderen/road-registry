#!/bin/bash
awslocal s3api create-bucket --bucket road-registry-uploads
awslocal s3api create-bucket --bucket road-registry-extract-downloads
awslocal s3api create-bucket --bucket road-registry-sqs-messages
awslocal s3api create-bucket --bucket road-registry-snapshots
awslocal s3api create-bucket --bucket road-registry-feature-compare
