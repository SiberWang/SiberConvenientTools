using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityTimer;
using Timer = System.Threading.Timer;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    /// <summary>
    /// 處理 A , B 角色 的動作
    /// </summary>
    public class Client
    {
    #region ========== Public Variables ==========

        /// <summary> 客戶端預測 Client-side prediction </summary>
        public bool SidePrediction;

        public int Lag => lag;

        public string ClientID => clientID;

        public List<PlayerData> PlayerDataList => playerDataList;

        public List<InputAction> InputList => inputList;

    #endregion

    #region ========== Private Variables ==========

        private ControlSystem controlSystem;

        private List<InputAction> inputList      = new List<InputAction>();
        private List<PlayerData>  playerDataList = new List<PlayerData>();

        private Dictionary<string, GameObject> playerDict = new Dictionary<string, GameObject>();

        private Server server;
        private string clientID; // clientA="1" , clientB="2"
        private string MainPlayerID;
        private int    lag; //ms
        private int    inputNumber;

        private ulong updateRate;
        private ulong nextUpdateTime;

    #endregion

    #region ========== Constructor ==========

        public Client(Server server, ControlSystem controlSystem, string clientID)
        {
            this.clientID      = clientID;
            this.server        = server;
            this.controlSystem = controlSystem;
            updateRate         = 60; // 預設更新率為60幀
            // CatchYouBug.DeShow($"ClientID :[{clientID}] , ControlSystem Index :[{controlSystem.Index}]");
        }

    #endregion

    #region ========== Public Methods ==========

        public void Tick()
        {
            var now = CacheTime.Now();
            if (now < nextUpdateTime) return;
            if (controlSystem.MoveLeft()) MoveAction(MainPlayerID, -1);
            else if (controlSystem.MoveRight()) MoveAction(MainPlayerID, 1);
            nextUpdateTime = now + updateRate;
        }

        public void SetLag(int value)
        {
            lag = value;
        }

        public void AddPlayer(PlayerData playerData, GameObject gameObject)
        {
            playerDict.Add(playerData.ID, gameObject);
            playerDataList.Add(playerData);
            if (playerDataList.Count >= 2)
            {
                if (clientID == "1") MainPlayerID = playerDataList[0].ID;
                if (clientID == "2") MainPlayerID = playerDataList[1].ID;
            }
        }

        public GameObject GetPlayerByID(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            if (playerDict.ContainsKey(ID))
                return playerDict[ID];
            return null;
        }

    #endregion

    #region ========== Private Methods ==========

        private void MoveAction(string playerID, float x)
        {
            inputNumber++;
            var inputAction = new InputAction();

            inputAction.ClientID    = clientID;
            inputAction.PlayerID    = playerID;
            inputAction.CacheTime   = new CacheTime();
            inputAction.inputNumber = inputNumber;
            inputAction.X           = x;

            server.NetWork.Send(lag, inputAction).Forget();

            if (SidePrediction)
            {
                var playerData    = playerDataList.Find(d => d.ID.Equals(inputAction.PlayerID));
                var ballBehaviour = GetPlayerByID(playerData.ID).GetComponent<BallBehaviour>();
                ballBehaviour.MoveX(x);
            }

            inputList.Add(inputAction);
        }

    #endregion
    }
}