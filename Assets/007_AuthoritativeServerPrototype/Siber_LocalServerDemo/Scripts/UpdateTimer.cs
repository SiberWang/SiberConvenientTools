using System;
using UnityTimer;

namespace LocalServerDemo.Scripts
{
    public class UpdateTimer
    {
        private Timer  serverTimer;
        private Action callBack;


        public UpdateTimer(Action callBack)
        {
            this.callBack = callBack;
        }

        public void SetNewUpdateTimes(float serverUpdateTimes)
        {
            if (serverTimer != null) serverTimer.Cancel();
            // 每秒更新 N 次
            serverTimer = Timer.Register
                (
                 duration: 1f / serverUpdateTimes,
                 onComplete: callBack,
                 isLooped: true
                );
        }

        public void Cancel()
        {
            if (serverTimer != null)
                serverTimer.Cancel();
        }
    }
}