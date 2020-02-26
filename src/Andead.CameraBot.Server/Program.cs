using System;
using System.IO;
using System.Reflection;
using Andead.CameraBot.Messaging;
using Andead.CameraBot.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Andead.CameraBot.Server
{
    public static class Program
    {
        public static int Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", new
                {
                    Bot = typeof(IMessenger).Assembly.GetName().Version.ToString(3),
                    Telegram = typeof(Messenger).Assembly.GetName().Version.ToString(3),
                    Server = typeof(Program).Assembly.GetName().Version.ToString(3)
                }, true)
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder().Build().Run();
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

        public static IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder();

            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureHostConfiguration(config => config.AddEnvironmentVariables("DOTNET_"));

            builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                    if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                    {
                        Assembly appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        config.AddUserSecrets(appAssembly, true);
                    }

                    config.AddEnvironmentVariables();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                });

            builder.ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                IConfiguration configuration = hostContext.Configuration;

                services
                    .AddCameraBot(configuration.GetSection("Bot"))
                    .AddTelegram(configuration.GetSection("Bot:Telegram"));
            });

            return builder;
        }
    }
}