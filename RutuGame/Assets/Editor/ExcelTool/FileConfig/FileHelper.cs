using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHelper
{
    /// <summary>
    /// 检查路径
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="isCreat">是否创建路径</param>
    /// <exception cref="Exception"></exception>
    public static void CheckPath(string path, bool isCreat = true)
    {
        if (!Directory.Exists(path))
        {
            if (isCreat)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                throw new Exception("没有此路径:" + path);
            }
        }
    }
    
    
}