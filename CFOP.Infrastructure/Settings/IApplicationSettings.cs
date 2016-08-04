namespace CFOP.Infrastructure.Settings
{
    public interface IApplicationSettings
    {
        string Locale { get; }
        string PrimaryKey { get; }
        string SecondaryKey { get; }
        string LuisAppId { get; }
        string LuisSubscriptionId { get; }
        string UsersFilePath { get; }
        string MainUserEmail { get; }
    }
}
