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
            DoMove();
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
        private void DoMove()
        {
            var moveLeftRight = controlSystem.MoveLeftRight();
            var moveUpDown    = controlSystem.MoveUpDown();
            if (moveLeftRight == 0 && moveUpDown == 0) return;
            var newPos = MovePos(moveLeftRight, moveUpDown);

            // 發送事件次數
            inputNumber++;
            SendToServer_Move(MainPlayerID, newPos);

            // 客戶端預測 (不等 Server，先走！)
            if (SidePrediction)
            {
                var player = userRepository.GetComponent(MainPlayerID);
                player.Move(player.transform.position + newPos);
            }
        }

        private void SendToServer_Move(string playerID, Vector2 pos)
        {
            var inputAction = new InputEvent();
            inputAction.ClientID    = clientID;
            inputAction.PlayerID    = playerID;
            inputAction.CacheTime   = new CacheTime();
            inputAction.inputNumber = inputNumber;
            inputAction.Pos         = pos;
            inputList.Add(inputAction); // 客戶端暫存
            server.Send(lag, inputAction).Forget();
        }

        private Vector3 MovePos(float moveLeftRight, float moveUpDown)
        {
            var direction = new Vector2(moveLeftRight, moveUpDown);
            direction.Normalize();
            return direction * 100f * Time.deltaTime;
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
                userData.Move(inputEvent.Pos);
                player.Move(userData.Pos);
                Debug.Log($"Receive_Move : inputEvent.X:[{inputEvent.Pos}]");
            }
        }

    #endregion
    }
}