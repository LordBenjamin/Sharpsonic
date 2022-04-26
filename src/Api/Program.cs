using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Auricular.Api {
    public class Program {

        public static void Main(string[] args) {
            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            IConfigurationRoot config = builder.Build();

            string config_dir = config["config_dir"];
            if (!string.IsNullOrEmpty(config_dir) && !File.Exists(config_dir + "/appsettings.json") && Directory.Exists(config_dir)) {
                throw new InvalidOperationException("bad config_dir - no appsettings.json file found at " + config_dir);
            }

            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((buildercontext, configbuilder) => {
                    configbuilder.AddConfiguration(config);
                    if (!string.IsNullOrEmpty(config_dir)) {
                        configbuilder.AddJsonFile(config_dir + "/appsettings.json", optional: true);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
