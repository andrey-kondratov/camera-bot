using System;
using CameraBot.Telegram;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace CameraBot.Server
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = CreateLoggerConfiguration().CreateLogger();

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

        private static LoggerConfiguration CreateLoggerConfiguration()
        {
            LoggerConfiguration configuration = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", Env.Name)
                .Enrich.WithProperty("Version", new
                {
                    Bot = typeof(CameraBotOptions).Assembly.GetName().Version.ToString(3),
                    Telegram = typeof(TelegramOptions).Assembly.GetName().Version.ToString(3),
                    Server = typeof(Program).Assembly.GetName().Version.ToString(3)
                }, true);

            configuration = Env.IsDevelopment
                ? configuration
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                : configuration
                    .MinimumLevel.Information()
                    .WriteTo.Console(new JsonFormatter());

            return configuration;
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder<Startup>(args)
                .UseKestrel(options => options.ListenAnyIP(Env.Port))
                .UseSerilog();
        }
    }
}