using System.Collections.Generic;
using LocalServerDemo.Repositorys;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LocalServerDemo.Scripts
{
    /// <summary>
    /// 處理 A , B 角色 的動作
    /// </summary>
    public class Client
    {
    #region ========== Public Variables ==========

        /// <summary> 客戶端預測 Client-side prediction </summary>
        public bool SidePrediction;

        public int              Lag            => lag;
        public string           ClientID       => clientID;
        public List<InputEvent> InputList      => inputList;
        public UserRepository   UserRepository => userRepository;

        public string MainPlayerID { get; set; }

    #endregion

    #region ========== Private Variables ==========

        private List<InputEvent> inputList = new List<InputEvent>();

        private Dictionary<string, GameObject> playerDict = new Dictionary<string, GameObject>();

        private ControlSystem  controlSystem;
        private UserRepository userRepository;
        private Server         server;

        private string clientID;
        private int    inputNumber;
        private int    lag; //ms
        private ulong  updateRate;
        private ulong  nextUpdateTime;

    #endregion

    #region ========== Constructor ==========

        public Client(string clientID, Server server, ControlSystem controlSystem)
        {
            this.clientID      = clientID;
            this.server        = server;
            this.controlSystem = controlSystem;
            updateRate         = 60; // 預設更新率為60幀
            userRepository     = new UserRepository();

            MediumManager.Instance.AddViewCallBack(Tick);
            this.server.AddReceiveCallBack(args => Receive_Move(args).Forget());
        }

    #endregion

    #region ========== Public Methods ==========

        public void Tick()
        {
            var now = CacheTime.Now();
            if (now < nextUpdateTime) return;
            if (controlSystem.MoveLeft()) DoMove(MainPlayerID, -1);
            else if (controlSystem.MoveRight()) DoMove(MainPlayerID, 1);
            nextUpdateTime = now + updateRate;
        }

        /// <summary> 網路延遲設定 </summary>
        public void SetLag(int value)
        {
            lag = value;
        }

    #endregion

    #region ========== Private Methods ==========

        //TODO: 修改做法：判斷左邊右邊，位移量固定
        // 30FPS = 往右邊
        private void DoMove(string playerID, float x)
        {
            inputNumber++; // 發送事件次數
            SendToServer_Move(playerID, x);

            // 客戶端預測 (不等 Server，先走 )
            if (SidePrediction)
            {
                var player = userRepository.GetComponent(playerID);
                player.MoveX(x);
            }
        }

        private void SendToServer_Move(string playerID, float x)
        {
            var inputAction = new InputEvent();
            inputAction.ClientID    = clientID;
            inputAction.PlayerID    = playerID;
            inputAction.CacheTime   = new CacheTime();
            inputAction.inputNumber = inputNumber;
            inputAction.X           = x;
            inputList.Add(inputAction); // 客戶端暫存
            server.Send(lag, inputAction).Forget();
        }

    #endregion

    #region ========== Events ==========

        /// <summary> 回覆事件(Move) </summary>
        private async UniTask Receive_Move(EventArgs args)
        {
            if (args is InputEvent inputEvent)
            {
                inputList.Clear();
                await UniTask.Delay(lag / 2);
                var player   = userRepository.GetComponent(inputEvent.PlayerID);
                var userData = userRepository.GetUserData(inputEvent.PlayerID);
                userData.MoveX(inputEvent.X);
                player.SetPos(userData.Pos);
                Debug.Log($"Receive_Move : inputEvent.X:[{inputEvent.X}]");
            }
        }

    #endregion
    }
}