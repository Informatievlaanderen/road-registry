#!/bin/bash

# Pre-generates the Marten code for one host (GAWR-7236), so the host does not have to compile it with Roslyn on the
# first use of every document type and projection. `codegen write` writes it into src/<project>/Internal/Generated,
# where the build that follows compiles it into the host's own assembly. The folder is ignored by git.
#
# Run from the repository root, after pre-restore.sh:
#
#   ./marten-codegen.sh RoadRegistry.Projector
#
# The build pipeline calls it through the prerestore-script input of the shared build workflows, which is the only
# hook they offer before the host is built.

set -euo pipefail

project="${1:?usage: marten-codegen.sh <project>}"
output="src/${project}/Internal/Generated"

rm -rf "src/${project}/Internal"

# Development, because that is the configuration a host can be built from outside its deployment: every host ships
# an appsettings.development.json, while the settings it needs in production only exist in its deployment. A host whose
# startup is short of a required setting does not fail - the startup error is captured - it just comes up without its
# services, and there is then no Marten store to generate code for. Development and production generate exactly the
# same files. Nothing is connected to: the host is built, never started. The launch profile is left out so nothing
# but this environment decides the configuration.
ASPNETCORE_ENVIRONMENT=Development DOTNET_ENVIRONMENT=Development \
  dotnet run -c Release --no-launch-profile --project "src/${project}" -- codegen write

count=$(find "${output}" -name '*.cs' 2>/dev/null | wc -l)
if [ "${count}" -eq 0 ]; then
  echo "No Marten code was generated for ${project}: the host did not register a Marten store." >&2
  exit 1
fi

echo "Generated ${count} Marten code file(s) for ${project}."
