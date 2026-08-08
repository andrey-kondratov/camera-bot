

![CI](https://github.com/andead/camera-bot/workflows/CI/badge.svg)
[![Nuget](https://img.shields.io/nuget/v/CameraBot.Telegram)](https://www.nuget.org/packages/CameraBot.Telegram/)
[![Docker Pulls](https://img.shields.io/docker/pulls/andeadlier/camera-bot)](https://hub.docker.com/r/andeadlier/camera-bot)
![GitHub](https://img.shields.io/github/license/andead/camera-bot)

# Bot de cámaras

Un bot de Telegram para cámaras web HTTP.

## Descripción

1. Un usuario envía el comando `/snapshot` al bot.
2. El bot responde con una lista de cámaras para seleccionar.
3. El usuario selecciona una cámara. 
4. El bot envía una captura de esa cámara como respuesta.

## Mensajerías compatibles

- Telegram

## Ejecución

Un ejemplo para `docker run` con variables de entorno:

```
docker run andeadlier/camera-bot \
  -e Bot__Telegram__ApiToken=123456789:ABCDEFGH \
  -e Bot__Cameras__root__children__0__Name=Garden \
  -e Bot__Cameras__root__children__0__SnapshotUrl=http://12.34.56.78/snapshot.jpg
```

## Agregar a una aplicación existente de ASP.NET Core

Instale el paquete NuGet:

```
dotnet add package CameraBot.Telegram
```

Actualice Startup.cs de la siguiente manera:

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

Describa las cámaras y configure Telegram en appsettings.json:

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

## Características

### Webhooks

El modo se selecciona al iniciar según la presencia de la URL del webhook en la configuración de la aplicación.
Si `Bot__Telegram__Webhook__Url` está establecido, el servidor configurará el webhook en Telegram y esperará
solicitudes HTTPS entrantes. 

Es necesario que la URL sea accesible desde Internet para que los servidores de Telegram puedan enviarle solicitudes. 
Se recomienda agregar un token secreto a la URL (consulte [aquí](https://core.telegram.org/bots/api#setwebhook) para obtener más detalles).

NOTA: El registro del webhook no se elimina en Telegram al cerrar el servidor. Esto es crucial para que otras
réplicas del mismo bot puedan continuar recibiendo mensajes cuando una réplica se reduce en escala.

### Long polling

Si no se establece la URL del webhook, el servidor asumirá el modo long-polling y comenzará a solicitar 
actualizaciones de Telegram hasta que se cierre. 

Al usar long-polling, no es necesario exponer ningún puerto a Internet. Sin embargo, no puede ejecutar múltiples réplicas del bot.

### Servidores proxy

Se admite un proxy SOCKS5 para las conexiones salientes del bot hacia Telegram mediante las variables de entorno `Bot__Telegram__Socks5__Hostname` y `Bot__Telegram__Socks5__Port`.

### Listas blancas de nombres de usuario

Establezca `Bot__Telegram__AllowedUsernames__0` en el primer nombre de usuario, `Bot__Telegram__AllowedUsernames__1` en el segundo, y así sucesivamente. El bot descartará cualquier actualización que provenga de usuarios cuyos nombres de usuario no estén en la lista `AllowedUsernames`. 

Cuando no se especifica una lista blanca, el bot responde a todos.

### Manejo de errores

- `Bot__RetryCount` establece el número de reintentos para descargar una captura, el valor predeterminado es 3;
- `Bot__TimeoutMilliseconds` establece el tiempo máximo en milisegundos para completar la descarga de la captura, el valor predeterminado es 1000 ms.

### Retroalimentación

Para el comando `/feedback`, las opciones de configuración son:

- `Bot__Telegram__Feedback__ChatId` - el id del chat donde se reenviarán los mensajes de retroalimentación;
- `Bot__Telegram__Feedback__Header` - un encabezado opcional para agregar al inicio de los mensajes.
