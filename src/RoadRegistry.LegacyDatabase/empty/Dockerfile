FROM ubuntu:18.04

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY entrypoint.sh ./

RUN apt update && \
    apt-get install dialog apt-utils gnupg2 curl -y && \
    echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/18.04/prod.list | tee /etc/apt/sources.list.d/msprod.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install mssql-tools unixodbc-dev -y && \
    chmod +x ./entrypoint.sh

CMD /bin/bash ./entrypoint.sh
