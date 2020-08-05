#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleTool
{
    /// <summary>
    /// 读取资源路径中的所有文件
    /// </summary>
    public static void ReadAssetsFloder(this Dictionary<string, List<AssetFileInfo>> fileInfos, AssetFloderInfo asset)
    {
        if (asset.FileType == AssetType.File || asset.AssetInfoName.Equals("Plugins") || asset.AssetInfoName.Equals("ThirdParty"))
        {
            return;
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(asset.AssetInfoFullPath);
        FileSystemInfo[] fileSystemInfo = directoryInfo.GetFileSystemInfos();
        foreach (var info in fileSystemInfo)
        {
            if (info is DirectoryInfo)
            {
                if (IsValidFolder(info.Name))
                {
                    AssetFloderInfo floderInfo = new AssetFloderInfo(info.FullName, info.Name, false);
                    //设置父目录
                    floderInfo.parent = asset;
                    asset.ChildrenFiles.Add(floderInfo);

                    fileInfos.ReadAssetsFloder(floderInfo);
                }
            }
            else
            {
                if (!info.Extension.Equals(".meta"))
                {
                    AssetFileInfo fileInfo = new AssetFileInfo(info.FullName, info.Name, info.Extension);
                    //设置父目录
                    fileInfo.parent = asset;
                    asset.ChildrenFiles.Add(fileInfo);

                    AssetImporter importer = AssetImporter.GetAtPath(fileInfo.AssetInfoPath);
                    fileInfo.BundleName = importer.assetBundleName;
                    if (!string.IsNullOrEmpty(fileInfo.BundleName))
                    {
                        if (!fileInfos.ContainsKey(fileInfo.BundleName))
                        {
                            List<AssetFileInfo> infos = new List<AssetFileInfo>();
                            infos.Add(fileInfo);
                            fileInfos.Add(fileInfo.BundleName, infos);
                        }
                        else
                        {
                            fileInfos[fileInfo.BundleName].Add(fileInfo);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取所有被选取的资源文件
    /// </summary>
    public static void CheckIsCheckedFile(this List<AssetFileInfo> fileInfos, AssetFloderInfo baseInfo)
    {
        if (baseInfo == null)
        {
            throw new Exception("资源目录为空" + baseInfo.AssetInfoName);
        }

        fileInfos.Clear();
        for (int i = 0; i < baseInfo.ChildrenFiles.Count; i++)
        {
            if (baseInfo.ChildrenFiles[i].FileType == AssetType.File && baseInfo.ChildrenFiles[i].IsChecked)
            {
                fileInfos.Add((AssetFileInfo) baseInfo.ChildrenFiles[i]);
            }
            else if (baseInfo.ChildrenFiles[i].FileType == AssetType.Floder)
            {
                fileInfos.CheckIsCheckedFile((AssetFloderInfo) baseInfo.ChildrenFiles[i]);
            }
        }
    }

    /// <summary>
    /// 改变子目录中的选中状态
    /// </summary>
    /// <param name="infos"></param>
    public static void ChangeIsChecked(this AssetInfo infos, bool isChecked)
    {
        infos.IsChecked = isChecked;
        if (infos.FileType == AssetType.Floder)
        {
            AssetFloderInfo floder = (AssetFloderInfo) infos;
            for (int i = 0; i < floder.ChildrenFiles.Count; i++)
            {
                floder.ChildrenFiles[i].IsChecked = isChecked;
                floder.ChildrenFiles[i].ChangeIsChecked(isChecked);
            }
        }
        else if (infos.FileType == AssetType.File)
        {
        }
    }

    /// <summary>
    /// 改变子目录下所有文件夹的展开状态
    /// </summary>
    /// <param name="floderInfo"></param>
    /// <param name="isExpending"></param>
    public static void ChangeIsExpending(this AssetFloderInfo floderInfo, bool isExpending)
    {
        floderInfo.IsExpanding = isExpending;
        for (int i = 0; i < floderInfo.ChildrenFiles.Count; i++)
        {
            if (floderInfo.ChildrenFiles[i].FileType == AssetType.Floder)
            {
                AssetFloderInfo floderChildren = (AssetFloderInfo) floderInfo.ChildrenFiles[i];
                floderChildren.ChangeIsExpending(isExpending);
            }
        }
    }

    /// <summary>
    /// 改变父目录的展开状态
    /// </summary>
    public static void ChangeParentIsExpending(this AssetInfo info, bool isExpending)
    {
        if (info.parent != null)
        {
            info.parent.IsExpanding = isExpending;
            info.parent.ChangeParentIsExpending(true);
        }
    }

    /// <summary>
    /// 判断文件是否有效
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsValidFolder(string name)
    {
        if (name.Equals("StreamingAssets") || name.Equals("Editor"))
        {
            return false;
        }

        return true;
    }
}

/// <summary>
/// 管理所有的AB包信息
/// </summary>
public class AssetBundlesController
{
    public List<AssetBundleInfo> AssetBundleInfos;

    public Dictionary<string, List<AssetFileInfo>> AssetBundleFileInfos;

    public AssetBundlesController()
    {
        AssetBundleInfos = new List<AssetBundleInfo>();
        AssetBundleFileInfos = new Dictionary<string, List<AssetFileInfo>>();
    }
}

/// <summary>
/// AB包信息
/// </summary>
public class AssetBundleInfo
{
    public string ABName { get; set; }

    public List<AssetFileInfo> Assets;

    public AssetBundleInfo(string name)
    {
        ABName = name;
        Assets = new List<AssetFileInfo>();
    }

    /// <summary>
    /// 重命名AB包名
    /// </summary>
    public void ReNameAB(string newName)
    {
        ABName = newName;
        foreach (var item in Assets)
        {
            item.SetBundleName(ABName);
        }
    }

    /// <summary>
    /// 删除指定资源
    /// </summary>
    public void RemoveAssetFile(AssetFileInfo info)
    {
        info.SetBundleName("");
        Assets.Remove(info);
    }

    /// <summary>
    /// 清除所有此AB包中的资源
    /// </summary>
    public void RemoveAllAssetFile()
    {
        foreach (var item in Assets)
        {
            item.SetBundleName("");
        }

        Assets.Clear();
    }

    /// <summary>
    /// 对此AB包增加资源
    /// </summary>
    public void AddAssetFile(AssetFileInfo info)
    {
        info.SetBundleName(ABName);
        Assets.Add(info);
    }
}

/// <summary>
/// 资源抽象类
/// </summary>
public abstract class AssetInfo
{
    /// <summary>
    /// 父文件夹
    /// </summary>
    public AssetFloderInfo parent;

    /// <summary>
    /// 资源文件名
    /// </summary>
    public string AssetInfoName { get; set; }

    /// <summary>
    /// 资源文件全路径
    /// </summary>
    public string AssetInfoFullPath { get; set; }

    /// <summary>
    /// 资源文件Asset路径
    /// </summary>
    public string AssetInfoPath { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public AssetType FileType { get; protected set; }
}

/// <summary>
/// 资源文件类
/// </summary>
public class AssetFileInfo : AssetInfo
{
    /// <summary>
    /// 所属AB包的名字
    /// </summary>
    public string BundleName { get; set; }

    /// <summary>
    /// 唯一ID
    /// </summary>
    public string GUID { get; private set; }

    /// <summary>
    /// 资源文件类型
    /// </summary>
    public Type AssetFileType { get; set; }

    /// <summary>
    /// 是否是无效的资源
    /// </summary>
    public bool IsInValidFile { get; set; }

    public AssetFileInfo(string fullPath, string name, string expention)
    {
        AssetInfoFullPath = fullPath;
        AssetInfoName = name;
        AssetInfoPath = "Assets" + AssetInfoFullPath.Replace(Application.dataPath.Replace("/", "\\"), "");
        AssetFileType = AssetDatabase.GetMainAssetTypeAtPath(AssetInfoPath);
        FileType = AssetType.File;
        if (!string.IsNullOrEmpty(AssetInfoPath))
        {
            GUID = AssetDatabase.AssetPathToGUID(AssetInfoPath);
        }
        else
        {
            new Exception("路径错误!!!" + AssetInfoPath);
        }

        IsChecked = false;
    }

    /// <summary>
    /// 设置资源Bundle
    /// </summary>
    /// <param name="name"></param>
    public void SetBundleName(string name)
    {
        AssetImporter importer = AssetImporter.GetAtPath(AssetInfoPath);
        importer.SetAssetBundleNameAndVariant(name, "");
        BundleName = name;
    }
}

/// <summary>
/// 资源文件夹类
/// </summary>
public class AssetFloderInfo : AssetInfo
{
    /// <summary>
    /// 文件夹中的子资源
    /// </summary>
    public List<AssetInfo> ChildrenFiles;

    /// <summary>
    /// 是否已经展开
    /// </summary>
    public bool IsExpanding { get; set; }

    public AssetFloderInfo(string fullPath, string name, bool isExpanding)
    {
        AssetInfoFullPath = fullPath;
        AssetInfoName = name;
        IsExpanding = isExpanding;
        AssetInfoPath = "Assets" + AssetInfoFullPath.Replace(Application.dataPath.Replace("/", "\\"), "");
        FileType = AssetType.Floder;
        IsChecked = false;
        ChildrenFiles = new List<AssetInfo>();
    }
}

/// <summary>
/// 资源文件类型
/// </summary>
public enum AssetType
{
    File,
    Floder
}
#endif