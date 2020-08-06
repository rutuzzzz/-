using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameData
{
    private static Data _gameData = new Data();

    public static string jsonPath = "Assets/Resources/Data";

    public static string fileName = "/data.json";

    /// <summary>
    /// 是否加载了数据
    /// </summary>
    public static bool isLoadedData;

    #region DataFunction

    public static void SetData(this int target, string key)
    {
        _gameData.SetData(key, target);
    }

    public static int GetData(this int target, string key)
    {
        target = _gameData.GetIntData(key);
        return target;
    }

    public static void SetData(this string target, string key)
    {
        _gameData.SetData(key, target);
    }

    public static string GetData(this string target, string key)
    {
        target = _gameData.GetStringData(key);
        return target;
    }

    public static void SetData(this float target, string key)
    {
        _gameData.SetData(key, target);
    }

    public static float GetData(this float target, string key)
    {
        target = _gameData.GetFloatData(key);
        return target;
    }

    public static void SetData<TKey>(this List<TKey> target, string key)
    {
        string s = JsonUtility.ToJson(new Serialization<TKey>(target));
        s.SetData(key);
    }

    public static List<TKey> GetData<TKey>(this List<TKey> target, string key)
    {
        string s = "";
        target = JsonUtility.FromJson<Serialization<TKey>>(s.GetData(key)).ToList();
        return target;
    }

    public static void SetData<TKey, TValue>(this Dictionary<TKey, TValue> target, string key)
    {
        string s = JsonUtility.ToJson(new SerializationDic<TKey, TValue>(target));
        s.SetData(key);
    }

    public static Dictionary<TKey, TValue> GetData<TKey, TValue>(this Dictionary<TKey, TValue> target, string key)
    {
        string s = "";
        foreach (var item in JsonUtility.FromJson<SerializationDic<TKey, TValue>>(s.GetData(key)).ToDictionary())
        {
            target.Add(item.Key, item.Value);
        }

        //target = JsonUtility.FromJson<SerializationDic<TKey, TValue>>(s.GetData(key)).ToDictionary();

        return target;
    }

    #endregion


    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public static void SaveData()
    {
        jsonPath.CheckDirectory();

        using (FileStream fileStream = File.Open(jsonPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                Debug.Log(JsonUtility.ToJson(_gameData));
                streamWriter.Write(JsonUtility.ToJson(_gameData));
            }
        }
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public static void LoadData()
    {
        isLoadedData = true;
        //从文件中加载数据

        using (FileStream fileStream = File.Open(jsonPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                _gameData = JsonUtility.FromJson<Data>(streamReader.ReadToEnd());
                if (_gameData == null)
                {
                    _gameData = new Data();
                    Debug.LogError("加载本地数据为空");
                }
            }
        }
    }
}