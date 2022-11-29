FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.3

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="road-registry"

COPY / /app
WORKDIR /app

ENTRYPOINT ["./RoadRegistry.Legacy.Extract"]
