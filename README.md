![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/andead/camera-bot)
![GitHub last commit](https://img.shields.io/github/last-commit/andead/camera-bot)
[![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/kondranazzo/camera-bot)](https://hub.docker.com/r/kondranazzo/camera-bot/builds)
[![Docker Pulls](https://img.shields.io/docker/pulls/kondranazzo/camera-bot)](https://hub.docker.com/r/kondranazzo/camera-bot)
![GitHub](https://img.shields.io/github/license/andead/camera-bot)

# Camera bot

A bot that responds to private messages with snapshots from webcams. 

## Supported messengers

Currently supports Telegram only. 

## How it works

Can be run in Docker or as a standalone app. Uses long-polling to get updates from the messenger, so no external IP is needed. 

1. A user sends a message to the bot.
2. If the message is not a camera name, the bot sends a list of valid camera names.
3. Otherwise, the bot downloads a snapshot and sends it to the user.

## Run

```
docker run kondranazzo/camera-bot \
  -e Bot__Telegram__ApiToken=123456789:ABCDEFGH \
  -e Bot__Cameras__garden__Name=Garden \
  -e Bot__Cameras__garden__SnapshotUrl=http://12.34.56.78/snapshot.jpg
  -e Bot__Cameras__garden__Url=http://12.34.56.78/video.html # optional, used only as an external link in captions
```

### SOCKS

Set `Bot__Telegram__Socks5__Hostname` and `Bot__Telegram__Socks5__Port` environment variables.

### Usernames white lists

Set `Bot__Telegram__AllowedUsernames__0` to the first username, `Bot__Telegram__AllowedUsernames__1` to the second, and so on.

