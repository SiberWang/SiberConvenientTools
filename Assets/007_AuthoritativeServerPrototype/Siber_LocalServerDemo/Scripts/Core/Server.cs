using System;
using System.Collections.Generic;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class Server
    {
    #region ========== Public Variables ==========

        public NetWork NetWork => netWork;

    #endregion

    #region ========== Private Variables ==========

        private int     nextUpdateTime;
        private NetWork netWork;

        private readonly List<Client>     clientList;
        private readonly List<PlayerData> playerList;

    #endregion

    #region ========== Constructor ==========

        public Server()
        {
            netWork    = new NetWork();
            clientList = new List<Client>();
            playerList = new List<PlayerData>();
        }

    #endregion

    #region ========== Public Methods ==========

        public void AddClient(Client clientID)
        {
            clientList.Add(clientID);
        }

        public void AddPlayer(PlayerData playerData)
        {
            playerList.Add(playerData);
        }

        public void Tick()
        {
            Debug.Log("Server Update");
            ReplyResult();
            MediumManager.Instance.UpdateWorld("0", playerList);
        }

    #endregion

    #region ========== Private Methods ==========

        private void ReplyResult()
        {
            var receive = netWork.Receive();
            if (receive == null) return;
            if (receive is InputAction inputAction)
            {
                var clientID = inputAction.ClientID;
                var playerID = inputAction.PlayerID;
                var client   = clientList.Find(c => c.ClientID == clientID);
                var player   = client.GetPlayerByID(playerID);
                player.Move(inputAction.X);
                Debug.Log("ReplyResult");
            }
        }

    #endregion
    }
}