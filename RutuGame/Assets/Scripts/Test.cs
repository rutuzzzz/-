using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int i;

    private void Awake()
    {
        GameData.LoadData();
        List<A> aa = new List<A>();
        aa.GetData("li");

        Dictionary<int, A> dic = new Dictionary<int, A>();
//        dic.Add(1, new A("a"));
//        dic.Add(2, new A("b"));
//        dic.Add(3, new A("c"));
//        dic.Add(4, new A("d"));
        //dic.SetData("ab");
        dic.GetData("ab");
        foreach (var item in aa)
        {
            Debug.Log(item.b);
        }

        foreach (var item in dic)
        {
            Debug.Log(item.Key + "-" + item.Value);
        }
        //GameData.SaveData();
//        
//        Debug.Log(i.GetData("s2"));
//        Debug.Log(i.GetData("s3"));
//        foreach (var item in GameData.intValues)
//        {
//            Debug.Log(item.Key + "-" + item.Value);
//        }

//        for (int i = 0; i < 10; i++)
//        {
//            i.SetData($"s{i}");
//        }
//
//        string s;
//        for (int i = 0; i < 10; i++)
//        {
//            s = $"z{i}";
//            s.SetData($"d{i}");
//        }

        GameData.SaveData();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Dictionary<int, int> test(Dictionary<int, int> a)
    {
        a= new Dictionary<int, int>();
        return a;
    }
}

[Serializable]
public class A
{
    public string b = "1";

    public A(string s)
    {
        b = s;
    }
}

[Serializable]
public class G
{
    public GG g = new GG();
}

[Serializable]
public class GG : SerializationDic<string, int>
{
//    public List<string> d = new List<string>();
//    public List<int> c = new List<int>();
//
//    public void Add(string a, int b)
//    {
//        d.Add(a);
//        c.Add(b);
//    }
}

public class GGG<T, K>
{
    public List<T> d = new List<T>();
    public List<K> c = new List<K>();

    public Dictionary<T, K> dic = new Dictionary<T, K>();

    public void Add(T a, K b)
    {
        d.Add(a);
        c.Add(b);
    }
}