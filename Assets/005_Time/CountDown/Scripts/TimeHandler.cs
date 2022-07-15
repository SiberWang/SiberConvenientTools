using System;
using System.Collections.Generic;
using rStarUtility.Util.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO:假設我有 3個時間需求
// A = 現在時間
// B = 碼表(可停止在開始計算上去)
// C = 倒數計時(可指定時間)
public class TimeHandler : MonoBehaviour
{
#region ========== Private Variables ==========

    [Header("[Clock Setting]")]
    [SerializeField] private TMP_Text textClockTime;
    [SerializeField] private TMP_Text textClockTimeDone;
    [SerializeField] private Button   startClockButton;
    [SerializeField] private Button   stopClockButton;
    [Header("[CountDown(Sec) Setting]")]
    [SerializeField] private TMP_InputField inputFieldTime;
    [SerializeField] private TMP_Text textCountDownTime;
    [SerializeField] private TMP_Text textCountDownDone;
    [SerializeField] private Button   startCountDownButton;
    [SerializeField] private Button   stopCountDownButton;
    [Header("[CountDown(Date) Setting]")]
    [SerializeField] private TMP_InputField inputFieldBeginTime;
    [SerializeField] private TMP_InputField inputFieldEndTime;
    [SerializeField] private TMP_Text       textCountDownDateTime;
    [SerializeField] private TMP_Text       textCountDownDateDone;
    [SerializeField] private Button         startCountDownDateButton;
    [SerializeField] private Button         stopCountDownDateButton;

    [Header("[Stopwatch Setting]")]
    [SerializeField] private TMP_Text textStopwatchTime;
    [SerializeField] private Button startStopwatchButton;
    [SerializeField] private Button stopStopwatchButton;
    [SerializeField] private Button pauseStopwatchButton;
    [SerializeField] private Button resumeStopwatchButton;

    [Header("[All Setting Button]")]
    [SerializeField] private Button startAllButton;
    [SerializeField] private Button stopAllButton;
    [SerializeField] private Button resetAllButton;
    [SerializeField] private Slider timeScaleSlider;

    private List<Button>       buttonList;
    private ClockTimer         clockTimer;
    private CountDownTimer     countDownTimer;
    private CountDownDateTimer countDownDateTimer;

    private DateTime       endDateTime;
    private DateTime       beginDateTime;
    private StopWatchTimer stopWatchTimer;

    private static string Str_Done = $"<color=green>[Done!]</color>";
    private static string Str_Tick = $"<color=yellow>[Tick]</color>";
    private static string Str_Stop = $"<color=red>[Stop]</color>";

#endregion

#region ========== Events ==========

    private void Start()
    {
        InitUI();
        CreateTimers();
        BindButtons();
    }

    private void Update()
    {
        Time.timeScale = timeScaleSlider.value;
    }

#endregion

#region ========== Private Methods ==========

    private void CreateTimers()
    {
        clockTimer = new ClockTimer
            (onStart: () => textClockTimeDone.text     = Str_Tick,
             onStop: () => textClockTimeDone.text      = Str_Stop,
             onTick: currentTime => textClockTime.text = DisplayHourTime(currentTime));

        countDownTimer = new CountDownTimer
            (onStart: () => countDownTimer.SetDuration(inputFieldTime.text),
             onStop: () => textCountDownDone.text = Str_Stop,
             onTick: currentTime =>
             {
                 textCountDownTime.text = DisplayHourTime(currentTime, true);
                 textCountDownDone.text = Str_Tick;
             },
             onComplete: () => textCountDownDone.text = Str_Done);

        countDownDateTimer = new CountDownDateTimer
            (onStart: () => countDownDateTimer.SetCountDown(inputFieldBeginTime.text, inputFieldEndTime.text),
             onStop: () => textCountDownDateDone.text = Str_Stop,
             onTick: currentTime =>
             {
                 textCountDownDateTime.text = DisplayDayTime(currentTime);
                 textCountDownDateDone.text = Str_Tick;
             },
             onComplete: () => textCountDownDateDone.text = Str_Done);

        stopWatchTimer = new StopWatchTimer
            (currentTime => textStopwatchTime.text = DisplayHourTime(currentTime, true));
    }

    private void BindButtons()
    {
        // Clock
        startClockButton.BindClick(clockTimer.Start);
        stopClockButton.BindClick(clockTimer.Stop);

        // CountDown(Sec)
        startCountDownButton.BindClick(countDownTimer.Start);
        stopCountDownButton.BindClick(countDownTimer.Stop);

        // CountDown(Date)
        startCountDownDateButton.BindClick(countDownDateTimer.Start);
        stopCountDownDateButton.BindClick(countDownDateTimer.Stop);

        // Stopwatch
        startStopwatchButton.BindClick(stopWatchTimer.Start);
        stopStopwatchButton.BindClick(stopWatchTimer.Stop);
        pauseStopwatchButton.BindClick(stopWatchTimer.Pause);
        resumeStopwatchButton.BindClick(stopWatchTimer.Resume);

        // All
        startAllButton.BindClick(OnStartAll);
        stopAllButton.BindClick(OnStopAll);
        resetAllButton.BindClick(() =>
        {
            OnStopAll();
            InitUI();
        });
    }

    private void OnStartAll()
    {
        clockTimer.Start();
        countDownTimer.Start();
        countDownDateTimer.Start();
        stopWatchTimer.Start();
    }

    private void OnStopAll()
    {
        clockTimer.Stop();
        countDownTimer.Stop();
        countDownDateTimer.Stop();
        stopWatchTimer.Stop();
    }

    /// <summary> 轉為 DDD-HH-MM-SS(-FF) </summary>
    private string DisplayDayTime(TimeSpan currentTime, bool showMilliseconds = false)
    {
        var days  = currentTime.Days > 0 ? currentTime.Days.ToString() : "00";
        var hours = currentTime.Hours > 0 ? currentTime.Hours.ToString() : "00";
        return $"{days}:{hours}:{DisplayMinutesTime(currentTime, showMilliseconds)}";
    }

    /// <summary> 轉為 HH-MM-SS(-FF) </summary>
    private string DisplayHourTime(TimeSpan currentTime, bool showMilliseconds = false)
    {
        var timeHours = currentTime.Days * 24 + currentTime.Hours;
        var hours = timeHours switch
        {
            > 0 and < 10 => $"0{timeHours}",
            > 10         => $"{timeHours}",
            _            => "00"
        };

        return $"{hours}:{DisplayMinutesTime(currentTime, showMilliseconds)}";
    }

    /// <summary> 轉為 MM-SS(-FF) </summary>
    private string DisplayMinutesTime(TimeSpan currentTime, bool showMilliseconds = false)
    {
        var minutes = showMilliseconds ? $@"{currentTime:mm\:ss\:ff}" : $@"{currentTime:mm\:ss}";
        return $"{minutes}";
    }

    private void InitUI()
    {
        textClockTimeDone.text     = "";
        textCountDownDone.text     = "";
        textCountDownDateDone.text = "";

        textClockTime.text         = "00:00:00";
        textCountDownTime.text     = "00:00:00:00";
        textCountDownDateTime.text = "00:00:00:00";
        textStopwatchTime.text     = "00:00:00:00";

        inputFieldTime.text      = "30";
        inputFieldBeginTime.text = "2022-07-15";
        inputFieldEndTime.text   = "2022-08-15";
    }

#endregion
}