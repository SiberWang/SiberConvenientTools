using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectSlider : MonoBehaviour, IPopPage, IBeginDragHandler, IEndDragHandler
{
#region ========== Private Variables ==========

    [SerializeField] private int        pageSize;
    [SerializeField] private float      smooting;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private List<GameObject> pageUIList = new List<GameObject>(); // 裝取pageItem
    [SerializeField] private List<Toggle>     toggleList = new List<Toggle>();     // 裝取pageItem's Toggle

    private bool  isDrag;           // 是否正在拖拉
    private float targetHorizontal; // 左右滑動的物件目標
    private int   pageCount;
    private int   sumRecord;
    private int   updateInfo; // 更新提示

    private List<float> pageArrayList = new List<float>();

#endregion

#region ========== Public Methods ==========

    ///<summary> 初始化一些參數 </summary>
    public void Init()
    {
        targetHorizontal                        = 0;
        scrollRect.horizontalNormalizedPosition = 0;

        sumRecord = toggleList.Count * toggleList.Count;
        pageCount = Convert.ToInt32(Math.Ceiling((decimal)sumRecord) / pageSize);
        
        int num = pageCount - 1;
        if (num == 0) { num = 1; }

        for (int i = 0; i < pageCount; i++)
        {
            float number = (float)i / (float)num;
            pageArrayList.Add(number);
        }

        ResetToIndex(0);
    }
    
    public void ResetToIndex(int index)
    {
        //重置第一頁
        CancelAllToggle(toggleList);
        if (toggleList.Count > 0)
            toggleList[index].isOn = true;
    }

#endregion

#region ========== Private Methods ==========

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (!isDrag)
            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetHorizontal, Time.deltaTime * smooting);
    }

    ///<summary> 關閉全按鈕 </summary>
    private void CancelAllToggle(List<Toggle> toggles)
    {
        foreach (var item in toggles)
            item.isOn = false;
    }

    ///<summary> 滑動運算觸發 </summary>
    private void SlidingOperation(int index)
    {
        var posX   = scrollRect.horizontalNormalizedPosition;
        var offset = Mathf.Abs(pageArrayList[index] - posX);
        for (int i = 1; i < pageArrayList.Count; i++)
        {
            float temp = Mathf.Abs(pageArrayList[i] - posX);
            if (temp < offset)
            {
                index = i;
                //保存當前位移量
                offset = temp;
            }
        }

        targetHorizontal = pageArrayList[index];
        ResetToIndex(index);
    }

#endregion

#region ========== Events ==========

    ///<summary> 拖拉時 </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    ///<summary> 放開時 </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        SlidingOperation(0);
    }

    /// <summary> 跳到當頁 </summary>
    public void OnClick_SwitchToggle(int pageIndex)
    {
        targetHorizontal = pageArrayList[pageIndex];
    }

#endregion
}