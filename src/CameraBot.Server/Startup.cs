using CameraBot.Telegram;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CameraBot.Server
{
    public class Startup
    {
        public Startup(IHostEnvironment environment)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddYamlFile("appsettings.yml", true, true)
                .AddYamlFile($"appsettings.{Env.Name}.yml", true, true)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Env.Name}.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCameraBot(Configuration.GetSection("Bot"))
                .AddTelegram(Configuration.GetSection("Bot:Telegram"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseCameraBot();
        }
    }
}