# Camera bot

A bot that responds to private messages with snapshots from webcams. 

## Supported messengers

Currently supports Telegram only. 

## How it works

Can be run in Docker or as a standalone app. Uses long-polling to get updates from the messenger, so no external IP is needed. 

1. A user sends a message to the bot.
2. If the message is not a camera name, the bot sends a list of valid camera names.
3. Otherwise, the bot downloads a snapshot and sends it to the user.

## Configuration

See [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1).

Via appsettings.json:

```js
{
  "Bot": {
    "Telegram": {
      "Socks5": { 
        // optional SOCKS5 proxy server configuration
        "Hostname": "my-proxy.org",
        "Port": 1234
      },
      "ApiToken": "123456789:ABCDEFGH",
      "AllowedUsernames": [
        // optional white-list of usernames
        "alice",
        "bob"
      ]
    },
    "Cameras": {
      "garden": {
        "Name": "Garden",
        "SnapshotUrl": "http://12.34.56.78/snapshot.jpg", 
        "Url": "http://12.34.56.78/video.html" // sent in the caption as a link
      },
      // etc 
    }
  }
}
```

Via environment variables. I.e., for API token:

```bash
docker run cambot -e Bot__Telegram__ApiToken=123456789:ABCDEFGH
```

## Build

```bash
docker build . -f=src/Dockerfile
```
