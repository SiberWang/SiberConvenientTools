using System;

public interface ITime
{
#region ========== Public Methods ==========

    void Init(Action<TimeSpan> tickAction);
    void Play();
    void Stop();

#endregion
}