#!/usr/bin/env bash
set -e

docker-compose up --build editor-projection-host product-projection-host wms-projection-host backoffice-event-host backoffice-command-host backoffice-api backoffice-ui
