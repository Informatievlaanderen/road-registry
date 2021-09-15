FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.10

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

ENTRYPOINT ["./RoadRegistry.Legacy.Extract"]
