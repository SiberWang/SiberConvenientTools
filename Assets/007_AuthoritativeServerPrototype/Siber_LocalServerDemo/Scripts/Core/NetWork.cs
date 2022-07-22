using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class NetWork
    {
    #region ========== Public Variables ==========

        public List<InputAction> ActionList => actionList;

    #endregion

    #region ========== Private Variables ==========

        private List<InputAction> actionList = new List<InputAction>(); // 紀錄要執行的事件

    #endregion

    #region ========== Public Methods ==========

        public async UniTask Send(int lag, InputAction inputAction)
        {
            await UniTask.Delay(lag / 2); // 傳送延遲 ms
            actionList.Add(inputAction);  // 增加至 代辦清單
            CatchYouBug.DeShow($"Send DelayTime [{lag / 2}] ms , ActionList :{actionList.Count} , from ClientID:[{inputAction.ClientID}]",
                               "NetWork<Send>");
        }
        
        public InputAction Receive()
        {
            for (var i = 0; i < actionList.Count; i++)
            {
                var inputAction = actionList[i];
                actionList.RemoveAt(i);
                CatchYouBug.DeShow($"Receive Action[{i}] , ActionList :{actionList.Count}", "NetWork<Receive>");
                return inputAction;
            }

            return null;
        }

    #endregion
    }
}