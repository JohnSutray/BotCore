using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace BotCore.Extensions.Query {
  public static class StringExtensions {
    public static bool IsRouteMatched(this string routeTemplate, string requestPath) =>
      routeTemplate.MatchRoute(requestPath) != null;

    public static RouteValueDictionary MatchRoute(this string routeTemplate, string requestPath) {
      var templateWithSlash = routeTemplate.EnsureStartWithSlash();
      var requestPathWithSlash = requestPath.EnsureStartWithSlash();
      var template = TemplateParser.Parse(templateWithSlash);
      var matcher = new TemplateMatcher(template, GetDefaults(template));
      var values = new RouteValueDictionary();

      return matcher.TryMatch(requestPathWithSlash, values) ? values : null;
    }

    private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate) {
      var result = new RouteValueDictionary();

      foreach (var parameter in parsedTemplate.Parameters) {
        if (parameter.DefaultValue != null) {
          result.Add(parameter.Name, parameter.DefaultValue);
        }
      }

      return result;
    }

    private static string EnsureStartWithSlash(this string value) => value.StartsWith("/")
      ? value
      : $"/{value}";
  }
}
