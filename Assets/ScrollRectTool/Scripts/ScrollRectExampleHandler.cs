using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace CustomTool.UIScrollRect
{
    /// <summary>
    /// [Example] - 模擬資料的來源，並導入至ScrollRectPanel
    /// </summary>
    public class ScrollRectExampleHandler : MonoBehaviour
    {
        public class TempData
        {
        #region Public Variables

            public CardType cardType;
            public string   id;
            public string   name;

        #endregion
        }

    #region Private Variables

        [SerializeField]
        private int cellCount; // 卡片數量

        [SerializeField]
        private GameObject cellPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private ScrollRectPanel scrollRectPanel;

        [SerializeField]
        private CardDataOverview cardDataOverview;

        private Dictionary<string, TempData> tempDatas = new Dictionary<string, TempData>();

        private List<TempData> tempDataList;
        private List<TempData> currentDataList;

    #endregion

    #region Unity events

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Debug.Log("Start ScrollRectExampleHandler");
                scrollRectPanel.StoreScrollRectSetting(scrollRect);
                //Store Data
                StoreDataToList(); // 沒問題
                //Create ScrollRectPanel
                CreateScrollRectPanel();
            }
        }

        private void OnDisable()
        {
            scrollRectPanel.Clear();
        }

    #endregion

    #region Private Methods

        // 紀錄資料到一個List 
        private void StoreDataToList()
        {
            if (cardDataOverview == null) Debug.LogError("CardDataOverview is null");
            Debug.Log($"----- Load Datas -----");
            var cardDatas = cardDataOverview.FindDatas();

            if (tempDatas.Count > 0) tempDatas.Clear();

            foreach (var cardData in cardDatas)
            {
                if (cardData == null) continue;
                var tempData = new TempData();
                tempData.name     = cardData.name;
                tempData.id       = cardData.id;
                tempData.cardType = cardData.cardType;
                if (!tempDatas.ContainsKey(tempData.id))
                    tempDatas.Add(tempData.id, tempData);
            }

            // 暫存需要用到的資料到List中，並排好順序
            tempDataList = OrderByUnits(new List<TempData>(tempDatas.Values));
            if (tempDataList != null) cellCount = tempDataList.Count;
            currentDataList = tempDataList;
            
            // foreach (var data in tempDataList)
            // {
            //     Debug.Log($"data : {data.name} {data.id}");
            // }
        }

        /// <summary>
        /// 讀取資料的地方
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="index"></param>
        private void LoadInfoCallBack(GameObject obj, int index)
        {
            // Debug.Log($"obj index : {index}");
            if (currentDataList == null)
            {
                Debug.LogError("currentDataList is null");
                return;
            }

            var cellSample = obj.GetComponent<CellSample>();
            for (int i = 0; i < currentDataList.Count; i++)
            {
                if (i != index) continue;
                cellSample.NameText.text = currentDataList[i].name;
                cellSample.IDText.text   = currentDataList[i].id;
                cellSample.SetCardType(currentDataList[i].cardType);
            }
        }

        private void CreateScrollRectPanel()
        {
            scrollRectPanel.Init(cellPrefab, LoadInfoCallBack);
            scrollRectPanel.ShowList(cellCount);
        }

        ///<summary> 編排List順序 </summary>
        private List<TempData> OrderByUnits(List<TempData> units)
        {
            //@"\d+\" 正整數
            //@"\d+\.*\d*" 正數(有小數點)
            // Debug.Log(Regex.Match("anything 876.8 anything", @"\d+\.*\d*").Value);
            // Debug.Log(Regex.Match("anything 876 anything", @"\d+\.*\d*").Value);
            // Debug.Log(Regex.Match("$876435", @"\d+\.*\d*").Value);
            // Debug.Log(Regex.Match("$876.435", @"\d+\.*\d*").Value);
            Debug.Log($"OrderByUnits");
            return units.OrderBy(x =>
            {
                // 利用大小來排序
                var targetID = x.id;
                var id       = Regex.Match($"{targetID}", @"\d+\.*\d*").Value;
                var intID    = int.Parse(id);
                return intID;
            }).ToList();
        }

        public void OnSelectCardType(string cardType)
        {
            List<TempData> selectDataList = null;
            if (cardType == CardType.Attack.ToString())
                selectDataList = tempDataList.Where(card => card.cardType == CardType.Attack).ToList();
            if (cardType == CardType.Heal.ToString())
                selectDataList = tempDataList.Where(card => card.cardType == CardType.Heal).ToList();
            if (cardType == CardType.Spawn.ToString())
                selectDataList = tempDataList.Where(card => card.cardType == CardType.Spawn).ToList();
            if (cardType == "All")
                selectDataList = tempDataList.ToList();


            currentDataList = selectDataList;
            scrollRectPanel.ShowList(currentDataList.Count);
            
            // foreach (var data in currentDataList)
            // {
            //     Debug.Log($"data : {data.name} {data.id} , OnSelectCardType");
            // }
        }

    #endregion
    }
}