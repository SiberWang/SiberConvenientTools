using UnityEngine;

namespace CustomTool.UIScrollRect
{
    public struct CardPosInfo
    {
        public Vector3 pos; // 紀錄位置
        public GameObject obj; // 紀錄物件 - 會因為滑動而改變
    }
}