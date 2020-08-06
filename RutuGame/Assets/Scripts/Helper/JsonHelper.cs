using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
}

/// <summary>
/// 扩展JsonUtility的List能力, T必须加上[Serializable]标签
/// https://blog.csdn.net/Truck_Truck/article/details/78292390
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Serialization<T>
{
    [SerializeField] List<T> target;

    public List<T> ToList()
    {
        return target;
    }

    public Serialization(List<T> target)
    {
        this.target = target;
    }
}

/// <summary>
/// 扩展JsonUtility的Dictionary能力，TValue必须加上[Serializable]标签
/// Dictionary<TKey, TValue>
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class SerializationDic<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] List<TKey> keys = new List<TKey>();
    [SerializeField] List<TValue> values = new List<TValue>();

    Dictionary<TKey, TValue> target = new Dictionary<TKey, TValue>();

    public new TValue this[TKey key]
    {
        get { return target[key]; }
        set { target[key] = value; }
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return target;
    }

    public SerializationDic()
    {
    }

    public void Add(TKey key, TValue value)
    {
        target.Add(key,value);
    }

    public void Remove(TKey key)
    {
        target.Remove(key);
    }

    public void Clear()
    {
        target.Clear();
    }

    public bool ContainsKey(TKey key)
    {
        return target.ContainsKey(key);
    }

    public SerializationDic(Dictionary<TKey, TValue> target)
    {
        this.target = target;
    }

    public void OnBeforeSerialize()
    {
        keys = new List<TKey>(target.Keys);
        values = new List<TValue>(target.Values);
    }

    public void OnAfterDeserialize()
    {
        var count = Math.Min(keys.Count, values.Count);
        target = new Dictionary<TKey, TValue>(count);
        for (var i = 0; i < count; ++i)
        {
            target.Add(keys[i], values[i]);
        }
    }
}