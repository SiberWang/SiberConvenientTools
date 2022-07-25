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

        private UpdateTimer    updateTimer;
        private NetWork        netWork;
        private UserRepository userRepository;

    #endregion

    #region ========== Constructor ==========

        public Server()
        {
            netWork        = new NetWork();
            userRepository = new UserRepository();
            updateTimer    = new UpdateTimer(Tick);
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
            await netWork.Send(lag, eventArgs);
            if (eventArgs is InputEvent inputEvent)
                CatchYouBug.DeShow($"Get from ClientID:[{inputEvent.ClientID}]", "Server");
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
            var inputAction = netWork.Receive();
            if (inputAction == null) return;
            serverUpdateDelegate?.Invoke(inputAction);  // 優先更新 Server 的角色位置顯示
            serverReceiveDelegate?.Invoke(inputAction); // 更新主客戶端 -> 其他客戶端顯示
        }

    #endregion

    #region ========== Events ==========

        private void OnUpdateServerView(EventArgs eventArgs)
        {
            netWork.ActionList.Clear();
            UpdateTestView(eventArgs);
        }

        /// <summary> 更新 Server 顯示用的玩家 </summary>
        private void UpdateTestView(EventArgs eventArgs)
        {
            if (eventArgs is InputEvent inputEvent)
            {
                var unityComponent = userRepository.GetComponent(inputEvent.PlayerID);
                unityComponent.MoveX(inputEvent.X);
                MediumManager.Instance.SetLastInputText(inputEvent.ClientID, inputEvent.inputNumber);
                CatchYouBug.DeShow($"Receive to ClientID:{inputEvent.ClientID}");
            }
        }

    #endregion
    }
}