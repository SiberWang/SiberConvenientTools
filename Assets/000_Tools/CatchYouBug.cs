/// <summary>
/// ======================================
/// Siber 2022/05/16 新增醒目Debug小工具
/// ======================================
/// 有時候會覺得，一堆Log然後又不醒目很麻煩嗎？
/// 用這個就對了，幫你加粗、加顏色、直接區隔於一般Debug.Log
/// </summary>
public static class CatchYouBug
{
    private static string Show         = $"<color=#00FF00><b>『 SHOW 』: </b></color>";
    private static string ShowBug      = $"<color=#FF4D00><b>『 BUG 』: </b></color>";
    private static string ShowPosition = $"<color=#FF4D00><b>『 BUG 』: </b></color>";

#region Public Methods

    /// <summary> 更醒目的顯示 Debug.Log (合併前記得刪除，這沒必要留著) </summary>
    /// <param name="log"> 你想顯示的Log </param>
    public static void Debug(string log)
    {
        UnityEngine.Debug.Log($"{ShowBug}{log}\n");
    }

    /// <summary> 更醒目的顯示 Debug.Log (合併前記得刪除，這沒必要留著) </summary>
    /// <param name="log"> 你想顯示的Log </param>
    /// <param name="title"> 你想標註的標題 </param>
    /// <param name="isHorizontal"> 預設true = 水平</param>
    public static void Debug(string log, string title, bool isHorizontal = true)
    {
        var showTitle = $"[<color=#00FF80><b> {title} </b></color>]";
        if (isHorizontal)
        {
            UnityEngine.Debug.Log($"{ShowBug}{log} , in {showTitle}\n");
        }
        else
        {
            UnityEngine.Debug.Log($"{ShowBug} in {showTitle}\n" +
                                  $"{log}\n");
        }

        /* 如果 isHorizontal = false , 建議用法:
         CatchYouBug.Debug($"DataA: {unit.Number}\n" +
                          $"DataB: {unit.Name}\n" +
                          $"DataC: {unit.Type}\n" +
                          $"DataD: {unit.Snake}" , 
                          this.name , false);
        */
    }

    /// <summary> 更醒目的顯示 Debug.Log (合併前記得刪除，這沒必要留著) </summary>
    /// <param name="log"> 你想顯示的Log </param>
    public static void DeShow(string log)
    {
        UnityEngine.Debug.Log($"{Show}{log}\n");
    }

    /// <summary> 更醒目的顯示 Debug.Log (合併前記得刪除，這沒必要留著) </summary>
    /// <param name="log"> 你想顯示的Log </param>
    /// <param name="title"> 你想標註的標題 </param>
    /// <param name="isHorizontal"> 預設true = 水平 </param>
    public static void DeShow(string log, string title, bool isHorizontal = true)
    {
        var showTitle = $"[<color=#00FF80><b> {title} </b></color>]";
        if (isHorizontal)
        {
            UnityEngine.Debug.Log($"{Show}{log} , in {showTitle}\n");
        }
        else
        {
            UnityEngine.Debug.Log($"{Show} in {showTitle}\n" +
                                  $"{log}\n");
        }
    }

#endregion
}