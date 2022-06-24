using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class KeepClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
#region ========== Public Variables ==========

    [Header("[頻率設定]")] [Tooltip("多久執行一次")]
    [SerializeField] private float frequency = 1f;
    [Header("[速度設定]")] [Tooltip("速度最大值")]
    [SerializeField] private float maxSpeed = 2.5f;

    private float speed = 1f;

    public UnityEvent OnKeepClick;

#endregion

#region ========== Private Variables ==========

    private Button button;

    private bool  isButtonDown;
    private float triggerTime;  // 觸發時間
    private float acceleration; // 加速度

#endregion

#region ========== Events ==========

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        triggerTime  = 0;
        acceleration = 0f;
        isButtonDown = true;
        OnKeepClick?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        OnStop();
    }

    /// <summary> 給 Back 註冊</summary>
    // 會發生按住然後旁邊放開失敗的情況，因此需要有強制停止的動作 
    public void OnStop()
    {
        isButtonDown = false;
        triggerTime  = 0;
        acceleration = 0f;
    }

#endregion

#region ========== Private Methods ==========

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void FixedUpdate()
    {
        if (isButtonDown)
        {
            if (!button.interactable) OnStop(); //按鈕關閉後，會發生OnPointerUp失效的問題
            if (acceleration < maxSpeed)        // 加速度
                acceleration += speed * 0.1f;
            triggerTime += acceleration;
            if (triggerTime > frequency)
            {
                OnKeepClick?.Invoke();
                triggerTime = 0;
            }
        }
    }

#endregion
}