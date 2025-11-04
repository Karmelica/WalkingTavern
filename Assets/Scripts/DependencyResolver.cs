using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class DependencyResolver : MonoBehaviour
{
    public static DependencyResolver Instance { get; private set; }
    
    private readonly Dictionary<Type, object> _dependencies = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void Register<T>(T instance)
    {
        var type = typeof(T);
        if (!_dependencies.TryAdd(type, instance))
        {
            Debug.LogWarning($"Dependency of type {type} is already registered.");
        }
    }
    
    public T Resolve<T>()
    {
        var type = typeof(T);
        if (_dependencies.TryGetValue(type, out var instance))
        {
            return (T)instance;
        }
        throw new Exception($"No dependency of type {type} is registered.");
    }
    
    public void Unregister<T>()
    {
        var type = typeof(T);
        if (!_dependencies.Remove(type))
        {
            Debug.LogWarning($"No dependency of type {type} is registered to unregister.");
        }
    }
    
    public bool IsRegistered<T>()
    {
        return _dependencies.ContainsKey(typeof(T));
    }
    
    
    
}
