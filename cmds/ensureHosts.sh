#!/bin/bash

# less specific domains go first
./hostsUtil.sh addhost develop-api.wegen.basisregisters.vlaanderen.be
./hostsUtil.sh addhost develop-ui.wegen.basisregisters.vlaanderen.be

./hostsUtil.sh printHosts
