#!/usr/bin/env bash

aws sso login --profile vbr-test

curl 'https://basisregisters-local-dev.s3.eu-west-1.amazonaws.com/database_backup.zip'

curl 'https://basisregisters-local-dev.s3.eu-west-1.amazonaws.com/legacy_db_filled.tar.gz'
tar -xvf legacy_db_filled.tar.gz