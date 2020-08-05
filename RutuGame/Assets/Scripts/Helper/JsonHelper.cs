using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
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
    public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField] List<TKey> keys;
        [SerializeField] List<TValue> values;

        Dictionary<TKey, TValue> target;

        public Dictionary<TKey, TValue> ToDictionary()
        {
            return target;
        }

        public Serialization(Dictionary<TKey, TValue> target)
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
}
