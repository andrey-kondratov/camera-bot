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

```bash
docker run kondranazzo/camera-bot \
  -e Bot__Telegram__ApiToken=123456789:ABCDEFGH \
  -e Bot__Telegram__Cameras__garden__Name=Garden \
  -e Bot__Telegram__Cameras__garden__SnapshotUrl=http://12.34.56.78/snapshot.jpg
```
