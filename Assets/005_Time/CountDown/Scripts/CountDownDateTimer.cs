using System;

public class CountDownDateTimer : CountDownTimer
{
#region ========== Private Variables ==========

    private DateTime beginTime;
    private DateTime endTime;

#endregion

#region ========== Constructor ==========

    public CountDownDateTimer(Action onStart, Action onStop, Action<TimeSpan> onTick, Action onComplete)
        : base(onStart, onStop, onTick, onComplete) { }

#endregion

#region ========== Public Methods ==========

    public void SetCountDown(DateTime beginTime, DateTime endTime)
    {
        this.beginTime = beginTime;
        this.endTime   = endTime;
        duration       = TurnDateTimeToSeconds(endTime - beginTime);
    }

    public void SetCountDown(string beginTimeValue, string endTimeValue)
    {
        beginTime = TurnDateTime(beginTimeValue);
        endTime   = TurnDateTime(endTimeValue);
        duration  = TurnDateTimeToSeconds(endTime - beginTime);
    }

#endregion

#region ========== Private Methods ==========

    /// <summary> 時間換算(string) </summary>
    /// <remarks> YYYY-MM-DD </remarks>
    private DateTime TurnDateTime(string timeValue)
    {
        if (DateTime.TryParse(timeValue, out var endTime))
            return endTime;

        CatchYouBug.Debug("You need to input YYYY-MM-DD");
        return new DateTime(99, 01, 01);
    }

    /// <summary> 時間換算(秒) </summary>
    private float TurnDateTimeToSeconds(TimeSpan timeSpan)
    {
        TimeSpan time    = timeSpan;
        float    seconds = (float)time.TotalSeconds;
        return seconds;
    }

#endregion
}