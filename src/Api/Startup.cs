using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sharpsonic.Api.Formatters;
using Sharpsonic.Api.Media;
using Sharpsonic.Api.Middleware;
using Sharpsonic.Api.Serialization;
using Sharpsonic.Api.Settings;
using Sharpsonic.DataAccess;
using Sharpsonic.DataAccess.Sqlite;

namespace Sharpsonic.Api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            IConfigurationSection section = Configuration.GetSection(nameof(ApplicationSettings));
            services.Configure<ApplicationSettings>(section);

            section = Configuration.GetSection(nameof(MediaLibrarySettings));
            services.Configure<MediaLibrarySettings>(section);

            // Replace FormatFilter with one that is Subsonic URI compatible
            ServiceDescriptor descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(FormatFilter));
            services.Remove(descriptor);
            services.Add(ServiceDescriptor.Singleton<FormatFilter, SubsonicFormatFilter>());

            services.AddControllers();
            services.AddMvcCore(options => {
                // Prefer XML by default
                options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
                options.OutputFormatters.Add(new CustomXmlSerializerOutputFormatter());
                options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(new JsonSerializerOptions()));
            })
            .AddFormatterMappings(mappings => {
                // text/xml as specified in Subsonic API docs
                mappings.SetMediaTypeMappingForFormat("xml", "text/xml");
                mappings.SetMediaTypeMappingForFormat("json", "application/json");

            });

            using(var context = new SqliteDbContext()) {
                context.Database.Migrate();
            }

            services.AddSingleton<IMediaLibrary, SqliteMediaLibrary>();
            services.AddSingleton<MediaScanner>();
            services.AddHostedService<MediaLibraryService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.RequireCompatibleSubsonicApiClient();

            app.UseSubsonicAuthentication();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
