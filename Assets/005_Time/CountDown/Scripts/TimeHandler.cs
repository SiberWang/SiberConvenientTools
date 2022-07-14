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
    [SerializeField] private TMP_Text textCountDownTime;
    [SerializeField] private Button startCountDownButton;
    [SerializeField] private Button stopCountDownButton;
    [SerializeField] private int    minute = 10;
    [SerializeField] private int    second = 0;
    [Header("[Stopwatch Setting]")]
    [SerializeField] private TMP_Text textStopwatchTime;
    [SerializeField] private Button startStopwatchButton;
    [SerializeField] private Button stopStopwatchButton;

    [Header("[All Setting Button]")]
    [SerializeField] private Button stopAllButton;

    private ClockTimer  clockTimer;
    private List<ITime> timeList = new List<ITime>();

#endregion

#region ========== Events ==========

    private void Start()
    {
        CreateSomeTimer();
        BindButtons();
    }

    private void CreateSomeTimer()
    {
        clockTimer = new ClockTimer();
        clockTimer.Init(UpdateClockTimeUI);

        timeList.Add(clockTimer);
    }

    private void BindButtons()
    {
        startClockButton.BindClick(clockTimer.Play);
        stopClockButton.BindClick(clockTimer.Stop);

        stopAllButton.BindClick(StopAll);
    }

#endregion

#region ========== Public Methods ==========

#endregion

#region ========== Private Methods ==========

    private void StopAll()
    {
        for (int i = 0; i < timeList.Count; i++)
        {
            timeList[i].Stop();
        }
    }

    /// <summary> 更新 ClockTime UI </summary>
    private void UpdateClockTimeUI(TimeSpan currentTime)
    {
        textClockTime.text = $"{currentTime.Hours}:{currentTime:mm\\:ss}";
    }

#endregion
}