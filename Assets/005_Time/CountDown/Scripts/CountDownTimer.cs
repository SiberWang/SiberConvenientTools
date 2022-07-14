using System;

public class CountDownTimer : ITime
{
#region ========== Public Methods ==========

    public void Init(Action<TimeSpan> tickAction)
    {
        throw new NotImplementedException();
    }

    public void Play() { }

    public void Stop() { }

#endregion
}