#!/bin/bash

data_dir="/usr/local/lib/localstack"

list_buckets=`ls $data_dir`
for bucket in $list_buckets
do
    echo "Found bucket $bucket"
    bucket_dir="$data_dir/$bucket"

    list_bucket_objects=`ls $bucket_dir`
    for bucket_object in $list_bucket_objects
    do
        echo "Found bucket $bucket object $bucket_object"
        bucket_object_path="$bucket_dir/$bucket_object"

        awslocal s3api put-object --bucket $bucket --key $bucket_object --body $bucket_object_path
    done

done
