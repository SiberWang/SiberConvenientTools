using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CustomTool.UIScrollRect
{
    /// <summary>
    /// 概念：紀錄ScrollRect尺寸，並判斷是否有 GridLayoutGroup、ContentSizeFitter
    /// 並使用 ScrollRectPool(StackPool) 來達到節省資源的效果
    /// </summary>
    public class ScrollRectPanel : MonoBehaviour
    {
    #region SerializeField Private Variables

        [SerializeField] private Direction m_Direction = Direction.Horizontal; // 垂直或水平

        private float m_Spacing_x = 0f; // 間距 
        private float m_Spacing_y = 0f; // 間距 
        private int   m_Row       = 1;  // 排數

        // 偵測下面兩種Components，並儲存尺寸資料至此Script內
        // 現在建議都使用 ContentSizeFitter + GridLayoutGroup 的方式
        private bool hasContentSizeFitter;
        private bool hasGridLayoutGroup;

    #endregion

    #region Private Variables

        private enum Direction
        {
            Horizontal, Vertical
        }

        private Action<GameObject, int> m_CallBackAction; // 想把物件的資訊更新，會需要用到這個

        private bool          m_IsClearList;
        private bool          m_IsInitedSetting;
        private bool          m_IsInitedPool;
        private CardPosInfo[] m_CardPosInfos;

        private GameObject     m_TargetPrefab;
        private RectOffset     m_tempPadding;
        private RectTransform  m_ContentRectTrans;
        private RectTransform  m_RectTrans;
        private ScrollRect     m_ScrollRect;
        private ScrollRectPool m_ScrollRectPool; // StackPool
        private Transform      m_Content;

        private float m_CellObjectHeight;
        private float m_CellObjectWidth;
        private float m_ContentHeight;
        private float m_ContentWidth;
        private float m_PlaneHeight;
        private float m_PlaneWidth;

        private int     m_MaxCount = -1;
        private int     m_MaxIndex = -1;
        private int     m_MinIndex = -1;
        private Vector2 m_CellSize;
        private bool    isStoreContentInfo;

    #endregion

    #region Events

        private void OnDestroy()
        {
            InitAllAction();
        }

    #endregion

    #region Public Methods

        /// <summary> 紀錄ScrollRect相關資訊，包含其中的 content </summary>
        /// <param name="scrollRect"> 指定的scrollRect </param>
        public void StoreScrollRectSetting(ScrollRect scrollRect)
        {
            if (isStoreContentInfo) return;
            m_ScrollRect = scrollRect;
            m_Content    = scrollRect.content;
            // 避免一開始就被看到，先隱藏
            m_Content.gameObject.SetActive(false);

            // 以下是讀取 GridLayoutGroup 的內容
            hasContentSizeFitter = m_Content.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter);
            if (hasContentSizeFitter) contentSizeFitter.enabled = false;
            else UnityEngine.Debug.LogError($"Your {m_Content.gameObject.name} , Don't have a ContentSizeFitter");

            hasGridLayoutGroup = m_Content.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup);
            if (hasGridLayoutGroup)
            {
                var constraintCount = gridLayoutGroup.constraintCount;
                // 抓 GridLayoutGroup 的設定
                var gridCellSize = gridLayoutGroup.cellSize;
                var gridSpacing  = gridLayoutGroup.spacing;

                // 判斷排列方式是否為 Flexible (這邊這樣做的原因是，讓 GridLayoutGroup 去自動排列時去紀錄現在顯示的排列數量)
                if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible)
                {
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                    var size = GetGridSize(gridLayoutGroup);
                    constraintCount = size.x;
                }

                m_tempPadding = gridLayoutGroup.padding;
                m_CellSize    = gridCellSize;
                m_Spacing_x   = gridSpacing.x;
                m_Spacing_y   = gridSpacing.y;
                m_Row         = Math.Max(2, constraintCount);

                // 關閉 GridLayoutGroup & Content Size Fitter
                gridLayoutGroup.enabled = false;

                // 刪除底下子物件
                if (m_Content.childCount > 0)
                    foreach (Transform child in m_Content)
                        Destroy(child.gameObject);
            }
            else UnityEngine.Debug.LogError($"Your {m_Content.gameObject.name} , Don't have a GridLayoutGroup");

            m_Content.gameObject.SetActive(true);
            isStoreContentInfo = true;
        }

        /// <summary> 基礎設定 </summary>
        /// <param name="prefab">要複製的物件</param>
        /// <param name="callBackAction">執行方法</param>
        /// <param name="isSpawnActive">是否一創建就顯示出來</param>
        public virtual void Init(GameObject prefab, Action<GameObject, int> callBackAction, bool isSpawnActive = true)
        {
            if (m_IsInitedSetting) return;
            if (prefab == null)
            {
                Debug.Log("prefab is null");
                return;
            }

            m_TargetPrefab = prefab;

            if (!m_IsInitedPool)
            {
                // 刪除底下子物件
                if (m_Content.childCount > 0)
                    foreach (Transform child in m_Content)
                        Destroy(child.gameObject);

                // 設定Cell Target 的資料
                var cellRectTrans = m_TargetPrefab.GetComponent<RectTransform>();
                cellRectTrans.anchorMin     = new Vector2(0, 1);
                cellRectTrans.anchorMax     = new Vector2(0, 1);
                cellRectTrans.sizeDelta     = m_CellSize;
                cellRectTrans.localPosition = new Vector3(default, default, 0f);

                m_TargetPrefab      = Instantiate(m_TargetPrefab, m_Content.parent, true);
                m_TargetPrefab.name = $"{m_TargetPrefab.name} (CellSample)";
                m_TargetPrefab.SetActive(false);
                m_ScrollRectPool = new ScrollRectPool(m_TargetPrefab, m_Content, isSpawnActive);
                m_IsInitedPool   = true;
            }

            m_Content.gameObject.SetActive(true);
            DoInit(callBackAction);
        }

        public void ShowList(int spawnNum)
        {
            SetPosByDirection(spawnNum);               // 計算 Content 尺寸
            var lastEndIndex = GetLastIndex(spawnNum); // 計算 開始索引Index
            RoundCard(spawnNum, lastEndIndex);
            m_MaxCount = spawnNum;
        }

        /// <summary> 整體更新 - 檢查 是否有物件超出範圍並回收 或 放出物件 </summary>
        public void UpdateCheck()
        {
            if (m_CardPosInfos == null) return;

            //檢查範圍
            for (int i = 0, length = m_CardPosInfos.Length; i < length; i++)
            {
                CardPosInfo cardPosInfo = m_CardPosInfos[i];
                Vector3     pos         = cardPosInfo.pos;

                float rangePos = m_Direction == Direction.Vertical ? pos.y : pos.x;
                //判斷是否超出顯示的範圍
                if (IsOutOfRange(rangePos))
                {
                    //把超出範圍的物件回收至StackPool內
                    SetIndexCardToPool(m_CardPosInfos, i);
                }
                else
                {
                    //優先從StackPool中，取出物件出來
                    if (cardPosInfo.obj != null) continue;
                    GameObject cell = m_ScrollRectPool.PopFromPool();
                    cell.transform.localPosition = pos;
                    cell.gameObject.name         = i.ToString();
                    m_CardPosInfos[i].obj        = cell;

                    DoAction(m_CallBackAction, cell);
                }
            }
        }

        /// <summary> 更新一個物件的尺寸及資訊 </summary>
        public void UpdateCell(int index)
        {
            CardPosInfo cardPosInfo = m_CardPosInfos[index];
            if (cardPosInfo.obj != null)
            {
                float rangePos = m_Direction == Direction.Vertical ? cardPosInfo.pos.y : cardPosInfo.pos.x;
                if (IsOutOfRange(rangePos)) return;
                DoAction(m_CallBackAction, cardPosInfo.obj);
            }
        }

        /// <summary> 更新List中的物件尺寸及資訊 </summary>
        public void UpdateList()
        {
            for (int i = 0, length = m_CardPosInfos.Length; i < length; i++)
            {
                CardPosInfo cardPosInfo = m_CardPosInfos[i];
                if (cardPosInfo.obj != null)
                {
                    float rangePos = m_Direction == Direction.Vertical ? cardPosInfo.pos.y : cardPosInfo.pos.x;
                    if (IsOutOfRange(rangePos)) continue;
                    DoAction(m_CallBackAction, cardPosInfo.obj);
                }
            }
        }

        /// <summary> 設定 CallBack action </summary>
        public void SetCallBack(Action<GameObject, int> callBack)
        {
            m_CallBackAction = callBack;
        }

        /// <summary> 清除至預設值 </summary>
        public void Clear()
        {
            m_MaxCount        = -1;
            m_MinIndex        = -1;
            m_MaxIndex        = -1;
            m_IsInitedSetting = false;
            m_IsInitedPool    = false;
            m_ScrollRectPool  = null;
        }

    #endregion

    #region Private Methods

        /// <summary> ScrollRect - RemoveAllListeners，並綁定指定事件 </summary>
        /// <param name="action">指定的方法</param>
        private void BindScrollRect(UnityAction<Vector2> action)
        {
            if (m_ScrollRect == null) return;
            m_ScrollRect.onValueChanged.RemoveAllListeners();
            m_ScrollRect.onValueChanged.AddListener(action);
        }

        /// <summary> 檢查 UI Rect Transform Anchor 位置有沒有跑掉 </summary>
        private void CheckAnchors(RectTransform rectTrans)
        {
            var minYEqualOne   = rectTrans.anchorMin == new Vector2(0, 1);
            var maxYEqualOne   = rectTrans.anchorMax == new Vector2(0, 1);
            var maxXYEqualOne  = rectTrans.anchorMax == new Vector2(1, 1);
            var minXYEqualZero = rectTrans.anchorMin == new Vector2(0, 0);

            // 如果是垂直
            if (m_Direction == Direction.Vertical)
            {
                if (!((minYEqualOne && maxYEqualOne) || (minYEqualOne && maxXYEqualOne)))
                {
                    rectTrans.anchorMin = new Vector2(0, 1);
                    rectTrans.anchorMax = new Vector2(1, 1);
                }
            }
            else // 如果是水平
            {
                if (!((minYEqualOne && maxYEqualOne) || (minXYEqualZero && maxYEqualOne)))
                {
                    rectTrans.anchorMin = new Vector2(0, 0);
                    rectTrans.anchorMax = new Vector2(0, 1);
                }
            }
        }

        /// <summary>呼叫事件</summary>
        /// <param name="action">執行指定方法</param>
        /// <param name="selectObject">由於物件命名為數字，利用名稱當編號</param>
        private void DoAction(Action<GameObject, int> action, GameObject selectObject)
        {
            var num = int.Parse(selectObject.name);
            action?.Invoke(selectObject, num);
        }

        /// <summary>初始化</summary>
        private void DoInit(Action<GameObject, int> callBackAction)
        {
            InitAllAction();
            m_CallBackAction = callBackAction;
            m_ScrollRectPool.PushToPool(m_TargetPrefab);
            StoreCardRect();                  //儲存Rect的相關資訊，並設定
            BindScrollRect(ScrollRectAction); //在 ScrollRect 增加滑動事件
            m_IsInitedSetting = true;
        }

        private int GetLastIndex(int spawnNum)
        {
            int lastEndIndex = 0;

            // 過多的物體丟回物件池，首次使用 ShowList 時則無效
            if (m_IsInitedPool)
            {
                lastEndIndex = spawnNum - m_MaxCount > 0 ? m_MaxCount : spawnNum; //如果Index超出最大值，就等於最大值
                lastEndIndex = m_IsClearList ? 0 : lastEndIndex;                  //如果List已經初始化，就從0開始

                int count = m_IsClearList ? m_CardPosInfos.Length : m_MaxCount;
                for (int i = lastEndIndex; i < count; i++) //從LastEndIndex 開始回收
                {
                    SetIndexCardToPool(m_CardPosInfos, i);
                }
            }

            return lastEndIndex;
        }

        private void InitAllAction()
        {
            if (m_CallBackAction != null)
                m_CallBackAction = null;
        }

        private CardPosInfo BackToPool(CardPosInfo info)
        {
            if (info.obj == null) return info;
            m_ScrollRectPool.PushToPool(info.obj);
            info.obj = null;
            return info;
        }

        /// <summary> 判斷是否超出顯示的範圍 </summary>
        private bool IsOutOfRange(float pos)
        {
            Vector3 listP = m_ContentRectTrans.anchoredPosition;
            if (m_Direction == Direction.Vertical)
            {
                if (pos + listP.y > m_CellObjectHeight || pos + listP.y < -m_RectTrans.rect.height)
                    return true;
            }
            else
            {
                if (pos + listP.x < -m_CellObjectWidth || pos + listP.x > m_RectTrans.rect.width)
                    return true;
            }

            return false;
        }


        /// <summary> 計算每一個位置並儲存，顯示範圍內的物件 </summary>
        private void RoundCard(int spawnNum, int lastEndIndex)
        {
            // Default index
            m_MinIndex = -1;
            m_MaxIndex = -1;
            // temp = 暫時記錄的 PosInfos
            CardPosInfo[] tempPosInfos = m_CardPosInfos;
            m_CardPosInfos = new CardPosInfo[spawnNum];

            for (int i = 0; i < spawnNum; i++)
            {
                // 儲存已經有的數據
                if (m_MaxCount != -1 && i < lastEndIndex)
                {
                    ShowCard(tempPosInfos, i);
                    continue;
                }

                // 首次使用，會儲存卡片數據
                ShowDefaultCard(i);
            }
        }

        /// <summary> ScrollRect - 滑動的事件 </summary>
        /// <param name="value">目前沒用途</param>
        private void ScrollRectAction(Vector2 value)
        {
            UpdateCheck();
        }

        /// <summary> StackPool - 回收並設定物件為Null </summary>
        private void SetIndexCardToPool(CardPosInfo[] infos, int i)
        {
            infos[i] = BackToPool(infos[i]);
        }

        /// <summary> 儲存並設定 Content'S RectTrans 的垂直或水平的數據 </summary>
        private void SetPosByDirection(int num)
        {
            if (m_Direction == Direction.Vertical)
            {
                float contentSize = m_tempPadding.top + (m_Spacing_y + m_CellObjectHeight) * Mathf.CeilToInt((float)num / m_Row) + m_tempPadding.bottom - m_Spacing_y;
                m_ContentHeight              = contentSize;
                m_ContentWidth               = m_ContentRectTrans.sizeDelta.x;
                contentSize                  = contentSize < m_RectTrans.rect.height ? m_RectTrans.rect.height : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(m_ContentWidth, contentSize);
                if (num != m_MaxCount)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(m_ContentRectTrans.anchoredPosition.x, 0);
                }
            }
            else
            {
                float contentSize = m_tempPadding.left + (m_Spacing_x + m_CellObjectWidth) * Mathf.CeilToInt((float)num / m_Row) +
                                    m_tempPadding.right - m_Spacing_x;
                m_ContentWidth               = contentSize;
                m_ContentHeight              = m_ContentRectTrans.sizeDelta.x;
                contentSize                  = contentSize < m_RectTrans.rect.width ? m_RectTrans.rect.width : contentSize;
                m_ContentRectTrans.sizeDelta = new Vector2(contentSize, m_ContentHeight);
                if (num != m_MaxCount)
                {
                    m_ContentRectTrans.anchoredPosition = new Vector2(0, m_ContentRectTrans.anchoredPosition.y);
                }
            }
        }

        /// <summary> 除了第一次創立之外，都會以這個來更新 </summary>
        private void ShowCard(CardPosInfo[] cardPosInfos, int index)
        {
            CardPosInfo cardPosInfo = cardPosInfos[index];
            var         cardPosY    = cardPosInfo.pos.y;
            var         cardPosX    = cardPosInfo.pos.x;
            float       cardPos     = m_Direction == Direction.Vertical ? cardPosY : cardPosX;
            if (IsOutOfRange(cardPos)) // 計算是否超出範圍
            {
                SetIndexCardToPool(cardPosInfos, index);
            }
            else
            {
                // 紀錄顯示中的 第一個 和 最後一個
                m_MinIndex = m_MinIndex == -1 ? index : m_MinIndex; // 第一個Index
                m_MaxIndex = index;                                 // 最後一個Index

                if (cardPosInfo.obj == null)
                    cardPosInfo.obj = m_ScrollRectPool.PopFromPool();

                var cardRect = cardPosInfo.obj.GetComponent<RectTransform>();
                cardRect.anchoredPosition = cardPosInfo.pos;
                cardPosInfo.obj.name      = index.ToString();
                DoAction(m_CallBackAction, cardPosInfo.obj);
            }

            m_CardPosInfos[index] = cardPosInfo;
        }

        /// <summary> 首次創立會先呼叫這個，來設定物件尺寸 </summary>
        private void ShowDefaultCard(int index)
        {
            CardPosInfo cardPosInfo = new CardPosInfo();

            float pos    = 0;
            float rowPos = 0;

            // 計算每一排卡片的座標
            // 改變卡片相對位置的一環，像是客製化的 Grid Layout Group
            if (m_Direction == Direction.Vertical)
            {
                rowPos = m_tempPadding.left + m_CellObjectWidth * (index % m_Row) +
                         m_Spacing_x * (index % m_Row);

                pos = m_tempPadding.top + m_CellObjectHeight * Mathf.FloorToInt(index / m_Row) +
                      m_Spacing_y * Mathf.FloorToInt(index / m_Row);

                cardPosInfo.pos = new Vector3(rowPos, -pos, 0);
            }
            else
            {
                pos = m_tempPadding.left + m_CellObjectWidth * Mathf.FloorToInt(index / m_Row) +
                      m_Spacing_x * Mathf.FloorToInt(index / m_Row);

                rowPos = m_tempPadding.top + m_CellObjectHeight * (index % m_Row) +
                         m_Spacing_y * (index % m_Row);

                cardPosInfo.pos = new Vector3(pos, -rowPos, 0);
            }

            var   posY    = cardPosInfo.pos.y;
            var   posX    = cardPosInfo.pos.x;
            float cardPos = m_Direction == Direction.Vertical ? posY : posX;
            if (IsOutOfRange(cardPos)) // 計算是否超出範圍
            {
                cardPosInfo.obj       = null;
                m_CardPosInfos[index] = cardPosInfo;
                return;
            }

            // 紀錄顯示中的 第一個 和 最後一個
            m_MinIndex = m_MinIndex == -1 ? index : m_MinIndex; // 第一個Index
            m_MaxIndex = index;                                 // 最後一個Index

            //創建卡片
            GameObject card          = m_ScrollRectPool.PopFromPool();
            var        rectTransform = card.transform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = cardPosInfo.pos;
            // 名稱一定要為數字的原因，因為目前DoAction，是利用名稱去索引 (其實可以利用Dictionary去做到)
            // 只是現在是為了驗證功能，未來在優化
            card.gameObject.name = index.ToString();

            // 更新數據
            cardPosInfo.obj       = card;
            m_CardPosInfos[index] = cardPosInfo;

            // 執行事件
            DoAction(m_CallBackAction, card);
        }

        /// <summary>儲存各UI RectTransform 的相關資訊，並設定</summary>
        private void StoreCardRect()
        {
            //重置引用的 Prefab 的 RectTransform 位置資訊
            var cellRectTrans = m_TargetPrefab.GetComponent<RectTransform>();
            cellRectTrans.pivot = new Vector2(0f, 1f);
            CheckAnchors(cellRectTrans);
            cellRectTrans.anchoredPosition = Vector2.zero;
            //紀錄物件尺寸資訊
            m_CellObjectHeight = cellRectTrans.rect.height;
            m_CellObjectWidth  = cellRectTrans.rect.width;

            //紀錄Plane尺寸資訊
            m_RectTrans = m_ScrollRect.GetComponent<RectTransform>();
            Rect planeRect = m_RectTrans.rect;
            m_PlaneHeight = planeRect.height;
            m_PlaneWidth  = planeRect.width;

            //紀錄Content尺寸資訊
            m_ContentRectTrans = m_Content.GetComponent<RectTransform>();
            Rect contentRect = m_ContentRectTrans.rect;
            m_ContentHeight = contentRect.height;
            m_ContentWidth  = contentRect.width;

            //設定 UI Content Rect 的尺寸資訊
            m_ContentRectTrans.pivot = new Vector2(0f, 1f);
            // m_ContentRectTrans.sizeDelta = new Vector2(planeRect.width, planeRect.height);
            if (hasGridLayoutGroup)
            {
                if (m_Direction == Direction.Vertical)
                    m_ContentRectTrans.sizeDelta = new Vector2(default, planeRect.height);
                if (m_Direction == Direction.Horizontal)
                    m_ContentRectTrans.sizeDelta = new Vector2(planeRect.width, default);
            }

            m_ContentRectTrans.anchoredPosition = Vector2.zero;
            CheckAnchors(m_ContentRectTrans);
        }

    #endregion

    #region Grid Size Count

        // 利用 GridLayoutGroup 的 CellSize 去計算幾排幾列
        public Vector2Int GetGridSize(GridLayoutGroup grid)
        {
            int        itemsCount = grid.transform.childCount;
            Vector2Int size       = Vector2Int.zero;

            if (itemsCount == 0) return size;

            switch (grid.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    size.x = grid.constraintCount;
                    size.y = getAnotherAxisCount(itemsCount, size.x);
                    break;

                case GridLayoutGroup.Constraint.FixedRowCount:
                    size.y = grid.constraintCount;
                    size.x = getAnotherAxisCount(itemsCount, size.y);
                    break;

                case GridLayoutGroup.Constraint.Flexible:
                    size = flexibleSize(grid);
                    break;
            }

            return size;
        }

        private Vector2Int flexibleSize(GridLayoutGroup grid)
        {
            int   itemsCount = grid.transform.childCount;
            float prevX      = float.NegativeInfinity;
            int   xCount     = 0;

            for (int i = 0; i < itemsCount; i++)
            {
                Vector2 pos = ((RectTransform)grid.transform.GetChild(i)).anchoredPosition;
                if (pos.x <= prevX) break;
                prevX = pos.x;
                xCount++;
            }

            int yCount = getAnotherAxisCount(itemsCount, xCount);
            return new Vector2Int(xCount, yCount);
        }

        private int getAnotherAxisCount(int totalCount, int axisCount)
        {
            return totalCount / axisCount + Mathf.Min(1, totalCount % axisCount);
        }

    #endregion

    #region Struct

        private struct CardPosInfo
        {
            public Vector3    pos; // 紀錄位置
            public GameObject obj; // 紀錄物件 - 會因為滑動而改變
        }

    #endregion
    }
}