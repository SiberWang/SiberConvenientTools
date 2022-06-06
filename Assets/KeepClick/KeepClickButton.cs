using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class KeepClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
#region ========== Public Variables ==========

    [Header("[頻率設定]")]
    [Tooltip("多久執行一次")]
    [SerializeField]
    private float frequency = 1f;

    [Header("[速度設定]")]
    [Tooltip("速度最大值")]
    [SerializeField]
    private float maxSpeed = 2.5f;

    [Tooltip("速度")]
    [SerializeField]
    private float speed = 1.5f;

    public UnityEvent OnKeepClick;

#endregion

#region ========== Private Variables ==========

    private Button button;

    private float time;
    private bool  isClick;
    private float acceleration;

#endregion

#region ========== Events ==========

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        Debug.Log("OnPointerDown");
        time         = 0;
        acceleration = 0f;
        isClick      = true;
        OnKeepClick?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        Debug.Log("OnPointerUp");
        OnStop();
    }

    public void OnStop()
    {
        isClick      = false;
        time         = 0;
        acceleration = 0f;
    }

#endregion

#region ========== Private Methods ==========

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (isClick)
        {
            // 如果按到底，按鈕關閉後，會發生OnPointerUp失效的問題
            // 所以在這邊需要做停止
            if (!button.interactable) OnStop();

            // 加速
            if (acceleration < maxSpeed)
                acceleration += Time.deltaTime * speed;

            // CD
            time += acceleration * 0.1f;

            // 執行
            if (time > frequency)
            {
                OnKeepClick?.Invoke();
                Debug.Log($"OnPointerDown Click: {speed}");
                time = 0;
            }

            Debug.Log($"{time}");
        }
    }

#endregion
}