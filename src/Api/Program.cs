using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Auricular.Api {
    public class Program {

        public static void Main(string[] args) {
            Directory.CreateDirectory("/config");

            // TODO: Hardcoded paths - can we pick this up from the environment?
            // E.g. what happens on a Windows container?
            if (!File.Exists("/config/appsettings.json") && Directory.Exists("/config")) {
                File.Copy("appsettings.json", "/config/appsettings.json");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((buildercontext, config) => {

                config.AddJsonFile("/config/appsettings.json", optional: true);
            })
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });
    }
}
