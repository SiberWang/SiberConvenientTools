using UnityEngine;

/// <summary>
/// 用於測試：計算物品合成時，會遇到的狀況
/// </summary>
public class MathfCounter : MonoBehaviour
{
    [Header("合成數量")]
    public int syntheticCount;
    [Header("物品持有數量")]
    public int currentCount;
    [Header("花費素材數量")]
    public int spendCount;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Count();
        }
    }

    private void Count()
    {
        if (currentCount < spendCount)
        {
            Debug.Log($"持有數量小於花費數量，無法合成");
            return;
        }
        
        var maxCount = currentCount / spendCount;
        if (syntheticCount > maxCount)
        {
            Debug.Log($"超出數量：合成數量 [{syntheticCount}] 改為 [{maxCount}]");
            syntheticCount = maxCount;
        }
        var remainder = currentCount % spendCount;
        var last      = currentCount - (syntheticCount * spendCount);

        Debug.Log($"預期/最大合成數量 : {syntheticCount}/{maxCount}\n" +
                  $"持有物品數量 : {currentCount}\n" +
                  $"最終餘數 : {remainder}\n" +
                  $"剩餘物品數量 : {last}");
    }
}