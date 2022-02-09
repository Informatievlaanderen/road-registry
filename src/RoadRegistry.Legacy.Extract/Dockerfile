FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.2

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

ENTRYPOINT ["./RoadRegistry.Legacy.Extract"]
