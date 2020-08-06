using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Excel;
using ICSharpCode.SharpZipLib;
using UnityEditor;
using UnityEngine;
using LitJson;
using UnityEditor.Compilation;
using UnityEngine.Experimental.UIElements;

public class ExcelToJsonWindow : EditorWindow
{
    private static string path = "Assets/Excel/";
    private static readonly string jsonPath = "Assets/Resources/Generate/Json/";
    private static readonly string scriptPath = "Assets/Resources/Generate/Script/";
    private static readonly string scriptableObjectPath = "Assets/Resources/Generate/ScriptableObject/";
    private static int start_Colum;
    private static int start_Row;
    private static readonly string scriptableObjectName = "JsonDataGroup";

    [MenuItem("Tool/ExcelToJson")]
    static void Init()
    {
        ExcelToJsonWindow s = GetWindow<ExcelToJsonWindow>();
        s.minSize = s.maxSize = new Vector2(350, 500);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 30;
        GUI.skin.label.normal.textColor = Color.gray;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("数据编辑器");
        EditorGUILayout.LabelField("Excel路径:", EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect();
        if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited) &&
            rect.Contains(Event.current.mousePosition))
        {
            path = DragAndDrop.paths[0];
        }

        if (Path.GetFileName(path) != "")
        {
            path = path.Replace(Path.GetFileName(path), "");
        }

        path = EditorGUI.TextField(rect, path);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Json路径:", EditorStyles.boldLabel);
        EditorGUILayout.TextField(jsonPath);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Scripts路径:", EditorStyles.boldLabel);
        EditorGUILayout.TextField(scriptPath);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Asset路径:", EditorStyles.boldLabel);
        EditorGUILayout.TextField(scriptableObjectPath);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Excel数据起始行:");
        start_Row = EditorGUILayout.IntField(start_Row);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Excel数据起始列:");
        start_Colum = EditorGUILayout.IntField(start_Colum);

        GUILayout.Space(10);
        if (GUILayout.Button("读取Excel"))
        {
            ReadAllExcel();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("生成资源文件"))
        {
            AutoGenerateCode.CreatScriptableObject(scriptableObjectName, scriptableObjectPath);
        }

        EditorGUILayout.LabelField("Excel格式要求:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("表中必须有变量类型且位于首行，起始行从变量类型行开始计算", EditorStyles.boldLabel);
    }

    /// <summary>
    /// 读取所有的Excel文件
    /// </summary>
    void ReadAllExcel()
    {
        AutoGenerateCode.Clear();
        List<string> files = new List<string>();

        path.CheckDirectory();
        jsonPath.CheckDirectory();
        scriptPath.CheckDirectory();
        scriptableObjectPath.CheckDirectory();

        foreach (string file in Directory.GetFiles(path, "*.xlsx"))
        {
            files.Add(file);
        }

        foreach (string excelPath in files)
        {
            using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataSet result = excelDataReader.AsDataSet();
                AutoGenerateCode.CreatJsonByExcel("", jsonPath, result, start_Colum, start_Row);
                AutoGenerateCode.CreatAttributeClass(scriptPath, result, start_Colum, start_Row);
            }
        }

        AutoGenerateCode.CreatJsonDataGroup(scriptableObjectName, scriptPath, jsonPath);
        AssetDatabase.Refresh();
    }
}