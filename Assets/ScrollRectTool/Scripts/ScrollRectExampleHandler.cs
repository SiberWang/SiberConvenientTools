using UnityEngine;
using UnityEngine.UI;

namespace CustomTool.UIScrollRect
{
    /// <summary>
    /// [Example] - 模擬資料的來源，並導入至ScrollRectPanel
    /// </summary>
    public class ScrollRectExampleHandler : MonoBehaviour
    {

    #region Private Variables

        [SerializeField]
        private int cellCount; // 卡片數量

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject cellPrefab;

        [SerializeField]
        private ScrollRectPanel scrollRectPanel;
        
        [SerializeField]
        private CardDataOverview cardDataOverview;

    #endregion

    #region Unity events

        private void Start()
        {
            StartScrollView();
            var cardDatas = cardDataOverview.FindDatas();
            // 紀錄資料到一個List 
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                scrollRectPanel.ShowList(cellCount);
                scrollRectPanel.UpdateList();
            }
        }

    #endregion

    #region Private Methods

        private void NormalCallBack(GameObject obj, int index)
        {
            Debug.Log($"obj index : {index}");
            // var cellSample = obj.GetComponent<CellSample>();
            // cellSample.name = 
            // obj.GetComponentInChildren<Text>().text = index.ToString();
        }

        private void StartScrollView()
        {
            scrollRectPanel.Init(cellPrefab, scrollRect, NormalCallBack);
            scrollRectPanel.ShowList(cellCount);
        }

    #endregion
    }
}