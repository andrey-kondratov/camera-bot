using System;
using Microsoft.Extensions.Hosting;

namespace Andead.CameraBot.Server
{
    public static class Env
    {
        public static readonly string Name = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                             Environments.Production;
        public static readonly int Port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "443");
        public static bool IsDevelopment => Name == Environments.Development;
    }
}