using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// where T: Singleton<T> 限制了 T 的类型，必须是 Singleton<T> 的子类
public class Singleton<T> : MonoBehaviour where T: Singleton<T>
{
    private static T instance;

    public static T Instance { get => instance; }

    protected virtual void Awake() 
    {
        if(instance != null) { Destroy(gameObject); }
        else { instance = this as T; }
    }

    protected virtual void OnDestroy() 
    {
        if(instance == this) { instance = null; }
    }
}
