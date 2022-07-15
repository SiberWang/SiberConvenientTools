using System;
using System.Threading;
using Timer = UnityTimer.Timer;

public class CountDownTimer : ITime
{
    private bool isStartTick;
    private bool isTickTime;

    private Action                  completeAction;
    private Action<TimeSpan>        tickAction;
    private CancellationTokenSource cancelToken;

    private DateTime beginTime;
    private DateTime endTime;

    private Timer timer;
    private float duration;

#region ========== Public Methods ==========

    public void Init(Action<TimeSpan> tickAction, Action completeAction)
    {
        this.tickAction     = tickAction;
        this.completeAction = completeAction;
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public void Play()
    {
        Stop();
        timer = Timer.Register
            (
             duration: duration,
             onComplete: completeAction,
             onUpdate: sec =>
             {
                 var      remaining = timer.GetTimeRemaining(); // 倒數
                 TimeSpan timeSpan  = TimeSpan.FromSeconds(remaining);
                 tickAction?.Invoke(timeSpan);
             },
             isLooped: false,
             useRealTime: false
            );
    }

    public void Stop()
    {
        Timer.Cancel(timer);
    }

#endregion
}