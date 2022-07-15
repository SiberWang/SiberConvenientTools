using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ClockTimer : ITime
{
#region ========== Private Variables ==========

    private Action           onStart;
    private Action           onStop;
    private Action<TimeSpan> onTick;

    private bool                    isStartTick;
    private bool                    isTickTime;
    private CancellationTokenSource cancelToken;
    private DateTime                dateTime;

#endregion

#region ========== Constructor ==========

    public ClockTimer(Action onStart, Action onStop, Action<TimeSpan> onTick)
    {
        this.onStop  = onStop;
        this.onStart = onStart;
        this.onTick  = onTick;
        dateTime     = DateTime.Today;
    }

#endregion

#region ========== Unity Events ==========

    public void Start()
    {
        Tick().Forget();
        onStart?.Invoke();
    }

#endregion

#region ========== Public Methods ==========

    public void Stop()
    {
        onStop?.Invoke();
        isStartTick = false;
        if (cancelToken != null)
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
            cancelToken = null;
        }
    }

#endregion

#region ========== Private Methods ==========

    private async UniTaskVoid Tick()
    {
        if (isStartTick)
        {
            CatchYouBug.Debug($"isStartTick : {isStartTick} , you need to stop !", "ClockTimer");
            return;
        }

        isStartTick = true;
        cancelToken = new CancellationTokenSource();
        onTick?.Invoke(UpdateTime());

        while (isStartTick)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancelToken.Token);
            onTick?.Invoke(UpdateTime());
        }
    }

    private TimeSpan UpdateTime()
    {
        var time = DateTime.Now - dateTime;
        return time;
    }

#endregion
}