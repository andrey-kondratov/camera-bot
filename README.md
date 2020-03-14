![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/andead/camera-bot)
![GitHub last commit](https://img.shields.io/github/last-commit/andead/camera-bot)
[![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/andeadlier/camera-bot)](https://hub.docker.com/r/andeadlier/camera-bot/builds)
[![Docker Pulls](https://img.shields.io/docker/pulls/andeadlier/camera-bot)](https://hub.docker.com/r/andeadlier/camera-bot)
![GitHub](https://img.shields.io/github/license/andead/camera-bot)

# Camera bot

A Telegram bot for HTTP web cameras.

## Description

1. A user sends a `/snapshot` command to the bot.
2. The bot responds with a list of cameras to select.
3. The user selects a camera. 
4. The bot sends a snapshot from that camera in response.

## Supported messengers

- Telegram

## Configuration

Cameras support tree-like structure. An example for appsettings.json:

```
{
  "Bot": {
    "Root": {
      "Children": [{
        "Name": "Garden",
        "SnapshotUrl": "http://12.34.56.78/snapshot.jpg",
        "Url": "http://12.34.56.78/watch.html", // will generate a link to open in browser
        "Website": "http://12.34.56.78", // will append a link to the website
        "Children": [
          // Properties Url, SnapshotUrl and Website are inherited by children
        ]
      }]
    }
  }
}
```

An example for `docker run` with environment variables:

```
docker run andeadlier/camera-bot \
  -e Bot__Telegram__ApiToken=123456789:ABCDEFGH \
  -e Bot__Cameras__root__children__0__Name=Garden \
  -e Bot__Cameras__root__children__0__SnapshotUrl=http://12.34.56.78/snapshot.jpg
```

## Adding to an existing ASP.NET Core application

Install both [CameraBot](https://www.nuget.org/packages/CameraBot/) and [CameraBot.Telegram](https://www.nuget.org/packages/CameraBot.Telegram/) packages:

```
dotnet add package CameraBot
dotnet add package CameraBot.Telegram
```

Update Startup.cs as follows:

```csharp
public void ConfigureServices(IServiceCollection services)
{
  // the bot core logic and Telegram implementation
  services
    .AddCameraBot(Configuration.GetSection("Bot"))
    .AddTelegram(Configuration.GetSection("Bot:Telegram"));

  // ...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
{
  // ...

  // required if you need the webhooks mode
  app.UseCameraBot();
}
```

Describe cameras and set up Telegram in appsettings.json:

```json
{
  "Bot": {
    "Root": {
      "Name": "42nd Street",
      "SnapshotUrl": "https://static.skylinewebcams.com/_9784192001.jpg",
      "Url": "https://www.skylinewebcams.com/en/webcam/united-states/new-york/new-york/nyc-42th-street.html",
      "Website": "https://www.skylinewebcams.com"
    },
    "Telegram": {
      "ApiToken": "<YOUR_API_TOKEN>",
      "Webhook": {
        "Url": "<YOUR_WEBHOOK_URL>"
      }
    }
  }
}
```

### Webhooks

The mode is selected upon startup based on presence of the webhook URL in the app configuration.
If `Bot__Telegram__Webhook__Url` is set, the server will set up the webhook in Telegram and wait for
incoming HTTPS requests. 

It is required that the Url is accessible from the Internet so that Telegram servers can send requests to it. 
It is recommended that you append a secret token to the URL (see [here](https://core.telegram.org/bots/api#setwebhook) for details).

NOTE: The webhook registration is not removed in Telegram upon shutdown. This is crucial for other
replicas of the same bot to be able to continue receiving messages when one replica is scaled down.

### Long polling

If the webhook URL is not set, the server will assume the long-polling mode and begin polling 
updates from Telegram until shut down. 

When using long-polling you don't have to expose any ports to the Internet. However, you cannot launch multiple bot replicas.

### Proxy servers

SOCKS5 proxy is supported for the outgoing connections from the bot to Telegram via `Bot__Telegram__Socks5__Hostname` and `Bot__Telegram__Socks5__Port` environment variables.

### Usernames white lists

Set `Bot__Telegram__AllowedUsernames__0` to the first username, `Bot__Telegram__AllowedUsernames__1` to the second, and so on. The bot will discard any updates that came from users having usernames other than in the `AllowedUsernames` list. 

When no white list specified, the bot answers everyone.

### Error handling

- `Bot__RetryCount` sets the number of times to retry downloading a snapshot, the default is 3;
- `Bot__TimeoutMilliseconds` sets the max number in milliseconds for the snapshot downloading to complete, the default is 1000 ms.

### Feedback

For the `/feedback` command, the configuration options are:

- `Bot__Telegram__Feedback__ChatId` - the id of the chat where to forward the feedback messages;
- `Bot__Telegram__Feedback__Header` - an optional header to prepend to the messages.
