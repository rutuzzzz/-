using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PathHelper
{
    /// <summary>
    /// 检查文件夹
    /// </summary>
    /// <param name="isCreat"></param>
    public static bool CheckDirectory(this string path, bool isCreat = true)
    {
        if (!Directory.Exists(path) && isCreat)
        {
            Directory.CreateDirectory(path);
            return true;
        }

        return false;
    }
}