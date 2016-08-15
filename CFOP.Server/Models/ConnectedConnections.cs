using System.Collections.Generic;

namespace CFOP.Server.Models
{
    public class ConnectedConnections
    {
        private static readonly HashSet<string> _connections = new HashSet<string>();

        public static void Add(string connectionId)
        {
            _connections.Add(connectionId);
        }

        public static void Remove(string connectionId)
        {
            if (_connections.Contains(connectionId))
            {
                _connections.Remove(connectionId);
            }
        }

        public static IEnumerable<string> GetAll()
        {
            return _connections;
        }
    }
}
