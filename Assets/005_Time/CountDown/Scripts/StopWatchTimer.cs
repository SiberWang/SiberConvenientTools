using System;
using UnityTimer;

public class StopWatchTimer : ITime
{

#region ========== Private Variables ==========

    private Action           completeAction;
    private Action<TimeSpan> tickAction;
    private Timer            timer;

#endregion

#region ========== Constructor ==========

    public StopWatchTimer(Action<TimeSpan> tickAction)
    {
        this.tickAction = tickAction;
    }

#endregion

#region ========== Public Methods ==========

    public void Pause()
    {
        Timer.Pause(timer);
    }

    public void Start()
    {
        Stop();
        timer = Timer.Register
            (
             duration: 3600000,
             onComplete: completeAction,
             onUpdate: sec =>
             {
                 timer.GetTimeElapsed();
                 TimeSpan timeSpan = TimeSpan.FromSeconds(sec);
                 tickAction?.Invoke(timeSpan);
             },
             isLooped: false,
             useRealTime: false
            );
    }

    public void Resume()
    {
        Timer.Resume(timer);
    }

    public void Stop()
    {
        Timer.Cancel(timer);
    }

#endregion
}