FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Be.Vlaanderen.Basisregisters.IdentityServer/Be.Vlaanderen.Basisregisters.IdentityServer.csproj", "src/Be.Vlaanderen.Basisregisters.IdentityServer/"]
RUN dotnet restore "src/Be.Vlaanderen.Basisregisters.IdentityServer/Be.Vlaanderen.Basisregisters.IdentityServer.csproj"
COPY . .
WORKDIR "/src/src/Be.Vlaanderen.Basisregisters.IdentityServer"
RUN dotnet build "Be.Vlaanderen.Basisregisters.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Be.Vlaanderen.Basisregisters.IdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Be.Vlaanderen.Basisregisters.IdentityServer.dll"]