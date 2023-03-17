dotnet build ../src/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy.csproj \
    --runtime win-x64 \
    --no-self-contained \
    --configuration Release \
    --output backoffice
cat backoffice/RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy.runtimeconfig.json

dotnet build ../src/RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy/RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy.csproj \
    --runtime win-x64 \
    --no-self-contained \
    --configuration Release \
    --output snapshot
cat snapshot/RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy.runtimeconfig.json
