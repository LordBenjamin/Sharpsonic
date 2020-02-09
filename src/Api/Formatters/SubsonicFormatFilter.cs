using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace Sharpsonic.Api.Formatters {
    public class SubsonicFormatFilter : FormatFilter {
        public SubsonicFormatFilter(IOptions<MvcOptions> options, ILoggerFactory loggerFactory)
            : base(options, loggerFactory) {
        }

        // See https://raw.githubusercontent.com/dotnet/aspnetcore/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/Formatters/FormatFilter.cs
        public override string GetFormat(ActionContext context) {
            if (context.RouteData.Values.TryGetValue("f", out object obj)) {
                // null and string.Empty are equivalent for route values.
                string routeValue = Convert.ToString(obj, CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(routeValue) ? null : routeValue;
            }

            Microsoft.Extensions.Primitives.StringValues query = context.HttpContext.Request.Query["f"];
            if (query.Count > 0) {
                return query.ToString();
            }

            return base.GetFormat(context);
        }
    }
}
