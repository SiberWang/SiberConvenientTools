using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CustomTool.UIScrollRect
{
    public class ScrollRectPanel : MonoBehaviour
    {
    #region SerializeField Private Variable

        private enum Direction
        {
            Horizontal,
            Vertical
        }

        [SerializeField]
        private Direction m_Direction = Direction.Horizontal;

        [SerializeField]
        private int m_Row = 1;

        [SerializeField]
        private float m_Spacing = 0f;

        [SerializeField]
        private RectOffset tempPadding;

        // 如果有引用下列兩者，會偵測其中的數據，並儲存尺寸資料至此Script內
        [SerializeField]
        private bool hasGridLayoutGroup;

        [SerializeField]
        private bool hasContentSizeFitter;

    #endregion

    #region Protected & Private Variable

        protected Action<GameObject, int> m_CallBackAction;

        protected float m_PlaneWidth;
        protected float m_PlaneHeight;
        protected float m_ContentWidth;
        protected float m_ContentHeight;
        protected float m_CellObjectWidth;
        protected float m_CellObjectHeight;

        protected bool m_IsInitedSetting;
        protected bool m_IsInitedPool;
        protected bool m_IsClearList; //目前沒啥用途
        protected int  m_MaxCount = -1;
        protected int  m_MinIndex = -1;
        protected int  m_MaxIndex = -1;

        protected Transform     m_Content;
        protected RectTransform m_ContentRectTrans;
        protected RectTransform m_RectTrans;
        protected CardPosInfo[] m_CardPosInfos;
        protected ScrollRect    m_ScrollRect;

        private ScrollRectPool m_ScrollRectPool;
        private GameObject     m_TargetPrefab;

    #endregion

    #region Public Methods - Init

        public void Clear()
        {
            Debug.Log($"Clear ScrollRectPanel");
            m_IsInitedSetting = false;
            m_MaxCount = -1;
            m_MinIndex = -1;
            m_MaxIndex = -1;
            Debug.Log($"m_ScrollRectPool : {m_ScrollRectPool != null}");
            // A 刪掉是一個方法
            m_IsInitedPool   = false;
            m_ScrollRectPool = null;

            // B 再利用原來存在的資源也是一個方法
            // m_IsInitedPool = true;
        }

        public virtual void Init(GameObject cellPrefab, ScrollRect scrollRect, Action<GameObject, int> callBackAction)
        {
            //以下只執行1次
            if (m_IsInitedSetting) return;
            if (cellPrefab == null)
            {
                Debug.LogError("m_TargetPrefab is null");
                return;
            }

            m_TargetPrefab = cellPrefab;
            m_ScrollRect   = scrollRect;
            m_Content      = m_ScrollRect.content;

            if (!m_IsInitedPool)
            {
                if (m_Content.childCount > 0)
                    foreach (Transform child in m_Content)
                        Destroy(child.gameObject);

                m_TargetPrefab.name = $"{m_TargetPrefab.name} (CellSample)";
                m_TargetPrefab.SetActive(false);
                m_TargetPrefab   = Instantiate(m_TargetPrefab, m_Content.parent, true);
                m_ScrollRectPool = new ScrollRectPool(m_TargetPrefab, m_Content);
                m_IsInitedPool   = true;
            }

            // 以下是讀取 GridLayoutGroup 的內容
            hasContentSizeFitter = m_Content.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter);
            if (hasContentSizeFitter) contentSizeFitter.enabled = false;

            hasGridLayoutGroup = m_Content.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup);
            if (hasGridLayoutGroup)
            {
                // 抓 GridLayoutGroup 的設定
                var gridCellSize = gridLayoutGroup.cellSize;
                var gridSpacing  = gridLayoutGroup.spacing;
                tempPadding = gridLayoutGroup.padding;
                m_Spacing   = gridSpacing.x;

                // 設定Cell Target 的資料
                var cellRectTrans = m_TargetPrefab.GetComponent<RectTransform>();
                // CheckAnchors(cellRectTrans);

                cellRectTrans.anchorMin = new Vector2(0, 1);
                cellRectTrans.anchorMax = new Vector2(0, 1);
                cellRectTrans.sizeDelta = gridCellSize;
                // 關閉 GridLayoutGroup & Content Size Fitter
                gridLayoutGroup.enabled     = false;
                cellRectTrans.localPosition = new Vector3(default, default, 0f);
            }

            DoInit(callBackAction);
        }

    #endregion

    #region Public Methods

        //更新資訊 - 刷新List
        public void UpdateList()
        {
            for (int i = 0, length = m_CardPosInfos.Length; i < length; i++)
            {
                CardPosInfo cardPosInfo = m_CardPosInfos[i];
                if (cardPosInfo.obj != null)
                {
                    float rangePos = m_Direction == Direction.Vertical
                        ? cardPosInfo.pos.y
                        : cardPosInfo.pos.x;
                    if (IsOutOfRange(rangePos)) continue;
                    DoAction(m_CallBackAction, cardPosInfo.obj);
                }
            }

            Debug.Log("UpdateList");
        }

        //更新資訊 - 刷新某一個項目
        public void UpdateCell(int index)
        {
            CardPosInfo cardPosInfo = m_CardPosInfos[index - 1];
            if (cardPosInfo.obj != null)
            {
                float rangePos = m_Direction == Direction.Vertical ? cardPosInfo.pos.y : cardPosInfo.pos.x;
                if (IsOutOfRange(rangePos)) return;
                DoAction(m_CallBackAction, cardPosInfo.obj);
            }
        }

        public virtual void ShowList(int spawnNum)
        {
            SetContentRectDirection(spawnNum);         // 設定 Content 尺寸
            var lastEndIndex = GetLastIndex(spawnNum); // 計算並開始索引Index
            RoundCard(spawnNum, lastEndIndex);
            m_MaxCount = spawnNum;
        }

        // 1- 計算每一個位置並儲存
        // 2- 顯示範圍內的物件
        // 3- 在這邊就要決定好卡片的Size了
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

        private void ShowDefaultCard(int index)
        {
            CardPosInfo cardPosInfo = new CardPosInfo();

            float rowPos = 0;
            float pos    = 0;

            // 計算每一排卡片的座標
            // 改變卡片相對位置的一環，像是客製化的 Grid Layout Group
            if (m_Direction == Direction.Vertical)
            {
                // 寬度 + 間距 + Padding
                var tempPaddingRight = tempPadding.right;
                var tempPaddingTop   = tempPadding.top;
                rowPos = m_CellObjectWidth * (index % m_Row) + m_Spacing * (index % m_Row) + tempPaddingRight;
                pos = m_CellObjectHeight * Mathf.FloorToInt(index / m_Row) +
                      m_Spacing * Mathf.FloorToInt(index / m_Row) + tempPaddingTop;
                cardPosInfo.pos = new Vector3(rowPos, -pos, 0);
            }
            else
            {
                pos             = m_CellObjectWidth * Mathf.FloorToInt(index / m_Row) + m_Spacing * Mathf.FloorToInt(index / m_Row);
                rowPos          = m_CellObjectHeight * (index % m_Row) + m_Spacing * (index % m_Row);
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

            //呼叫卡片
            GameObject card          = m_ScrollRectPool.PopFromPool();
            var        rectTransform = card.GetComponent<RectTransform>();
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

        private void ShowCard(CardPosInfo[] cardPosInfos, int index)
        {
            CardPosInfo cardPosInfo = cardPosInfos[index];
            var         cardPosY    = cardPosInfo.pos.y;
            var         cardPosX    = cardPosInfo.pos.x;
            float       cardPos     = m_Direction == Direction.Vertical ? cardPosY : cardPosX;
            if (IsOutOfRange(cardPos)) // 計算是否超出範圍
            {
                TakeCardToPool(cardPosInfos, index);
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
                cardPosInfo.obj.SetActive(true);

                DoAction(m_CallBackAction, cardPosInfo.obj);
            }

            m_CardPosInfos[index] = cardPosInfo;
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
                    TakeCardToPool(m_CardPosInfos, i);
                }
            }

            return lastEndIndex;
        }

        private void SetContentRectDirection(int num)
        {
            if (m_Direction == Direction.Vertical)
            {
                float contentSize = (m_Spacing + m_CellObjectHeight) * Mathf.CeilToInt((float)num / m_Row);
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
                float contentSize = (m_Spacing + m_CellObjectWidth) * Mathf.CeilToInt((float)num / m_Row);
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

        public void SetCellBack(Action<GameObject, int> cellBack)
        {
            m_CallBackAction = cellBack;
        }

    #endregion

    #region Unity Events

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                for (int i = 0; i < m_CardPosInfos.Length; i++)
                {
                    var mCardPosInfo = m_CardPosInfos[i];
                    if (mCardPosInfo.obj == null) continue;
                    Debug.Log($"m_CardPosInfos[{i}].pos : {mCardPosInfo.pos}\n" +
                              $"m_CardPosInfos[{i}].obj : name: {mCardPosInfo.obj.name}/pos :{mCardPosInfo.obj.transform.position}");
                }
            }
        }

        private void OnDestroy()
        {
            DisableAll();
        }

        private void OnDisable()
        {
            DisableAll();
        }

    #endregion

    #region Private Methods

        private void DoInit(Action<GameObject, int> callBackAction)
        {
            DisableAll();
            m_CallBackAction = callBackAction;

            m_ScrollRectPool.PushToPool(m_TargetPrefab);
            StoreCardRect();                  //儲存Rect的相關資訊，並設定
            BindScrollRect(ScrollRectAction); //在 ScrollRect 增加滑動事件
            m_IsInitedSetting = true;
        }

        /// <summary> ScrollRect - RemoveAllListeners，並綁定指定事件 </summary>
        /// <param name="action">指定的方法</param>
        private void BindScrollRect(UnityAction<Vector2> action)
        {
            m_ScrollRect.onValueChanged.RemoveAllListeners();
            m_ScrollRect.onValueChanged.AddListener(action);
        }

        //檢查 UI Rect Transform Anchor 位置有沒有跑掉
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

        //紀錄物件尺寸資訊
        private void StoreCardRect()
        {
            // m_Content = GetComponent<ScrollRect>().content;

            //重置引用的 Prefab 的 RectTransform 位置資訊
            var cellRectTrans = m_TargetPrefab.GetComponent<RectTransform>();
            cellRectTrans.pivot = new Vector2(0f, 1f);
            CheckAnchors(cellRectTrans);
            cellRectTrans.anchoredPosition = Vector2.zero;
            m_CellObjectHeight             = cellRectTrans.rect.height;
            m_CellObjectWidth              = cellRectTrans.rect.width;

            //紀錄Plane尺寸資訊
            m_RectTrans = GetComponent<RectTransform>();
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

        /// <summary>ScrollRect - 滑動的事件</summary>
        /// <param name="value">目前沒用途</param>
        private void ScrollRectAction(Vector2 value)
        {
            UpdateCheck();
        }

        //滑動時檢查是否超出範圍，釋出 or 回收
        private void UpdateCheck()
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
                    TakeCardToPool(m_CardPosInfos, i);
                }
                else
                {
                    if (cardPosInfo.obj != null) continue;
                    //優先從StackPool中，取出物件出來
                    GameObject cell = m_ScrollRectPool.PopFromPool();
                    cell.transform.localPosition = pos;
                    cell.gameObject.name         = i.ToString();
                    m_CardPosInfos[i].obj        = cell;

                    DoAction(m_CallBackAction, cell);
                }
            }
        }

        //判斷是否超出顯示的範圍
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

        /// <summary>呼叫事件</summary>
        /// <param name="action">執行指定方法</param>
        /// <param name="selectObject">由於物件命名為數字，利用名稱當編號</param>
        private void DoAction(Action<GameObject, int> action, GameObject selectObject)
        {
            var num = int.Parse(selectObject.name);
            action?.Invoke(selectObject, num);
        }

        private void DisableAll()
        {
            if (m_CallBackAction != null)
                m_CallBackAction = null;
        }

        /// <summary> StackPool - 回收並設定物件為Null </summary>
        private void TakeCardToPool(CardPosInfo[] infos, int i)
        {
            infos[i] = BackToPool(infos[i]);
        }

        private CardPosInfo BackToPool(CardPosInfo info)
        {
            if (info.obj == null) return info;
            m_ScrollRectPool.PushToPool(info.obj);
            info.obj = null;
            return info;
        }

    #endregion
    }
}