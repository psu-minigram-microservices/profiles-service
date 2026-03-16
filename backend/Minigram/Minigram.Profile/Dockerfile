FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Minigram.Profile/Minigram.Profile.csproj Minigram.Profile/
COPY Minigram.Core/Minigram.Core.csproj Minigram.Core/

RUN dotnet restore Minigram.Profile/Minigram.Profile.csproj --force 

COPY Minigram.Core/ Minigram.Core/
COPY Minigram.Profile/ Minigram.Profile/

WORKDIR /src/Minigram.Profile
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Minigram.Profile.dll"]
