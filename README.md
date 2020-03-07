![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/andead/camera-bot)
![GitHub last commit](https://img.shields.io/github/last-commit/andead/camera-bot)
[![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/andeadlier/camera-bot)](https://hub.docker.com/r/andeadlier/camera-bot/builds)
[![Docker Pulls](https://img.shields.io/docker/pulls/andeadlier/camera-bot)](https://hub.docker.com/r/andeadlier/camera-bot)
![GitHub](https://img.shields.io/github/license/andead/camera-bot)

# Camera bot

A bot for HTTP web cameras snapshots.

## Supported messengers

Currently supports Telegram only. 

## How it works

Can be run in Docker or as a standalone app. Uses long-polling or webhooks to get updates from the messenger. 

1. A user sends a message to the bot.
2. If the message is not a camera name, the bot sends a list of valid camera names.
3. Otherwise, the bot downloads a snapshot and sends it to the user.

## Run

```
docker run andeadlier/camera-bot \
  -e Bot__Telegram__ApiToken=123456789:ABCDEFGH \
  -e Bot__Cameras__root__children__0__Name=Garden \
  -e Bot__Cameras__root__children__0__SnapshotUrl=http://12.34.56.78/snapshot.jpg \
  -e Bot__Cameras__root__children__0__Url=http://12.34.56.78/video.html # optional, used only as an external link in captions
```

### SOCKS

Set `Bot__Telegram__Socks5__Hostname` and `Bot__Telegram__Socks5__Port` environment variables.

### Usernames white lists

Set `Bot__Telegram__AllowedUsernames__0` to the first username, `Bot__Telegram__AllowedUsernames__1` to the second, and so on.

### Webhooks

Set `Bot__Telegram__Webhook__Url` to the webhook URL. If omitted, long-polling is used.

### Errors handling policy

- `Bot__RetryCount` sets the number of times to retry downloading a snapshot, the default is 3;
- `Bot__TimeoutMilliseconds` sets the max number in milliseconds for the snapshot downloading to complete, the default is 1000 ms.
