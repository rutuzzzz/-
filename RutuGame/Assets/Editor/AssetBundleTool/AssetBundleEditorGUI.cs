#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class AssetBundleEditor : EditorWindow
{
    [MenuItem("Tool/AB管理工具")]
    static void Open()
    {
        AssetBundleEditor window = GetWindow<AssetBundleEditor>("AssetBundleEditor");
        window.minSize = new Vector2(1055, 730);
        window.maxSize = new Vector2(1055, 730);
        //window.position = new Rect(Screen.width / 2 - 100, Screen.height / 2, 0, 0);
        window.Init();
        window.Show();
    }

    //Assets文件基类
    private AssetFloderInfo _baseFloderInfo;

    //AB资源管理
    private AssetBundlesController _bundlesController;

    //已经勾选的文件资源
    private List<AssetFileInfo> _checkedFileInfos;

    //按钮样式
    private GUIStyle _preButton;
        
    //区域范围的样式
    private GUIStyle _box;

    private GUIStyle _prefabStyle;
    
    private GUIStyle _foldoutStyle;

    private GUIStyle _closeButton;

    private void Init()
    {
        _preButton = new GUIStyle("PreButton");
        _box = new GUIStyle("flow background");
        _prefabStyle = new GUIStyle("BoldLabel");
        _foldoutStyle = new GUIStyle("Foldout");
        _closeButton = new GUIStyle("OL Minus");

        
        _bundlesController = new AssetBundlesController();
        _baseFloderInfo = new AssetFloderInfo(Application.dataPath, "Assets", false);
        _checkedFileInfos = new List<AssetFileInfo>();
        //读取所有的资源目录文件
        _bundlesController.AssetBundleFileInfos.ReadAssetsFloder(_baseFloderInfo);

        //读取所有的AssetBundle名字
        foreach (var abName in AssetDatabase.GetAllAssetBundleNames())
        {
            if (!_bundlesController.AssetBundleFileInfos.ContainsKey(abName))
            {
                _bundlesController.AssetBundleFileInfos.Add(abName, new List<AssetFileInfo>());
            }

            AssetBundleInfo info = new AssetBundleInfo(abName);
            for (int i = 0; i < _bundlesController.AssetBundleFileInfos[info.ABName].Count; i++)
            {
                info.Assets.Add(_bundlesController.AssetBundleFileInfos[info.ABName][i]);
            }

            _bundlesController.AssetBundleInfos.Add(info);
        }
    }
    
    /// <summary>
    /// UI绘制
    /// </summary>
    void OnGUI()
    {
        //_bundlesController = new AssetBundlesController();
        TitleGUI();
        ABGUI();
        CurrentABGUI();
        AssetsGUI();
    }

    #region 标题按钮区域



    void TitleGUI()
    {
        _preButton.fixedHeight = 20;
        if (GUI.Button(new Rect(5, 5, 60, 25), "Creat", _preButton))
        {
            AssetBundleInfo abInfo = new AssetBundleInfo("ab" + _bundlesController.AssetBundleInfos.Count);
            _bundlesController.AssetBundleInfos.Add(abInfo);
        }

        GUI.enabled = _isCheckedAssetBundle;
        if (GUI.Button(new Rect(70, 5, 60, 25), "ReName", _preButton))
        {
            if (_isCheckedAssetBundle)
            {
                _isReName = true;
            }
        }

        if (GUI.Button(new Rect(135, 5, 60, 25), "Delete", _preButton))
        {
            AssetBundleInfo info = _bundlesController.AssetBundleInfos[_curABIndex];
            if (AssetDatabase.GetAllAssetBundleNames().Contains(info.ABName))
            {
                AssetDatabase.RemoveAssetBundleName(info.ABName, true);
            }

            _bundlesController.AssetBundleInfos.Remove(info);
            info.RemoveAllAssetFile();
            _isCheckedAssetBundle = false;
            _isCheckedCurABAssetFile = false;
        }

        if (GUI.Button(new Rect(200, 5, 60, 25), "Add", _preButton))
        {
            if (_isCheckedAssetBundle)
            {
                _checkedFileInfos.CheckIsCheckedFile(_baseFloderInfo);
                for (int i = 0; i < _checkedFileInfos.Count; i++)
                {
                    if (string.IsNullOrEmpty(_checkedFileInfos[i].BundleName))
                    {
                        _bundlesController.AssetBundleInfos[_curABIndex].AddAssetFile(_checkedFileInfos[i]);
                        _checkedFileInfos[i].IsChecked = false;
                    }
                }
            }
        }

        GUI.enabled = true;
//        string[] s = new[] {"1", "2", "3"};
//        EditorGUI.Popup(new Rect(900, 30, 70, 30), 0, s);
//        if (GUI.Button(new Rect(975, 30, 75, 25), "Build", _preButton))
//        {
//        }
    }

    #endregion

    #region AB包总览区域

    //区域视图的范围
    private Rect _abViewRect;

    //滚动区域的范围
    private Rect _abScrollRect;

    //区域视图的高度
    private int _abViewHeght = 0;



    //当前选中的AB包索引
    private int _curABIndex = 0;

    //是否选中AB包
    private bool _isCheckedAssetBundle;

    //是否重命名
    private bool _isReName;

    //AB总览视图滚动区域所在位置
    private Vector2 _ABScrollPos;

    string _reName = "";

    /// <summary>
    /// AB资源总览视图
    /// </summary>
    void ABGUI()
    {
        
        _abViewRect = new Rect(5, 30, 255, 350);
        _abScrollRect = new Rect(5, 30, 255, _abViewHeght);

        _ABScrollPos = GUI.BeginScrollView(_abViewRect, _ABScrollPos, _abScrollRect);
        GUI.BeginGroup(_abScrollRect, _box);

        _abViewHeght = 5;

        for (int i = 0; i < _bundlesController.AssetBundleInfos.Count; i++)
        {
            GUIContent content = EditorGUIUtility.IconContent("Prefab Icon");
            content.text = _bundlesController.AssetBundleInfos[i].ABName;

            if (_isCheckedAssetBundle)
            {
                if (_curABIndex == i)
                {
                    if (_isReName)
                    {
                        content.text = "";
                        _reName = _reName.ToLower();
                        _reName = GUI.TextField(new Rect(5, _abViewHeght, 155, 25), _reName, 15);
                        if (GUI.Button(new Rect(165, _abViewHeght, 40, 25), "OK"))
                        {
                            content.text = _reName.ToLower();
                            _isReName = false;
                            //删除之前的AB包名
                            AssetBundleInfo info = _bundlesController.AssetBundleInfos[_curABIndex];
                            AssetDatabase.RemoveAssetBundleName(info.ABName, true);
                            //重置名字
                            _bundlesController.AssetBundleInfos[_curABIndex].ReNameAB(_reName.ToLower());
                        }

                        if (GUI.Button(new Rect(210, _abViewHeght, 40, 25), "NO"))
                        {
                            content.text = _bundlesController.AssetBundleInfos[i].ABName;
                            _isReName = false;
                        }
                    }
                    else
                    {
                        GUI.Box(new Rect(5, _abViewHeght, 255, 25), "", new GUIStyle("OL SelectedRow"));
                        if (GUI.Button(new Rect(5, _abViewHeght, 220, 25), content, _prefabStyle))
                        {
                            _isCheckedCurABAssetFile = false;
                        }
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(5, _abViewHeght, 220, 25), content, _prefabStyle))
                    {
                        _isCheckedCurABAssetFile = false;
                        _isCheckedAssetBundle = true;
                        _curABIndex = i;
                    }
                }
            }
            else
            {
                if (GUI.Button(new Rect(5, _abViewHeght, 220, 25), content, _prefabStyle))
                {
                    _isCheckedAssetBundle = true;
                    _curABIndex = i;
                }
            }

            _abViewHeght += 25;
        }

        if (_abViewHeght < _abViewRect.height)
        {
            _abViewHeght = (int) _abViewRect.height;
        }

        GUI.EndGroup();
        GUI.EndScrollView();
    }

    #endregion

    #region 当前AB包资源显示区域

    //当前AB显示视图
    private Rect _curABViewRect;

    //当前AB滚动区域
    private Rect _curABScrollRect;

    //当前AB视图高度
    private int _curABViewHeight = 0;

    //是否选中了当前AB包的资源
    private bool _isCheckedCurABAssetFile;

    //当前选中的资源
    private AssetFileInfo _curAssetFileInfo;

    //当前AB滚动区域所在位置
    private Vector2 _curABViewScrollPos;


    /// <summary>
    /// 当前AB资源视图
    /// </summary>
    void CurrentABGUI()
    {
        _curABViewRect = new Rect(5, 385, 255, 345);
        _curABScrollRect = new Rect(5, 385, 255, _curABViewHeight);

        _curABViewScrollPos = GUI.BeginScrollView(_curABViewRect, _curABViewScrollPos, _curABScrollRect);
        GUI.BeginGroup(_curABScrollRect, _box);


        _curABViewHeight = 5;
        if (_isCheckedAssetBundle)
        {
            for (int i = 0; i < _bundlesController.AssetBundleInfos[_curABIndex].Assets.Count; i++)
            {
                GUIContent content = EditorGUIUtility.ObjectContent(null,
                    _bundlesController.AssetBundleInfos[_curABIndex].Assets[i].AssetFileType);
                content.text = _bundlesController.AssetBundleInfos[_curABIndex].Assets[i].AssetInfoName;
                GUI.Label(new Rect(5, _curABViewHeight, 220, 25), content, _prefabStyle);
                if (GUI.Button(new Rect(5, _curABViewHeight, 210, 25), "", GUIStyle.none))
                {
                    _baseFloderInfo.ChangeIsExpending(false);
                    _bundlesController.AssetBundleInfos[_curABIndex].Assets[i].ChangeParentIsExpending(true);
                    _isCheckedCurABAssetFile = true;
                    _curAssetFileInfo = _bundlesController.AssetBundleInfos[_curABIndex].Assets[i];
                }

                if (GUI.Button(new Rect(230, _curABViewHeight + 5, 25, 25), "", _closeButton))
                {
                    _bundlesController.AssetBundleInfos[_curABIndex].RemoveAssetFile(_bundlesController.AssetBundleInfos[_curABIndex].Assets[i]);
                    _isCheckedCurABAssetFile = false;
                }

                _curABViewHeight += 25;
            }
        }

        if (_curABViewHeight < _curABViewRect.height)
        {
            _curABViewHeight = (int) _curABViewRect.height;
        }

        GUI.EndGroup();
        GUI.EndScrollView();
    }

    #endregion

    #region 资源目录区域

    //当前资源目录区域
    private Rect _assetsViewRect;

    //当前资源目录滚动区域
    private Rect _assetsScrollRect;

    //当前资源视图高度
    private int _assetsViewHeight = 0;

    //是否改变了资源文件的勾选
    private bool _isFileCheckChanged;

    private Vector2 _curAssetsScrollPos;

    /// <summary>
    /// 文件视图
    /// </summary>
    void AssetsGUI()
    {
        _assetsViewRect = new Rect(265, 55, 785, 675);
        _assetsScrollRect = new Rect(265, 55, 785, _assetsViewHeight);

        _curAssetsScrollPos = GUI.BeginScrollView(_assetsViewRect, _curAssetsScrollPos, _assetsScrollRect);
        GUI.BeginGroup(_assetsScrollRect, _box);
        _assetsViewHeight = 10;

        GUILayout.Label("", GUILayout.Height(5));
        DrawAssetGUI(_baseFloderInfo, 0);
        
        if (_assetsViewHeight < _assetsViewRect.height)
        {
            _assetsViewHeight = (int) _assetsViewRect.height;
        }

        GUI.EndGroup();
        GUI.EndScrollView();
    }

    /// <summary>
    /// 画出资源目录图标
    /// </summary>
    void DrawAssetGUI(AssetInfo assetInfo, int index)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(index * 20 + 5);
        _assetsViewHeight += 22;
        if (assetInfo.FileType == AssetType.Floder)
        {
            AssetFloderInfo info = assetInfo as AssetFloderInfo;
            if (GUILayout.Toggle(assetInfo.IsChecked, "", GUILayout.Width(20)) != assetInfo.IsChecked)
            {
                assetInfo.ChangeIsChecked(!assetInfo.IsChecked);
            }

            GUILayout.Space(1);
            GUIContent content = EditorGUIUtility.IconContent("Folder Icon");
            content.text = assetInfo.AssetInfoName;
            info.IsExpanding = EditorGUILayout.Foldout(info.IsExpanding, content, _foldoutStyle);
            GUILayout.EndHorizontal();
            if (info.IsExpanding)
            {
                for (int i = 0; i < info.ChildrenFiles.Count; i++)
                {
                    DrawAssetGUI(info.ChildrenFiles[i], index + 1);
                }
            }
        }
        else if (assetInfo.FileType == AssetType.File)
        {
            AssetFileInfo info = assetInfo as AssetFileInfo;
            GUI.enabled = string.IsNullOrEmpty(info.BundleName);
            if (GUILayout.Toggle(assetInfo.IsChecked, "", GUILayout.Width(20)) != assetInfo.IsChecked)
            {
                assetInfo.IsChecked = !assetInfo.IsChecked;
                _isFileCheckChanged = true;
            }

            GUILayout.Space(2);


            GUIContent content = EditorGUIUtility.ObjectContent(null, info.AssetFileType);
            content.text = assetInfo.AssetInfoName;
            if (_isCheckedCurABAssetFile && _curAssetFileInfo.GUID == info.GUID)
            {
                GUILayout.Label(content, GUILayout.Height(40));
            }
            else
            {
                GUILayout.Label(content, GUILayout.Height(20));
            }

            if (!string.IsNullOrEmpty(info.BundleName))
            {
                GUILayout.Label("[" + info.BundleName + "]", _prefabStyle);
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }

    #endregion
}
#endif