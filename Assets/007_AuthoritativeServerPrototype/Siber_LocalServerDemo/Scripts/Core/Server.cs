using Cysharp.Threading.Tasks;
using LocalServerDemo.Repositorys;
using UnityTimer;

namespace LocalServerDemo.Scripts
{
    public class Server
    {
    #region ========== delegate ==========

        public delegate void ServerReceive(EventArgs eventArgs);

        public delegate void ServerUpdate(EventArgs eventArgs);

        private ServerReceive serverReceiveDelegate; // Server Receive 回傳回覆事件
        private ServerUpdate  serverUpdateDelegate;  // Server 同步更新事件

    #endregion

    #region ========== Public Variables ==========

        public UserRepository UserRepository => userRepository;

    #endregion

    #region ========== Private Variables ==========

        private UpdateTimer      updateTimer;
        private UserRepository   userRepository;
        private ClientRepository clientRepository;

    #endregion

    #region ========== Constructor ==========

        public Server()
        {
            userRepository   = new UserRepository();
            updateTimer      = new UpdateTimer(Tick);
            clientRepository = ClientRepository.Instance;
            AddUpdateCallBack(OnUpdateServerView);
        }

    #endregion

    #region ========== Public Methods ==========

        /// <summary> 增加回傳事件 </summary>
        /// <param name="someDelegate"> 要放入的方法 </param>
        public void AddReceiveCallBack(ServerReceive someDelegate)
        {
            serverReceiveDelegate += someDelegate;
        }

        /// <summary> 增加Server更新事件 </summary>
        /// <param name="someDelegate"> 要放入的方法 </param>
        public void AddUpdateCallBack(ServerUpdate someDelegate)
        {
            serverUpdateDelegate += someDelegate;
        }

        public async UniTask Send(int lag, EventArgs eventArgs)
        {
            var client = clientRepository.GetClient(eventArgs.ClientID);
            await client.NetWork.Send(lag, eventArgs);
            if (eventArgs is InputEvent inputEvent)
                CatchYouBug.DeShow($"Send from Client: [{inputEvent.ClientID}]", "Server");
        }

        /// <summary> 更新率 </summary>
        public void SetUpdateTimes(float serverUpdateTimes)
        {
            updateTimer.Cancel();
            updateTimer.SetNewUpdateTimes(serverUpdateTimes);
        }

    #endregion

    #region ========== Private Methods ==========

        private void Tick()
        {
            var clients = clientRepository.GetClients();
            for (int i = 0; i < clients.Count; i++)
            {
                var eventArgs = clients[i].NetWork.Receive();
                if (eventArgs == null) continue;
                serverUpdateDelegate?.Invoke(eventArgs);  // 優先更新 Server 的角色位置顯示
                serverReceiveDelegate?.Invoke(eventArgs); // 更新主客戶端 -> 其他客戶端顯示
            }
        }

    #endregion

    #region ========== Events ==========
        
        private void OnUpdateServerView(EventArgs eventArgs)
        {
            UpdateTestView(eventArgs);
            // netWork.ActionList.Clear(); // 這邊註解掉，就可以讓接收到的事件，依依的完成為止
        }

        /// <summary> 更新 Server 顯示用的玩家 </summary>
        private void UpdateTestView(EventArgs eventArgs)
        {
            if (eventArgs is InputEvent inputEvent)
            {
                var player   = userRepository.GetComponent(inputEvent.PlayerID);
                var userData = userRepository.GetUserData(inputEvent.PlayerID);
                userData.Move(inputEvent.Pos);
                player.Move(userData.Pos);
                MediumManager.Instance.SetLastInputText(inputEvent.ClientID, inputEvent.inputNumber);
                CatchYouBug.DeShow($"Receive to Client: [{inputEvent.ClientID}]", "Server");
            }
        }

    #endregion
    }
}