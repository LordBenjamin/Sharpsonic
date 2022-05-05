using System.IO;
using System.Threading.Tasks;
using Auricular.Api.Media;
using Auricular.Api.Settings;
using Auricular.DataAccess;
using Auricular.DataAccess.Sqlite;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
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

            // Require authenticated user by default
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    // Needed for cookie auth + CORS (https://stackoverflow.com/a/48108964/2048780)
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    // Prevent ASP.NET Core redirecting to non-existent Identity pages
                    options.Events.OnRedirectToLogin = context => {
                        context.Response.Headers["Location"] = string.Empty;
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                }); ;
            services.AddAuthorization(options =>
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

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

            IConfigurationSection section = Configuration.GetSection(nameof(ApplicationSettings));

            app.UseCors(options => options
                .WithOrigins(section.Get<ApplicationSettings>().CorsAllowedOrigins)
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader());

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
