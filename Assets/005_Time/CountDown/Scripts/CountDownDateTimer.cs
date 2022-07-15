using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CountDownDateTimer : ITime
{
    private bool isStartTick;
    private bool isTickTime;

    private Action<TimeSpan>        tickAction;
    private CancellationTokenSource cancelToken;

    private DateTime beginTime;
    private DateTime endTime;

#region ========== Public Methods ==========

    public void Init(Action<TimeSpan> tickAction)
    {
        this.tickAction = tickAction;
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

    public void SetCountDown(DateTime endDay, DateTime beginDay)
    {
        endTime   = endDay;
        beginTime = beginDay;
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
        var time = endTime - beginTime;
        return time;
    }

#endregion
}