using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ClockTimer : ITime
{
    private bool     isStartTick;
    private bool     isTickTime;
    private DateTime dataTime;

    private Action<TimeSpan>        tickAction;
    private CancellationTokenSource cancelToken;

#region ========== Public Methods ==========

    public void Init(Action<TimeSpan> tickAction)
    {
        this.tickAction = tickAction;
        dataTime        = DateTime.Today;
    }

    public void Play()
    {
        Tick().Forget();
    }

    public void Stop()
    {
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
        tickAction?.Invoke(UpdateTime());

        while (isStartTick)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancelToken.Token);
            tickAction?.Invoke(UpdateTime());
        }
    }
    
    private TimeSpan UpdateTime()
    {
        var time = DateTime.Now - dataTime;
        return time;
    }

#endregion
}