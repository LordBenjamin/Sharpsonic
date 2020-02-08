using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api {
    public class Program {
        public static Dictionary<string, string> arrayDict = new Dictionary<string, string>
        {
            {"ApplicationSettings:MusicSourceDirectory", "/music"},
            {"ApplicationSettings:UserName", "A"},
            {"ApplicationSettings:Password", "b"},
        };

        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => {
                config.AddInMemoryCollection(arrayDict);
            })
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });
    }
}
