using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Excel;
using UnityEditor;
using UnityEngine;

public class AutoGenerateCode
{
    private static List<string> attributeNames = new List<string>();
    private static List<string> attributeSubNames = new List<string>();

    public static void Clear()
    {
        attributeNames.Clear();
        attributeSubNames.Clear();
    }

    #region 自动生成

    /// <summary>
    /// 通过Excel创建Josn
    /// </summary>
    /// <param name="fileName">Json文件名</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="excelPath">Excel数据</param>
    /// <param name="startColumns">读取Excel起始列</param>
    /// <param name="startRows">读取Excel起始行</param>
    public static void CreatJsonByExcel(string fileName, string savePath, DataSet result, int startColumns, int startRows)
    {
        //FileHelper.CheckPath(excelPath, false);
        FileHelper.CheckPath(savePath);

        CreatJson(result, savePath, startColumns, startRows);
    }

    private static void CreatJson(DataSet result, string savePath, int startColumns, int startRows)
    {
        int colums = result.Tables[0].Columns.Count; //获取列数
        int rows = result.Tables[0].Rows.Count; //获取行数

        string fileName = result.Tables[0].Rows[3][0].ToString();
        string filePath = savePath + fileName + ConfigSuffix.json;
        attributeNames.Add(fileName);
        using (StreamWriter fileWriter = new StreamWriter(filePath))
        {
            fileWriter.Write("{");
            fileWriter.Write("\"");
            fileWriter.Write("Attribute");
            fileWriter.Write("\"");
            fileWriter.Write(":");
            fileWriter.Write("[");
            for (int i = startRows + 1; i < rows; i++)
            {
                fileWriter.Write("{");
                for (int j = startColumns; j < colums; j++)
                {
                    fileWriter.Write("\"");
                    fileWriter.Write(result.Tables[0].Rows[startRows][j]);
                    fileWriter.Write("\"");
                    fileWriter.Write(":");

                    fileWriter.Write("\"");
                    fileWriter.Write(result.Tables[0].Rows[i][j]);
                    fileWriter.Write("\"");

                    if (j != colums - 1)
                    {
                        fileWriter.Write(",");
                    }
                }

                fileWriter.Write("}");
                if (i != rows - 1)
                {
                    fileWriter.Write(",");
                }

                fileWriter.Write("\n");
            }

            fileWriter.Write("]");
            fileWriter.Write("}");
        }


        StreamReader reader = new StreamReader(filePath);
        string jsonData = reader.ReadToEnd();
        Debug.Log(jsonData);
        reader.Close();
    }

    /// <summary>
    /// 通过Excel创建属性类
    /// </summary>
    /// <param name="savePath">保存路径</param>
    /// <param name="result">Excel数据</param>
    public static void CreatAttributeClass(string savePath, DataSet result, int startColumns, int startRows)
    {
        string fileName = result.Tables[0].Rows[3][0].ToString();
        string filePath = savePath + fileName + ConfigSuffix.cs;
        Dictionary<string, string> typeNames = new Dictionary<string, string>();
        using (StreamWriter fileWriter = new StreamWriter(filePath))
        {
            fileWriter.Write("using System.Collections;\n");
            fileWriter.Write("using System.Collections.Generic;\n");
            fileWriter.Write("using UnityEngine;\n");
            fileWriter.Write("\n");
            fileWriter.Write("[System.Serializable]");
            fileWriter.Write("\n");
            //写入类名(与文件名相同)
            fileWriter.Write("public class ");
            fileWriter.Write(fileName);

            fileWriter.Write("\n{\n");
            for (int i = startColumns; i < result.Tables[0].Columns.Count; i++)
            {
                if (result.Tables[0].Rows[startRows - 1][i].ToString() != "" && result.Tables[0].Rows[startRows][i].ToString() != "")
                {
                    typeNames.Add(result.Tables[0].Rows[startRows][i].ToString(), result.Tables[0].Rows[startRows - 1][i].ToString());
                    fileWriter.Write("\tpublic " + result.Tables[0].Rows[startRows - 1][i] + " " +
                                     result.Tables[0].Rows[startRows][i] + ";\n");
                }
            }

            fileWriter.Write("\n}\n");
            fileWriter.Flush();
        }

        CreatAtrributeSubClass(fileName, savePath, typeNames);
    }

    /// <summary>
    /// 创建属性集合类
    /// </summary>
    /// <param name="typeName">属性名</param>
    /// <param name="savePath">保存路径</param>
    private static void CreatAtrributeSubClass(string typeName, string savePath, Dictionary<string, string> typeNames)
    {
        string attributeName = "_" + typeName.ToLower();
        string fileName = typeName + "Sub";
        string filePath = savePath + fileName + ".cs";
        attributeSubNames.Add(fileName);

        using (StreamWriter fileWriter = new StreamWriter(filePath))
        {
            fileWriter.Write("using System.Collections;\n");
            fileWriter.Write("using System.Collections.Generic;\n");
            fileWriter.Write("using UnityEngine;\n");
            fileWriter.Write("\n");
            fileWriter.Write("[System.Serializable]");
            fileWriter.Write("\n");
            //写入类名(与文件名相同)
            fileWriter.Write("public class ");
            fileWriter.Write(fileName);

            fileWriter.Write("\n{\n");

            fileWriter.Write("\tpublic ");
            fileWriter.Write(typeName + "[] ");
            fileWriter.Write("Attribute" + ";\n");

            foreach (KeyValuePair<string, string> item in typeNames)
            {
                fileWriter.Write("\tpublic ");
                fileWriter.Write(typeName);
                fileWriter.Write(" GetBy" + item.Key);
                fileWriter.Write("(");
                fileWriter.Write(item.Value + " " + item.Key.ToLower());
                fileWriter.Write(")\n");

                fileWriter.Write("\t{\n");
                fileWriter.Write("\t\tforeach(var item in ");
                fileWriter.Write("Attribute" + ")\n");

                fileWriter.Write("\t\t{\n");
                fileWriter.Write("\t\t\tif(item.");
                fileWriter.Write(item.Key + " == " + item.Key.ToLower());
                fileWriter.Write(")");
                fileWriter.Write("\n");

                fileWriter.Write("\t\t\t{\n");
                fileWriter.Write("\t\t\t\treturn item;\n");
                fileWriter.Write("\t\t\t}\n");
                fileWriter.Write("\t\t}\n");
                fileWriter.Write("\t\treturn null;\n");
                fileWriter.Write("\t}\n\n");
            }

            fileWriter.Write("}\n");
            fileWriter.Flush();
        }
    }

    /// <summary>
    /// 创建属性管理类
    /// </summary>
    /// <param name="fileName">文件名/param>
    /// <param name="savePath">保存路径</param>
    /// <param name="jsonPath">json文件路径</param>
    public static void CreatJsonDataGroup(string fileName, string savePath, string jsonPath)
    {
        string filePath = savePath + fileName + ".cs";
        using (StreamWriter fileWriter = new StreamWriter(filePath))
        {
            fileWriter.Write("using System.Collections;\n");
            fileWriter.Write("using System.Collections.Generic;\n");
            fileWriter.Write("using UnityEngine;\n");
            fileWriter.Write("using GameFramework;\n");
            fileWriter.Write("using LitJson;\n");
            fileWriter.Write("[System.Serializable]");
            fileWriter.Write("\n");
            //写入类名(与文件名相同)
            fileWriter.Write("public class ");
            fileWriter.Write(fileName);
            fileWriter.Write(" : ScriptableObject");

            fileWriter.Write("\n{\n");
            string typeSubName;
            string typeName;
            string attributeSubName;
            for (int i = 0; i < attributeSubNames.Count; i++)
            {
                typeSubName = Path.GetFileName(attributeSubNames[i]);
                fileWriter.Write("\tpublic ");
                fileWriter.Write(typeSubName);
                attributeSubName = " _" + typeSubName.ToLower();
                fileWriter.Write(attributeSubName + " = new " + typeSubName + "()");
                fileWriter.Write(";");
                fileWriter.Write("\n");
            }

            fileWriter.Write("\tprivate static ");
            fileWriter.Write(fileName + " instance;\n");
            fileWriter.Write("\n");
            fileWriter.Write("\tpublic static " + fileName + " Instance\n");
            fileWriter.Write("\t{\n");
            fileWriter.Write("\t\tget\n");
            fileWriter.Write("\t\t{\n");
            fileWriter.Write("\t\t\tif (instance != null)\n");
            fileWriter.Write("\t\t\t{\n");
            fileWriter.Write("\t\t\t\treturn instance;\n");
            fileWriter.Write("\t\t\t}\n");
            fileWriter.Write("\t\t\telse\n");
            fileWriter.Write("\t\t\t{\n");
            fileWriter.Write("\t\t\t\tDebug.Log(\"资源文件为空，重新查找\");\n");
            fileWriter.Write("\t\t\t\tinstance = UnityEditor.AssetDatabase.LoadAssetAtPath<JsonDataGroup>(\"Assets/Resources/Json/");
            fileWriter.Write(fileName + ".asset\");\n");
            fileWriter.Write("\t\t\t}\n");
            fileWriter.Write("\n");
            fileWriter.Write("\t\t\treturn instance;\n");
            fileWriter.Write("\t\t}\n");
            fileWriter.Write("\t}\n");

            fileWriter.Write("\n");
            fileWriter.Write("\tprivate void ");
            fileWriter.Write("OnEnable" + "()");
            fileWriter.Write("\n\t{");

            for (int i = 0; i < attributeNames.Count; i++)
            {
                typeSubName = Path.GetFileName(attributeSubNames[i]);
                typeName = Path.GetFileName(attributeNames[i]);
                attributeSubName = "_" + typeSubName.ToLower();
                fileWriter.Write("\n\t\t");
                fileWriter.Write(attributeSubName);
                fileWriter.Write(" = JsonUtility.FromJson<");
                fileWriter.Write(typeSubName + ">(");
                fileWriter.Write("IOBase.ReadTxt(");
                fileWriter.Write("\"");
                fileWriter.Write(jsonPath + typeName + ".json");
                fileWriter.Write("\"");
                fileWriter.Write(")");
                fileWriter.Write(");");
            }

            fileWriter.Write("\n\t}");

            fileWriter.Write("\n}\n");
            fileWriter.Flush();
        }
    }


    /// <summary>
    /// 生成资源文件
    /// </summary>
    /// <param name="fileNames"></param>
    /// <param name="filePath"></param>
    public static void CreatScriptableObject(string fileNames, string filePath)
    {
        ScriptableObject obj = ScriptableObject.CreateInstance(fileNames);

        AssetDatabase.CreateAsset(obj, filePath + fileNames + ConfigSuffix.asset);
        AssetDatabase.Refresh();
    }

    #endregion
}