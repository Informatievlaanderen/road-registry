FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.13

# create group & user
RUN addgroup --gid 1000 --system app && adduser --uid 1000 -system app --gid 1000

# create work dir and set permissions as WORKDIR sets permissions as root
RUN mkdir /app && chown -R app:app /app
WORKDIR /app

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="road-registry"

COPY / /app
WORKDIR /app

# switch to created user
USER app

ENTRYPOINT ["./RoadRegistry.Legacy.Extract"]
