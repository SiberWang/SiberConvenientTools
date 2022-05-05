using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomTool.UIScrollRect
{
    [CreateAssetMenu(fileName = "CardDataOverview", menuName = "ScrollRectTool/Example/Create CardDataOverview", order = 0)]
    public class CardDataOverview : ScriptableObject
    {
    #region Public Variables

        public List<CardData> CardDatas;

    #endregion

    #region Public Methods

        public CardData FindData(string dataID)
        {
            var cardData = CardDatas.Find((data => data.id == dataID));
            if (cardData != null) return cardData;
            return null;
        }
        
        public List<CardData> FindDatas()
        {
            var cardDatas = CardDatas.ToList();
            return cardDatas;
        }

    #endregion
    }
}