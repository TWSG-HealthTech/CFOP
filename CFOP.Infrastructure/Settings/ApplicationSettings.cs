using System.Configuration;

namespace CFOP.Infrastructure.Settings
{
    public class ApplicationSettings : IApplicationSettings
    {
        public string Locale { get; } = ConfigurationManager.AppSettings["locale"];
        public string PrimaryKey { get; } = ConfigurationManager.AppSettings["primaryKey"];
        public string SecondaryKey { get; } = ConfigurationManager.AppSettings["secondaryKey"];
        public string LuisAppId { get; } = ConfigurationManager.AppSettings["luisAppId"];
        public string LuisSubscriptionId { get; } = ConfigurationManager.AppSettings["luisSubscriptionId"];
        public string UsersFilePath { get; } = ConfigurationManager.AppSettings["usersFilePath"];
        public string MainUserEmail { get; } = ConfigurationManager.AppSettings["mainUserEmail"];
        public string ServerUrl { get; } = ConfigurationManager.AppSettings["serverUrl"];
    }
}
