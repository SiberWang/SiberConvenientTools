using System;
using UnityTimer;

public class CountDownTimer : ITime
{
#region ========== Protected Variables ==========

    protected float duration;

#endregion

#region ========== Private Variables ==========

    private Action           onStart;
    private Action           onStop;
    private Action           onComplete;
    private Action<TimeSpan> onTick;

    private Timer timer;

#endregion

#region ========== Constructor ==========

    public CountDownTimer(Action onStart, Action onStop, Action<TimeSpan> onTick, Action onComplete)
    {
        this.onStop     = onStop;
        this.onStart    = onStart;
        this.onTick     = onTick;
        this.onComplete = onComplete;
    }

#endregion

#region ========== Public Methods ==========

    public void Start()
    {
        Stop();
        onStart?.Invoke();
        timer = Timer.Register
            (
             duration: duration,
             onComplete: onComplete,
             onUpdate: sec =>
             {
                 var      remaining = timer.GetTimeRemaining(); // 倒數
                 TimeSpan timeSpan  = TimeSpan.FromSeconds(remaining);
                 onTick?.Invoke(timeSpan);
             },
             isLooped: false,
             useRealTime: false
            );
    }

    public void SetDuration(float duration)
    {
        this.duration = duration;
    }

    public void SetDuration(string durationValue)
    {
        duration = float.TryParse(durationValue, out var time) ? time : 10f;
        if (duration > 3600000) duration = 3600000; // 1000 小時
    }

    public void Stop()
    {
        Timer.Cancel(timer);
        onStop?.Invoke();
    }

#endregion
}