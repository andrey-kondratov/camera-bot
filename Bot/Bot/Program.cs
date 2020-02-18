using Microsoft.Extensions.Configuration;
using MihaZupan;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Andead.CameraBot
{
    internal static class Program
    {
        private static async Task<int> Main()
        {
            // set up
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, args) => { cts.Cancel(); args.Cancel = true; };

            try
            {
                Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
                var options = new Options();
                new ConfigurationBuilder().AddEnvironmentVariables().Build().Bind(options);
                var telegram = new TelegramBotClient(options.TelegramBotApiKey, new HttpToSocks5Proxy(options.Socks5Hostname, options.Socks5Port));
                using var camera = new HttpClient();

                bool ok = await telegram.TestApiAsync(cts.Token);
                Log.Write(ok ? LogEventLevel.Information : LogEventLevel.Error, $"API test {(ok ? "ok" : "failed")}.");
                if (!ok)
                {
                    return 2;
                }

                // loop
                int offset = 0;
                Log.Information("Starting the updates loop.");
                while (!cts.IsCancellationRequested)
                {
                    Update[] updates = await telegram.GetUpdatesAsync(offset,
                        timeout: options.TelegramPollingTimeoutSeconds,
                        cancellationToken: cts.Token);

                    if (updates.Any())
                    {
                        Log.Information($"Got {updates.Length} updates, preparing message.");
                        using Stream image = await camera.GetStreamAsync(options.CameraImageUrl);
                        var photo = new InputOnlineFile(image);

                        foreach (Update update in updates)
                        {
                            Log.Information($"Sending photo to chat of update {update.Id}.");
                            await telegram.SendPhotoAsync(update.Message.Chat.Id, photo, cancellationToken: cts.Token);

                            offset = update.Id + 1;
                        }

                        Log.Information($"Finished posting.");
                    }

                    if (cts.IsCancellationRequested)
                    {
                        Log.Information($"Cancellation requested, exiting the loop.");
                        break;
                    }

                    await Task.Delay(options.IntervalMs);
                }

                return 0;
            }
            catch (TaskCanceledException)
            {
                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Fatal crash.");
                return 1;
            }
            finally
            {
                Log.Information("Shutting down.");
                Log.CloseAndFlush();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
