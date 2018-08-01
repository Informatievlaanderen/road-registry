FROM microsoft/dotnet:2.0.7-runtime-deps

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["./RoadRegistry.UI"]
