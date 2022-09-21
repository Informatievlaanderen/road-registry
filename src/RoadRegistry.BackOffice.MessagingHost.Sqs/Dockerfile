#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/RoadRegistry.BackOffice.MessagingHost.Sqs/RoadRegistry.BackOffice.MessagingHost.Sqs.csproj", "src/RoadRegistry.BackOffice.MessagingHost.Sqs/"]
RUN dotnet restore "src/RoadRegistry.BackOffice.MessagingHost.Sqs/RoadRegistry.BackOffice.MessagingHost.Sqs.csproj"
COPY . .
WORKDIR "/src/src/RoadRegistry.BackOffice.MessagingHost.Sqs"
RUN dotnet build "RoadRegistry.BackOffice.MessagingHost.Sqs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RoadRegistry.BackOffice.MessagingHost.Sqs.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RoadRegistry.BackOffice.MessagingHost.Sqs.dll"]