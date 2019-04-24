FROM microsoft/dotnet:2.2.0-runtime-deps

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /dist
WORKDIR /dist

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["./RoadRegistry.UI"]
