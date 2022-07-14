/// <summary>
/// 針對 Class 所用的單例模式 
/// </summary>
/// <typeparam name="T"> class Type </typeparam>
public class Singleton<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) 
                instance = new T();
            return instance;
        }
    }
}