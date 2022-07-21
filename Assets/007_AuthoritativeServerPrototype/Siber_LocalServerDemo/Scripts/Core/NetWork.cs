using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class NetWork
    {
        private List<InputAction> actionList = new List<InputAction>(); // 紀錄要執行的事件

        public async UniTask Send(InputAction inputAction)
        {
            float delayTime = inputAction.Lag;
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
            actionList.Add(inputAction);
            Debug.Log($"actionList :{actionList.Count}");
        }

        public async UniTask<InputAction> Receive()
        {
            Debug.Log($"actionList :{actionList.Count}");
            for (var i = 0; i < actionList.Count; i++)
            {
                float delayTime = actionList[i].CacheTime.Sec;
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
                actionList.RemoveAt(i);
                return actionList[i];
            }

            return null;
        }
    }
}