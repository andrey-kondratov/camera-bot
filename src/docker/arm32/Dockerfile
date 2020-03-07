FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-buster-slim-arm32v7 AS base

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "CameraBot.Server/CameraBot.Server.csproj" \
    --runtime linux-arm --configuration Release \
    --output /app --self-contained true /p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./cbot"]