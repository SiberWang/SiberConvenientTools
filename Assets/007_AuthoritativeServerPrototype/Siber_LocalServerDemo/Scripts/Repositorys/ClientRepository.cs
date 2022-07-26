using System.Collections.Generic;
using System.Linq;
using LocalServerDemo.Scripts;

namespace LocalServerDemo.Repositorys
{
    public class ClientRepository : Singleton<ClientRepository>
    {
    #region ========== Private Variables ==========

        private readonly List<Client> clientList = new List<Client>();

    #endregion

    #region ========== Public Methods ==========

        public void AddClient(Client client)
        {
            clientList.Add(client);
        }

        public Client GetClient(int index)
        {
            if (clientList[index] != null)
                return clientList[index];
            CatchYouBug.Debug($"Can't find Client by List index:[{index}]", "ClientRepository");
            return null;
        }

        public Client GetClient(string clientID)
        {
            var client = clientList.Find(c => c.ClientID.Equals(clientID));
            if (client != null) return client;
            CatchYouBug.Debug($"Can't find Client by ID:[{clientID}]", "ClientRepository");
            return null;
        }
        
        public List<Client> GetClients()
        {
            return clientList;
        }

        /// <summary> 獲得指定 ID 之外的 Clients </summary>
        /// <param name="clientID">Client ID</param>
        /// <returns></returns>
        public List<Client> GetRestOfClients(string clientID)
        {
            return clientList.Where(c => !c.ClientID.Equals(clientID)).ToList();
        }

    #endregion
        
    }
}