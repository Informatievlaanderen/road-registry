#!/bin/bash

# more specific domains go first
./hostsUtil.sh removehost develop-api.wegen.basisregisters.vlaanderen.be
./hostsUtil.sh removehost develop-ui.wegen.basisregisters.vlaanderen.be

./hostsUtil.sh printHosts
