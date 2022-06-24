using TMPro;
using UnityEngine;

public class NumberCounter : MonoBehaviour
{
#region ========== Public Variables ==========

    public TMP_Text textNumber;

#endregion

#region ========== Private Variables ==========

    private int amount;

#endregion

#region ========== Events ==========

    public void OnAdd()
    {
        amount++;
        UpdateText();
    }

    public void OnReduce()
    {
        amount--;
        UpdateText();
    }

#endregion

#region ========== Private Methods ==========

    private void UpdateText()
    {
        textNumber.text = $"{amount}";
    }

#endregion
}