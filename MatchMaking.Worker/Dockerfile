FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

COPY Shared.Contracts/ Shared.Contracts/
COPY MatchMaking.Worker/ MatchMaking.Worker/

RUN dotnet restore MatchMaking.Worker/MatchMaking.Worker.csproj
RUN dotnet build MatchMaking.Worker/MatchMaking.Worker.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish MatchMaking.Worker/MatchMaking.Worker.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchMaking.Worker.dll"]
