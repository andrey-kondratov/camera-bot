FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "CameraBot.Server/CameraBot.Server.csproj" \
    --runtime linux-musl-x64 --configuration Release \
    --output /app --self-contained true /p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]