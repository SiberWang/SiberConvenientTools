using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
        private readonly List<PlayerData> playerDataList;

        private Dictionary<string, GameObject> playerDict = new Dictionary<string, GameObject>();

    #endregion

    #region ========== Constructor ==========

        public Server()
        {
            netWork        = new NetWork();
            clientList     = new List<Client>();
            playerDataList = new List<PlayerData>();
        }

    #endregion

    #region ========== Public Methods ==========

        public void AddClient(Client clientID)
        {
            clientList.Add(clientID);
        }

        public void AddPlayer(PlayerData playerData, GameObject gameObject)
        {
            playerDict.Add(playerData.ID, gameObject);
            playerDataList.Add(playerData);
        }

        public async void Tick()
        {
            await ReplyResult();
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

        /// <summary> 回覆事件結果 </summary>
        private async UniTask ReplyResult()
        {
            var inputAction = netWork.Receive();
            if (inputAction == null) return;

            MoveServerPlayer(inputAction);        // 更新 Server 的顯示
            await ReceiveClient(inputAction);     // 傳回 主要接收的 Client (延遲) 更新顯示
            await UpdateOtherClient(inputAction); // 傳回 其他的 Clients (延遲) 更新顯示
        }

        private async UniTask ReceiveClient(InputAction inputAction)
        {
            var client = clientList.Find(c => c.ClientID.Equals(inputAction.ClientID));
            client.InputList.Clear();
            netWork.ActionList.Clear();
            await UniTask.Delay(client.Lag / 2);
            MoveClientPlayer(client, inputAction);
            CatchYouBug.DeShow($"Receive DelayTime :[{client.Lag / 2}] ms", $"Receive to ClientID:{inputAction.ClientID}");
        }

        private async UniTask UpdateOtherClient(InputAction inputAction)
        {
            // 可能就 N 個客戶端
            var otherClients = clientList.Where(c => !c.ClientID.Equals(inputAction.ClientID)).ToList();
            for (int i = 0; i < otherClients.Count; i++)
            {
                await UniTask.Delay(otherClients[i].Lag / 2);
                MoveClientPlayer(otherClients[i], inputAction);
                // CatchYouBug.DeShow($"Update ClientID:{otherClients[i].ClientID} DelayTime :[{otherClients[i].Lag}] ms",
                //                    $"Receive From Server , ClientID:{inputAction.ClientID}");
            }
        }

        private void MoveServerPlayer(InputAction inputAction)
        {
            Debug.Log($"inputAction.inputNumber:{inputAction.inputNumber}");
            var playerData = playerDataList.Find(d => d.ID.Equals(inputAction.PlayerID));
            playerData.Pos.x += inputAction.X;
            var ballBehaviour = GetPlayerByID(playerData.ID).GetComponent<BallBehaviour>();
            ballBehaviour.SetPos(playerData.Pos);
            MediumManager.Instance.SetLastInputText(inputAction.ClientID, inputAction.inputNumber);
        }

        private void MoveClientPlayer(Client client, InputAction inputAction)
        {
            var playerData = client.PlayerDataList.Find(d => d.ID.Equals(inputAction.PlayerID));
            playerData.Pos.x += inputAction.X;
            var ballBehaviour = client.GetPlayerByID(playerData.ID).GetComponent<BallBehaviour>();
            ballBehaviour.SetPos(playerData.Pos);
        }

    #endregion
    }
}