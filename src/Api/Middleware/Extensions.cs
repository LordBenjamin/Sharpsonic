using Microsoft.AspNetCore.Builder;

namespace Api.Middleware {
    public static class Extensions {
        public static IApplicationBuilder UseSubsonicAuthentication(
              this IApplicationBuilder builder) {
            return builder.UseMiddleware<SubsonicAuthenticationMiddleware>();
        }
        public static IApplicationBuilder RequireCompatibleSubsonicApiClient(
              this IApplicationBuilder builder) {
            return builder.UseMiddleware<SubsonicApiVersioningMiddleware>();
        }
    }
}
