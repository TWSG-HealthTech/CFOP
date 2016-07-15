namespace CFOP.Infrastructure.Settings
{
    public interface IApplicationSettings
    {
        string SubscriptionKey { get; }
        string LuisAppId { get; }
        string LuisSubscriptionId { get; }
    }
}
