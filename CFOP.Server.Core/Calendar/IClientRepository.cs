using System.Collections.Generic;

namespace CFOP.Server.Core.Calendar
{
    public interface IClientRepository
    {
        List<Client> FindAll();
        void Add(Client client);
        void DeleteBy(string connectionId);
    }
}
