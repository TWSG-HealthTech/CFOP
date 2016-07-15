using System.Configuration;

namespace CFOP.Infrastructure.Settings
{
    public class ApplicationSettings : IApplicationSettings
    {
        public string SubscriptionKey { get; } = ConfigurationManager.AppSettings["SubscriptionKey"];
        public string LuisAppId { get; } = ConfigurationManager.AppSettings["LuisAppId"];
        public string LuisSubscriptionId { get; } = ConfigurationManager.AppSettings["LuisSubscriptionId"];
    }
}
