using System.Collections.Generic;
using System.Linq;

namespace CFOP.Server.Core.Calendar
{
    public class Client
    {
        public int Id { get; private set; }
        public string ConnectionId { get; private set; }
        public List<Subscription> Subscriptions { get; private set; }

        public Client(string connectionId, IEnumerable<string> calendarIds)
        {
            ConnectionId = connectionId;
            Subscriptions = calendarIds.Select(id => new Subscription(id)).ToList();
        }

        private Client() { }
    }
}
