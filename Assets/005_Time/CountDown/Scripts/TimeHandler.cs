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
    [SerializeField] private Button startClockButton;
    [SerializeField] private Button stopClockButton;
    [Header("[CountDown Setting]")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text textCountDownTime;
    [SerializeField] private TMP_Text textCountDownDone;
    [SerializeField] private Button   startCountDownButton;
    [SerializeField] private Button   stopCountDownButton;
    [SerializeField] private string   endTimeValue   = "2022-07-15";
    [SerializeField] private string   beginTimeValue = "2022-07-14";
    [Header("[Stopwatch Setting]")]
    [SerializeField] private TMP_Text textStopwatchTime;
    [SerializeField] private Button startStopwatchButton;
    [SerializeField] private Button stopStopwatchButton;

    [Header("[All Setting Button]")]
    [SerializeField] private Button startAllButton;
    [SerializeField] private Button stopAllButton;
    [SerializeField] private Button resetAllButton;

    private ClockTimer     clockTimer;
    private CountDownTimer countDownTimer;
    private List<ITime>    timeList = new List<ITime>();

    private DateTime endDateTime;
    private DateTime beginDateTime;

#endregion

#region ========== Events ==========

    private void Start()
    {
        InitUI();
        CreateSomeTimer();
        BindButtons();
    }

    private void CreateSomeTimer()
    {
        clockTimer     = new ClockTimer();
        countDownTimer = new CountDownTimer();

        clockTimer.Init(UpdateClockTimeUI);
        countDownTimer.Init(UpdateCountDownTimeUI, CompleteCountDownTimeUI);

        // 時間換算 : string => DateTime
        endDateTime   = TurnDateTime(endTimeValue);
        beginDateTime = TurnDateTime(beginTimeValue);

        timeList.Add(clockTimer);
        timeList.Add(countDownTimer);
    }

    private DateTime TurnDateTime(string timeValue)
    {
        if (DateTime.TryParse(timeValue, out var endTime))
            return endTime;

        CatchYouBug.Debug("You need to input YYYY-MM-DD");
        return new DateTime(99, 01, 01);
    }

    private void BindButtons()
    {
        startClockButton.BindClick(clockTimer.Play);
        stopClockButton.BindClick(clockTimer.Stop);
        startCountDownButton.BindClick(()=>
        {
            var duration = float.TryParse(inputField.text, out var time) ? time : 10f;
            countDownTimer.SetDuration(duration);
            countDownTimer.Play();
        });
        stopCountDownButton.BindClick(countDownTimer.Stop);
        startAllButton.BindClick(StartAll);
        stopAllButton.BindClick(StopAll);
        resetAllButton.BindClick(() =>
        {
            StopAll();
            InitUI();
        });
    }

#endregion

#region ========== Public Methods ==========

#endregion

#region ========== Private Methods ==========

    private void InitUI()
    {
        textClockTime.text     = "0:00:00";
        textCountDownTime.text = "0:00:00";
        textCountDownDone.text = "";
    }

    private void StartAll()
    {
        foreach (var timer in timeList)
            timer.Play();
    }

    private void StopAll()
    {
        foreach (var timer in timeList)
            timer.Stop();
    }

    /// <summary> 更新 ClockTime UI </summary>
    private void UpdateClockTimeUI(TimeSpan currentTime)
    {
        textClockTime.text = $"{currentTime.Days * 24 + currentTime.Hours}:{currentTime:mm\\:ss}";
    }

    /// <summary> 更新 CountDownTime UI - Tick </summary>
    private void UpdateCountDownTimeUI(TimeSpan currentTime)
    {
        textCountDownTime.text = $"{currentTime.Days * 24 + currentTime.Hours}:{currentTime:mm\\:ss}";
        textCountDownDone.text = "[Tick]";
    }

    /// <summary> 更新 CountDownTime UI - Complete </summary>
    private void CompleteCountDownTimeUI()
    {
        textCountDownDone.text = "[Done!]";
    }

#endregion
}