FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.2

LABEL maintainer "agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

ENTRYPOINT ["./RoadRegistry.LegacyStreamExtraction"]
