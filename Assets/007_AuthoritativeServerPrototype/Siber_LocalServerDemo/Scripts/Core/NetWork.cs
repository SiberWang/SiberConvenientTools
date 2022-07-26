using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace LocalServerDemo.Scripts
{
    public class NetWork
    {
    #region ========== Public Variables ==========

        public List<EventArgs> ActionList => actionList;

    #endregion

    #region ========== Private Variables ==========

        private List<EventArgs> actionList = new List<EventArgs>(); // 紀錄要執行的事件

    #endregion

    #region ========== Public Methods ==========

        /// <summary> 發送事件 </summary>
        /// <param name="lag">網路延遲</param>
        /// <param name="eventArgs">特定事件</param>
        public async UniTask Send(int lag, EventArgs eventArgs)
        {
            await UniTask.Delay(lag / 2);
            actionList.Add(eventArgs);
            // CatchYouBug.DeShow($"Send DelayTime [{lag / 2}] ms , ActionList :{actionList.Count}", "NetWork[Send]");
        }

        /// <summary> 回覆結果 </summary>
        /// <returns>特定事件</returns>
        public EventArgs Receive()
        {
            for (var i = 0; i < actionList.Count; i++)
            {
                var inputAction = actionList[i];
                actionList.RemoveAt(i);
                // CatchYouBug.DeShow($"Receive Action[{i}] , ActionList :{actionList.Count}", "NetWork<Receive>");
                return inputAction;
            }

            return null;
        }

    #endregion
    }
}