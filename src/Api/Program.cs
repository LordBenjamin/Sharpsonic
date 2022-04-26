using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Auricular.Api {
    public class Program {

        public static void Main(string[] args) {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            IConfigurationRoot config = builder.Build();

            string config_dir = config["config_dir"];
            Directory.CreateDirectory(config_dir);
            if (!File.Exists(config_dir + "/appsettings.json") && Directory.Exists(config_dir)) {
                File.Copy("appsettings.json", config_dir + "/appsettings.json");
            }

            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((buildercontext, configbuilder) => {
                configbuilder.AddConfiguration(config);
                configbuilder.AddJsonFile(config_dir + "/appsettings.json", optional: true);
            })
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            }).Build().Run();
        }
    }
}
