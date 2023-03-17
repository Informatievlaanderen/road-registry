#!/bin/bash

data_dir="/usr/local/lib/localstack"

list_buckets=`awslocal s3api list-buckets --output text --query Buckets[].Name`
for bucket in $list_buckets
do
    echo "Found bucket $bucket"
    bucket_dir="$data_dir/$bucket"
    
    [ ! -d $bucket_dir ] && mkdir $bucket_dir

    list_bucket_objects=`awslocal s3api list-objects --bucket $bucket --output text --query Contents[].Key`
    for bucket_object in $list_bucket_objects
    do
        if [[ "$bucket_object" != "None" ]]
        then
            echo "Found bucket $bucket object $bucket_object"
            bucket_object_path="$bucket_dir/$bucket_object"

            [ ! -f $bucket_object_path ] && awslocal s3api get-object --bucket $bucket --key $bucket_object $bucket_object_path
        fi
    done

done
