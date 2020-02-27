using System;
using Andead.CameraBot.Telegram;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Andead.CameraBot.Server
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", new
                {
                    Bot = typeof(CameraBotOptions).Assembly.GetName().Version.ToString(3),
                    Telegram = typeof(TelegramOptions).Assembly.GetName().Version.ToString(3),
                    Server = typeof(Program).Assembly.GetName().Version.ToString(3)
                }, true)
                .WriteTo.Console(
#if !DEBUG
                    new Serilog.Formatting.Json.JsonFormatter()
#endif
                )
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder<Startup>(args)
                .UseSerilog();
        }
    }
}