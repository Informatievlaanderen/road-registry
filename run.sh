#!/usr/bin/env bash
set -e

docker-compose up --build municipality-api streetname-api syndication-projection-host editor-projection-host product-projection-host wms-projection-host backoffice-event-host backoffice-command-host backoffice-api backoffice-ui
