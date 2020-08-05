using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GameData
{
    public static Dictionary<string, int> intValues = new Dictionary<string, int>();

    /// <summary>
    /// 设置游戏数据
    /// </summary>
    public static void SetData(this int target, string keyName)
    {
        if (!intValues.ContainsKey(keyName))
        {
            intValues.Add(keyName, target);
        }
        else
        {
            intValues[keyName] = target;
        }
    }

    /// <summary>
    /// 获取游戏数据
    /// </summary>
    public static int GetData(this int target, string keyName)
    {
        if (!intValues.ContainsKey(keyName))
        {
            Debug.LogError($"Int数据中没有此{keyName}键");
        }
        else
        {
            target = intValues[keyName];
        }

        return target;
    }
}