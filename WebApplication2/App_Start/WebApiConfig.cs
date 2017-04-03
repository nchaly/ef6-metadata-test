using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WebApplication2
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            ConfigureFormatters(config);

            var x = config.Services.GetHttpControllerSelector().GetControllerMapping();

        }
        private static void ConfigureFormatters(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SerializerSettings.ApplyDefaultDataServicesSerializationSettings();
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }

    }
    public static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings ApplyDefaultDataServicesSerializationSettings(this JsonSerializerSettings settings)
        {
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.DefaultValueHandling = DefaultValueHandling.Include;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });

            return settings;
        }
    }
}
