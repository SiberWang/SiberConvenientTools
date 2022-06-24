using CustomTool.UIScrollRect;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellSample : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text IDText;
    public TMP_Text DescriptionText;
    public Image    BackGround;

    public void SetCardType(CardType cardType)
    {
        var textColor = Color.white;
        switch (cardType)
        {
            case CardType.Attack:
                textColor = Color.red;
                break;
            case CardType.Heal:
                textColor = Color.green;
                break;
            case CardType.Spawn:
                textColor = Color.yellow;
                break;
        }

        DescriptionText.color = textColor;
        DescriptionText.text  = cardType.ToString();
    }
    
    public void SetCardType(string cardType)
    {
        DescriptionText.text  = cardType;
    }
}
