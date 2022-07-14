using System.Linq;
using UnityEngine;

/// <summary>
/// 針對 ScriptableObject 所用的單例模式
/// 普遍用於，只能存在一個的 SO 所用
/// </summary>
/// <typeparam name="T"> class Type </typeparam>
public abstract class SingletonSO<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null) 
                instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            return instance;
        }
    }
}