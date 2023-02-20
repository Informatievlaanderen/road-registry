#!/usr/bin/env bash
set -e

export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm
[ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion"  # This loads nvm bash_completion
nvm install 18.12.1
nvm use 18.12.1
dotnet tool restore
dotnet paket restore
chmod +x packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/*

if [ $# -eq 0 ]
then
  FAKE_DETAILED_ERRORS=true FAKE_ALLOW_NO_DEPENDENCIES=true dotnet fake build
else
  FAKE_DETAILED_ERRORS=true FAKE_ALLOW_NO_DEPENDENCIES=true dotnet fake build -t "$@"
fi
