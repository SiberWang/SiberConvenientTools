using UnityEngine;

namespace CustomTool.UIScrollRect
{
    [CreateAssetMenu(fileName = "CardData", menuName = "ScrollRectTool/Example/Create CardData", order = 0)]
    public class CardData : ScriptableObject
    {
    #region Public Variables

        public string   name;
        public string   id;
        public CardType cardType;

    #endregion
    }
    
    public enum CardType
    {
        Attack ,Heal ,Spawn ,NoAction
    }
}