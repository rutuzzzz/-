using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GameData
{
    /// <summary>
    /// 是否加载了数据
    /// </summary>
    public static bool isLoadedData;

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public static void SaveData()
    {
        //JsonUtility
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    public static void LoadData()
    {
        isLoadedData = true;
        //从文件中加载数据
    }
}