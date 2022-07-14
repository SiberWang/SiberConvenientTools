using UnityEngine;

/// <summary>
/// 針對 MonoBehaviour 所用的單例模式 
/// </summary>
/// <typeparam name="T"> class Type </typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null) {
                    GameObject obj = new GameObject(typeof(T).ToString());
                    instance = obj.AddComponent<T>();
                }
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }
}