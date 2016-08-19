namespace CFOP.Server.Core.Calendar
{
    public class Client
    {
        public string ConnectionId { get; private set; }

        public Client(string connectionId)
        {
            ConnectionId = connectionId;
        }

        private Client() { }
    }
}
