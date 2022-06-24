using System;
using UnityEngine;

namespace CustomTool.UIScrollRect
{
    /// <summary>
    /// 為了測試 rectTransform.sizeDelta 所遇到的問題
    /// 問題1 - 會因為 Anchors 的關係而導致很奇怪，建議強制改變後在進行 
    /// </summary>
    public class TestForSpawnCard : MonoBehaviour
    {
        public Vector2 cellSize;
        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.one;
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.sizeDelta = cellSize;
        }
    }
}