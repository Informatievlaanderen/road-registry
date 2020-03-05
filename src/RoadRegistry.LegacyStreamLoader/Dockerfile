FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.2

LABEL maintainer "Agentschap Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

ENTRYPOINT ["./init.sh"]
