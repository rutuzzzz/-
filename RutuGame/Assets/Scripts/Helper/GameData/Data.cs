using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public GameIntData _intData = new GameIntData();

    public GameStringData _stringData = new GameStringData();

    public GameFloatData _floatData = new GameFloatData();

    public void SetData(string key, int value)
    {
        if (!_intData.ContainsKey(key))
        {
            _intData.Add(key, value);
        }
        else
        {
            _intData[key] = value;
        }
    }

    public void SetData(string key, string value)
    {
        if (!_stringData.ContainsKey(key))
        {
            _stringData.Add(key, value);
        }
        else
        {
            _stringData[key] = value;
        }
    }

    public void SetData(string key, float value)
    {
        if (!_floatData.ContainsKey(key))
        {
            _floatData.Add(key, value);
        }
        else
        {
            _floatData[key] = value;
        }
    }

    public int GetIntData(string key)
    {
        if (!_intData.ContainsKey(key))
        {
            Debug.LogError($"Int数据中没有此{key}键");
            return 0;
        }

        return _intData[key];
    }

    public string GetStringData(string key)
    {
        if (!_stringData.ContainsKey(key))
        {
            Debug.LogError($"String数据中没有此{key}键");
            return "";
        }

        return _stringData[key];
    }

    public float GetFloatData(string key)
    {
        if (!_floatData.ContainsKey(key))
        {
            Debug.LogError($"Float数据中没有此{key}键");
            return 0;
        }

        return _floatData[key];
    }

    #region DataClass

    [Serializable]
    public class GameIntData : SerializationDic<string, int>
    {
    }

    [Serializable]
    public class GameStringData : SerializationDic<string, string>
    {
    }

    [Serializable]
    public class GameFloatData : SerializationDic<string, float>
    {
    }

    #endregion
}