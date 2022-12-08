FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.0

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="road-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

EXPOSE 10002/tcp
ENV ASPNETCORE_URLS http://*:10002

ENTRYPOINT ["./init.sh"]
