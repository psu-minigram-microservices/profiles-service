FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY Minigram.Profile/Minigram.Profile.csproj Minigram.Profile/
COPY Minigram.Core/Minigram.Core.csproj Minigram.Core/

RUN arch=$([ "$TARGETARCH" = "amd64" ] && echo "x64" || echo "$TARGETARCH") && \
    dotnet restore Minigram.Profile/Minigram.Profile.csproj -a $arch --force

COPY Minigram.Core/ Minigram.Core/
COPY Minigram.Profile/ Minigram.Profile/

WORKDIR /src/Minigram.Profile
RUN arch=$([ "$TARGETARCH" = "amd64" ] && echo "x64" || echo "$TARGETARCH") && \
    dotnet publish -c Release -o /app -a $arch --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Minigram.Profile.dll"]
