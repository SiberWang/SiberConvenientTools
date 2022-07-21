using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 處理 A , B 角色 的動作
    /// </summary>
    public class Client
    {
    #region ========== Public Variables ==========

        public int Lag => lag;

        public NetWork NetWork => netWork;

        public string ClientID => clientID;

    #endregion

    #region ========== Private Variables ==========

        private ControlSystem controlSystem;

        private Dictionary<string, BallBehaviour> ballDict = new Dictionary<string, BallBehaviour>();

        private NetWork netWork;
        private Server  server;
        private string  clientID; // clientA="1" , clientB="2"
        private string  mainBallID;
        private int     lag;

    #endregion

    #region ========== Constructor ==========

        public Client(Server server, ControlSystem controlSystem, string clientID)
        {
            netWork            = new NetWork();
            this.clientID      = clientID;
            this.server        = server;
            this.controlSystem = controlSystem;
        }

    #endregion

    #region ========== Public Methods ==========

        public BallBehaviour GetPlayerByID(string ID)
        {
            if (ballDict.ContainsKey(ID))
                return ballDict[ID];
            return null;
        }

        public void SetLag(int value)
        {
            lag = value;
        }


        public void Tick()
        {
            if (controlSystem.MoveLeft()) MoveAction(mainBallID, -1);
            if (controlSystem.MoveRight()) MoveAction(mainBallID, 1);
            // var playerList = ballDict.Values.ToList();
            // MediumManager.Instance.UpdateWorld(clientID, playerList);
        }

    #endregion

    #region ========== Private Methods ==========

        private void MoveAction(string ID, float X)
        {
            var inputAction = new InputAction();
            inputAction.ClientID  = clientID;
            inputAction.PlayerID  = ID;
            inputAction.CacheTime = new CacheTime();
            inputAction.X         = X;
            server.NetWork.Send(lag, inputAction);
        }

    #endregion

        public void AddPlayer(PlayerData playerData)
        {
        }
    }
}