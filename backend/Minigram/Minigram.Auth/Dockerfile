FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Minigram.Auth/Minigram.Auth.csproj Minigram.Auth/
COPY Minigram.Core/Minigram.Core.csproj Minigram.Core/

RUN dotnet restore Minigram.Auth/Minigram.Auth.csproj --force 

COPY Minigram.Core/ Minigram.Core/
COPY Minigram.Auth/ Minigram.Auth/

WORKDIR /src/Minigram.Auth
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Minigram.Auth.dll"]
