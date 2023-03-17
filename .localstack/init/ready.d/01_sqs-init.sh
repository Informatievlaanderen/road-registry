#!/bin/bash
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true,DeduplicationScope=messageGroup
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry-backoffice.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true,DeduplicationScope=messageGroup
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry-feature-compare-request.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true,DeduplicationScope=messageGroup
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry-feature-compare-response.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true,DeduplicationScope=messageGroup
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry-snapshot.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true,DeduplicationScope=messageGroup
awslocal sqs create-queue --region eu-west-1 --queue-name road-registry-attributes --attributes FifoQueue=false
