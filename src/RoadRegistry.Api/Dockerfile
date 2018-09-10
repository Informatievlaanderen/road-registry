FROM microsoft/dotnet:2.1.1-runtime-deps

LABEL maintainer "Informatie Vlaanderen <informatie.vlaanderen@vlaanderen.be>"

COPY / /app
WORKDIR /app

# 5000 is the default mapped port?
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000

# Need to run both, so will need a script to start them
# - RoadRegistry.Api
# - RoadRegistry.UI
ENTRYPOINT ["./start_site.sh"]
