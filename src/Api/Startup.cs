using Sharpsonic.Api.Media;
using Sharpsonic.Api.Middleware;
using Sharpsonic.Api.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Json;
using Sharpsonic.Api.Formatters;
using System.Linq;

namespace Sharpsonic.Api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
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

            IConfigurationSection section = Configuration.GetSection("ApplicationSettings");

            services.Configure<ApplicationSettings>(section);

            MediaIndex index = new MediaIndex(section.Get<ApplicationSettings>().SourceDirectory);
            index.Scan();

            services.AddSingleton(index);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
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
