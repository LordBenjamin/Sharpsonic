using System.IO;
using Auricular.Api.Media;
using Auricular.Api.Settings;
using Auricular.DataAccess;
using Auricular.DataAccess.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Auricular.Api {
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

            section = Configuration.GetSection(nameof(SqliteSettings));
            services.Configure<SqliteSettings>(section);

            services.AddCors();

            services.AddControllers();
            services.AddMvcCore();

            SqliteSettings sqliteSettings = section.Get<SqliteSettings>() ?? new SqliteSettings();
            string databaseFileDirectory = Path.GetDirectoryName(sqliteSettings.DatabaseFilename);
            if (!string.IsNullOrEmpty(databaseFileDirectory)) {
                Directory.CreateDirectory(databaseFileDirectory);
            }

            var settings = new SqliteDbContextSettings() { ConnectionString = "Data Source=" + sqliteSettings.DatabaseFilename };
            using (var context = new SqliteDbContext(settings)) {
                context.Database.Migrate();
            }

            services.AddSingleton(settings);
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

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod());

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
