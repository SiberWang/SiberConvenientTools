using System;
using System.Collections.Generic;
using UnityEngine;

namespace _007_AuthoritativeServerPrototype.Siber_LocalServerDemo.Scripts
{
    public class NetWork
    {
        private List<PendingData> dataList = new List<PendingData>(); // 紀錄要執行的事件

        public void Send(int ms, InputAction inputAction)
        {
            var now      = inputAction.CacheTime.GetTime();
            var doAction = new PendingData();
            doAction.ms          = now + ms;
            doAction.InputAction = inputAction;
            dataList.Add(doAction);
            Debug.Log($"NewWorkCenter send : {doAction.ms} , ms: {ms}");
        }

        public IAction Receive()
        {
            var now = DateTime.Now.Millisecond;
            for (var i = 0; i < dataList.Count; i++)
            {
                if (dataList[i].ms > now) continue;
                dataList.RemoveAt(i);
                return dataList[i].InputAction;
            }

            return null;
        }
    }

    public class PendingData
    {
        public int         ms;
        public InputAction InputAction;
    }
}