using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LitJson
{
    public class JsonData : IJsonWrapper, IList, IOrderedDictionary, IDictionary, ICollection, IEnumerable, IEquatable<JsonData>
    {
        public IList<JsonData> inst_array;

        private bool inst_boolean;

        private double inst_double;

        private int inst_int;

        private uint inst_uint;

        private long inst_long;

        private IDictionary<string, JsonData> inst_object;

        private string inst_string;

        private string m_strJson;

        private JsonType type;

        private IList<KeyValuePair<string, JsonData>> object_list;

        private string m_strFilePath;

        private List<JsonData> m_listParent = new List<JsonData>();    //同一个JsonData的引用，可能被加在多个Parent下，此处都做存放

        public IDictionary<string, JsonData> Inst_object
        {
            get
            {
                return this.inst_object;
            }
        }

        public int Count
        {
            get
            {
                return this.EnsureCollection().Count;
            }
        }

        public bool IsArray
        {
            get
            {
                return this.type == JsonType.Array;
            }
        }

        public bool IsBoolean
        {
            get
            {
                return this.type == JsonType.Boolean;
            }
        }

        public bool IsDouble
        {
            get
            {
                return this.type == JsonType.Double;
            }
        }

        public bool IsInt
        {
            get
            {
                return this.type == JsonType.Int;
            }
        }

        public bool IsUint
        {
            get
            {
                return this.type == JsonType.Uint;
            }
        }

        public bool IsLong
        {
            get
            {
                return this.type == JsonType.Long;
            }
        }

        public bool IsObject
        {
            get
            {
                return this.type == JsonType.Object;
            }
        }

        public bool IsString
        {
            get
            {
                return this.type == JsonType.String;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.EnsureCollection().IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.EnsureCollection().SyncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return this.EnsureDictionary().IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.EnsureDictionary().IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                this.EnsureDictionary();
                IList<string> list = new List<string>();
                foreach (KeyValuePair<string, JsonData> current in this.object_list)
                {
                    list.Add(current.Key);
                }
                return (ICollection)list;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                this.EnsureDictionary();
                IList<JsonData> list = new List<JsonData>();
                foreach (KeyValuePair<string, JsonData> current in this.object_list)
                {
                    list.Add(current.Value);
                }
                return (ICollection)list;
            }
        }

        bool IJsonWrapper.IsArray
        {
            get
            {
                return this.IsArray;
            }
        }

        bool IJsonWrapper.IsBoolean
        {
            get
            {
                return this.IsBoolean;
            }
        }

        bool IJsonWrapper.IsDouble
        {
            get
            {
                return this.IsDouble;
            }
        }

        bool IJsonWrapper.IsInt
        {
            get
            {
                return this.IsInt;
            }
        }

        bool IJsonWrapper.IsUint
        {
            get
            {
                return this.IsUint;
            }
        }

        bool IJsonWrapper.IsLong
        {
            get
            {
                return this.IsLong;
            }
        }

        bool IJsonWrapper.IsObject
        {
            get
            {
                return this.IsObject;
            }
        }

        bool IJsonWrapper.IsString
        {
            get
            {
                return this.IsString;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.EnsureList().IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.EnsureList().IsReadOnly;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this.EnsureDictionary()[key];
            }
            set
            {
                if (!(key is string))
                {
                    JsonException.Throw(new ArgumentException("The key has to be a string"));
                }
                JsonData value2 = this.ToJsonData(value);
                this[(string)key] = value2;
            }
        }

        object IOrderedDictionary.this[int idx]
        {
            get
            {
                this.EnsureDictionary();
                return this.object_list[idx].Value;
            }
            set
            {
                this.EnsureDictionary();
                JsonData value2 = this.ToJsonData(value);
                value2.OnAddToParent(this);
                KeyValuePair<string, JsonData> keyValuePair = this.object_list[idx];
                this.inst_object[keyValuePair.Key] = value2;
                KeyValuePair<string, JsonData> value3 = new KeyValuePair<string, JsonData>(keyValuePair.Key, value2);
                this.object_list[idx] = value3;
                this.json = null;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.EnsureList()[index];
            }
            set
            {
                this.EnsureList();
                JsonData value2 = this.ToJsonData(value);
                this[index] = value2;
            }
        }

        public JsonData this[string prop_name]
        {
            get
            {
                try
                {
                    if (this.IsEmpty())
                    {
                        Debug.AssertFormat(false, "\n====Json asset====\nKey:[{0}]\n====Json asset====\n, data is empty\n", prop_name);
                        return CreateEmptyObj();
                    }

                    this.EnsureDictionary();
                    return this.inst_object[prop_name];
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("\n====Json asset====\nKey:[{0}]\n====Json asset====\n, Exception Msg:[{1}]\n", prop_name, ex.ToString());
                    return CreateEmptyObj();
                }
            }
            set
            {
                try
                {
                    this.EnsureDictionary();
                    value.OnAddToParent(this);
                    KeyValuePair<string, JsonData> keyValuePair = new KeyValuePair<string, JsonData>(prop_name, value);
                    if (this.inst_object.ContainsKey(prop_name))
                    {
                        for (int i = 0; i < this.object_list.Count; i++)
                        {
                            if (this.object_list[i].Key == prop_name)
                            {
                                this.object_list[i] = keyValuePair;
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.object_list.Add(keyValuePair);
                    }

                    this.inst_object[prop_name] = value;
                    this.json = null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("\n====Json asset====\nKey:[{0}]\n====Json asset====\n, Exception Msg:[{1}]\n", prop_name, ex.ToString());
                }
            }
        }

        public JsonData this[int index]
        {
            get
            {
                try
                {
                    if (this.IsEmpty())
                    {
                        Debug.AssertFormat(false, "\n====Json asset====\nIndex:[{0}]\n====Json asset====\n, data is empty\n", index);
                        return CreateEmptyObj();
                    }

                    this.EnsureCollection();
                    JsonData result;
                    if (this.type == JsonType.Array)
                    {
                        result = this.inst_array[index];
                    }
                    else
                    {
                        result = this.object_list[index].Value;
                    }
                    return result;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("\n====Json asset====\nIndex:[{0}]\n====Json asset====\n, Exception Msg:[{1}]\n", index, ex.ToString());
                    return CreateEmptyObj();
                }
            }
            set
            {
                try
                {
                    this.EnsureCollection();
                    value.OnAddToParent(this);
                    if (this.type == JsonType.Array)
                    {
                        this.inst_array[index] = value;
                    }
                    else
                    {
                        KeyValuePair<string, JsonData> keyValuePair = this.object_list[index];
                        KeyValuePair<string, JsonData> value2 = new KeyValuePair<string, JsonData>(keyValuePair.Key, value);
                        this.object_list[index] = value2;
                        this.inst_object[keyValuePair.Key] = value;
                    }

                    this.json = null;
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("\n====Json asset====\nIndex:[{0}]\n====Json asset====\n, Exception Msg:[{1}]\n", index, ex.ToString());
                }
            }
        }

        public JsonData()
        {
        }

        public JsonData(bool boolean)
        {
            this.AsBool = boolean;
        }

        public JsonData(double number)
        {
            this.AsDouble = number;
        }

        public JsonData(int number)
        {
            this.AsInt = number;
        }

        public JsonData(uint number)
        {
            this.AsUint = number;
        }

        public JsonData(long number)
        {
            this.AsLong = number;
        }

        public JsonData(object obj)
        {
            if (obj is bool)
            {
                this.AsBool = (bool)obj;
            }
            else if (obj is double)
            {
                this.AsDouble = (double)obj;
            }
            else if (obj is int)
            {
                this.AsInt = (int)obj;
            }
            else if (obj is uint)
            {
                this.AsUint = (uint)obj;
            }
            else if (obj is long)
            {
                this.AsLong = (long)obj;
            }
            else if (obj is float)
            {
                this.AsFloat = (float)obj;
            }
            else
            {
                if (!(obj is string))
                {
                    JsonException.Throw(new ArgumentException("Unable to wrap the given object with JsonData"));
                }
                this.type = JsonType.String;
                this.inst_string = (string)obj;
            }
        }

        public JsonData(string str)
        {
            this.AsString = str;
        }

        public static implicit operator JsonData(bool data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(double data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(int data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(uint data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(long data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(string data)
        {
            return new JsonData(data);
        }

        public static explicit operator bool(JsonData data)
        {
            return data.AsBool;
        }

        public static explicit operator double(JsonData data)
        {
            return data.AsDouble;
        }

        public static explicit operator float(JsonData data)
        {
            return data.AsFloat;
        }

        public static explicit operator int(JsonData data)
        {
            return data.AsInt;
        }

        public static explicit operator uint (JsonData data)
        {
            return data.AsUint;
        }

        public static explicit operator long(JsonData data)
        {
            return data.AsLong;
        }

        public static explicit operator string(JsonData data)
        {
            return data.AsString;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.EnsureCollection().CopyTo(array, index);
        }

        void IDictionary.Add(object key, object value)
        {
            if (!(key is string))
            {
                JsonException.Throw(new ArgumentException("the key is invalid"));
            }

            string strKey = (string)key;
            this.Add(strKey, value);
        }

        void IDictionary.Clear()
        {
            IDictionary pDictionary = this.EnsureDictionary();
            if (null != pDictionary)
            {
                foreach (ICollection item in pDictionary.Values)
                {
                    JsonData pData = item as JsonData;
                    if (null == pData)
                    {
                        continue;
                    }

                    pData.OnRemoveFromParent(this);
                }

                pDictionary.Clear();
            }

            if (null != this.object_list)
            {
                foreach (var item in this.object_list)
                {
                    JsonData pData = item.Value;
                    pData.OnRemoveFromParent(this);
                }

                this.object_list.Clear();
            }

            this.json = null;
        }

        bool IDictionary.Contains(object key)
        {
            return this.EnsureDictionary().Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            IDictionary pDictionary = this.EnsureDictionary();
            if (null != pDictionary && pDictionary.Contains(key))
            {
                JsonData pData = pDictionary[key] as JsonData;
                if (null != pData)
                {
                    pData.OnRemoveFromParent(this);
                }

                pDictionary.Remove(key);
            }

            for (int i = 0; i < this.object_list.Count; i++)
            {
                if (this.object_list[i].Key == (string)key)
                {
                    JsonData pData = this.object_list[i].Value as JsonData;
                    if (null != pData)
                    {
                        pData.OnRemoveFromParent(this);
                    }

                    this.object_list.RemoveAt(i);
                    break;
                }
            }
            this.json = null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.EnsureCollection().GetEnumerator();
        }

        public bool AsBool
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        {
                            bool Result;
                            if (!bool.TryParse(this.AsString, out Result))
                            {
                                break;
                            }

                            return Result;
                        }
                    case JsonType.Boolean:
                        return this.inst_boolean;
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold a boolean"));
                return default(bool);
            }

            set
            {
                this.type = JsonType.Boolean;
                this.inst_boolean = value;
                this.json = null;
            }
        }

        public float AsFloat
        {
            get
            {
                double dResult = this.AsDouble;
                return (float)dResult;
            }

            set
            {
                this.AsDouble = value;
            }
        }

        public double AsDouble
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        {
                            double Result;
                            if (!double.TryParse(this.AsString, out Result))
                            {
                                break;
                            }

                            return Result;
                        }
                    case JsonType.Int:
                        return this.AsInt;
                    case JsonType.Uint:
                        return this.AsUint;
                    case JsonType.Long:
                        return this.AsLong;
                    case JsonType.Double:
                        return this.inst_double;
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold a double"));
                return default(double);
            }

            set
            {
                this.type = JsonType.Double;
                this.inst_double = value;
                this.json = null;
            }
        }

        public int AsInt
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        {
                            //对于配置成XX.XX的字符串，依然允许转换为整形输出。此处不用int.TryParse的方式，以免解析失败
                            return (int)this.AsDouble;
                        }
                    case JsonType.Int:
                        return this.inst_int;
                    case JsonType.Uint:
                        return (int)this.AsUint;
                    case JsonType.Long:
                        return (int)this.AsLong;
                    case JsonType.Double:
                        return (int)this.AsDouble;
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold an int"));
                return default(int);
            }

            set
            {
                this.type = JsonType.Int;
                this.inst_int = value;
                this.json = null;
            }
        }

        public uint AsUint
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        {
                            //对于配置成XX.XX的字符串，依然允许转换为整形输出。此处不用int.TryParse的方式，以免解析失败
                            return (uint)this.AsDouble;
                        }
                    case JsonType.Int:
                        return (uint)this.AsInt;
                    case JsonType.Uint:
                        return this.inst_uint;
                    case JsonType.Long:
                        return (uint)this.AsLong;
                    case JsonType.Double:
                        return (uint)this.AsDouble;
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold an uint"));
                return default(uint);
            }

            set
            {
                this.type = JsonType.Uint;
                this.inst_uint = value;
                this.json = null;
            }
        }

        public long AsLong
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        {
                            //对于配置成XX.XX的字符串，依然允许转换为整形输出。此处不用long.TryParse的方式，以免解析失败
                            return (long)this.AsDouble;
                        }
                    case JsonType.Int:
                        return this.AsInt;
                    case JsonType.Uint:
                        return (long)this.AsUint;
                    case JsonType.Long:
                        return this.inst_long;
                    case JsonType.Double:
                        return (long)this.AsDouble;
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold a long"));
                return default(long);
            }

            set
            {
                this.type = JsonType.Long;
                this.inst_long = value;
                this.json = null;
            }
        }

        public string AsString
        {
            get
            {
                switch (this.type)
                {
                    case JsonType.String:
                        return this.inst_string;
                    case JsonType.Int:
                    case JsonType.Uint:
                    case JsonType.Long:
                    case JsonType.Double:
                    case JsonType.Boolean:
                        return this.ToString();
                    default:
                        break;
                }

                JsonException.Throw(new InvalidOperationException("JsonData instance doesn't hold a string"));
                return string.Empty;
            }

            set
            {
                this.type = JsonType.String;
                this.inst_string = value;
                this.json = null;
            }
        }

        bool IJsonWrapper.GetBoolean()
        {
            return this.AsBool;
        }

        double IJsonWrapper.GetDouble()
        {
            return this.AsDouble;
        }

        int IJsonWrapper.GetInt()
        {
            return this.AsInt;
        }

        uint IJsonWrapper.GetUint()
        {
            return this.AsUint;
        }

        long IJsonWrapper.GetLong()
        {
            return this.AsLong;
        }

        string IJsonWrapper.GetString()
        {
            return this.AsString;
        }

        void IJsonWrapper.SetBoolean(bool val)
        {
            this.AsBool = val;
        }

        void IJsonWrapper.SetDouble(double val)
        {
            this.AsDouble = val;
        }

        void IJsonWrapper.SetInt(int val)
        {
            this.AsInt = val;
        }

        void IJsonWrapper.SetLong(long val)
        {
            this.AsLong = val;
        }

        void IJsonWrapper.SetString(string val)
        {
            this.AsString = val;
        }

        string IJsonWrapper.ToJson()
        {
            return this.ToJson();
        }

        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            this.ToJson(writer);
        }

        int IList.Add(object value)
        {
            return this.Add(value);
        }

        void IList.Clear()
        {
            IList pList = this.EnsureList();
            if (null != pList)
            {
                for (int i = 0; i < pList.Count; ++i)
                {
                    JsonData pData = pList[i] as JsonData;
                    if (null == pData)
                    {
                        continue;
                    }

                    pData.OnRemoveFromParent(this);
                }

                pList.Clear();
            }

            this.json = null;
        }

        bool IList.Contains(object value)
        {
            return this.EnsureList().Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.EnsureList().IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            JsonData objJsonValue = this.ToJsonData(value);
            objJsonValue.OnAddToParent(this);
            this.EnsureList().Insert(index, objJsonValue);
            this.json = null;
        }

        void IList.Remove(object value)
        {
            IList pList = this.EnsureList();
            if (null != pList)
            {
                for (int n = pList.Count - 1; n >= 0; --n)
                {
                    JsonData pData = pList[n] as JsonData;
                    if (null == pData)
                    {
                        continue;
                    }

                    if (!pData.EqualsTo(value))
                    {
                        continue;
                    }

                    pData.OnRemoveFromParent(this);
                    pList.RemoveAt(n);
                }
            }

            this.json = null;
        }

        void IList.RemoveAt(int index)
        {
            IList pList = this.EnsureList();
            if (null != pList
                && index >= 0
                && index < pList.Count)
            {
                JsonData pData = pList[index] as JsonData;
                if (null != pData)
                {
                    pData.OnRemoveFromParent(this);
                }

                pList.RemoveAt(index);
            }

            this.json = null;
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            this.EnsureDictionary();
            return new OrderedDictionaryEnumerator(this.object_list.GetEnumerator());
        }

        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            string text = (string)key;
            JsonData value2 = this.ToJsonData(value);
            value2.OnAddToParent(this);
            this[text] = value2;
            KeyValuePair<string, JsonData> item = new KeyValuePair<string, JsonData>(text, value2);
            this.object_list.Insert(idx, item);
        }

        void IOrderedDictionary.RemoveAt(int idx)
        {
            this.EnsureDictionary();

            //inst_object
            JsonData pData;
            this.inst_object.TryGetValue(this.object_list[idx].Key, out pData);
            if (null != pData)
            {
                pData.OnRemoveFromParent(this);
            }

            this.inst_object.Remove(this.object_list[idx].Key);

            //object_list
            if (idx >= 0 && idx < this.object_list.Count)
            {
                pData = this.object_list[idx].Value;
                if (null != pData)
                {
                    pData.OnRemoveFromParent(this);
                }

                this.object_list.RemoveAt(idx);
            }
        }

        private ICollection EnsureCollection()
        {
            ICollection result;
            if (this.type == JsonType.Array)
            {
                result = (ICollection)this.inst_array;
            }
            else
            {
                if (this.type != JsonType.Object)
                {
                    JsonException.Throw(new InvalidOperationException("The JsonData instance has to be initialized first"));
                }
                result = (ICollection)this.inst_object;
            }
            return result;
        }

        private IDictionary EnsureDictionary()
        {
            IDictionary result;
            if (this.type == JsonType.Object)
            {
                result = (IDictionary)this.inst_object;
            }
            else
            {
                if (this.type != JsonType.None)
                {
                    JsonException.Throw(new InvalidOperationException("Instance of JsonData is not a dictionary"));
                }
                this.type = JsonType.Object;
                this.inst_object = new Dictionary<string, JsonData>();
                this.object_list = new List<KeyValuePair<string, JsonData>>();
                result = (IDictionary)this.inst_object;
            }
            return result;
        }

        private IList EnsureList()
        {
            IList result;
            if (this.type == JsonType.Array)
            {
                result = (IList)this.inst_array;
            }
            else
            {
                if (this.type != JsonType.None)
                {
                    JsonException.Throw(new InvalidOperationException("Instance of JsonData is not a list"));
                }
                this.type = JsonType.Array;
                this.inst_array = new List<JsonData>();
                result = (IList)this.inst_array;
            }
            return result;
        }

        private JsonData ToJsonData(object obj)
        {
            JsonData result;
            if (obj == null)
            {
                result = null;
            }
            else if (obj is JsonData)
            {
                result = (JsonData)obj;
            }
            else
            {
                result = new JsonData(obj);
            }
            return result;
        }

        private static void WriteJson(IJsonWrapper obj, JsonWriter writer)
        {
            if (obj.IsString)
            {
                writer.Write(obj.GetString());
            }
            else if (obj.IsBoolean)
            {
                writer.Write(obj.GetBoolean());
            }
            else if (obj.IsDouble)
            {
                writer.Write(obj.GetDouble());
            }
            else if (obj.IsInt)
            {
                writer.Write(obj.GetInt());
            }
            else if (obj.IsUint)
            {
                writer.Write(obj.GetUint());
            }
            else if (obj.IsLong)
            {
                writer.Write(obj.GetLong());
            }
            else if (obj.IsArray)
            {
                writer.WriteArrayStart();
                foreach (object current in (IList)obj)
                {
                    JsonData.WriteJson((JsonData)current, writer);
                }
                writer.WriteArrayEnd();
            }
            else if (obj.IsObject)
            {
                writer.WriteObjectStart();
                foreach (DictionaryEntry dictionaryEntry in (IDictionary)obj)
                {
                    writer.WritePropertyName((string)dictionaryEntry.Key);
                    JsonData.WriteJson((JsonData)dictionaryEntry.Value, writer);
                }
                writer.WriteObjectEnd();
            }
            else if (obj.GetJsonType() == JsonType.None)
            {
                //为空的情况下，也要能写一对花括号，避免格式出错。   
                writer.WriteObjectStart();
                writer.WriteObjectEnd();
            }
        }

        public int Add(object value)
        {
            JsonData value2 = this.ToJsonData(value);
            value2.OnAddToParent(this);
            this.json = null;
            return this.EnsureList().Add(value2);
        }

        public void Add(string strKey, object value)
        {
            if (this.ContainsKey(strKey))
            {
                string strMsg = string.Format("The given key {0} exists!", strKey);
                JsonException.Throw(new ArgumentException(strMsg));
            }

            JsonData objData = this.ToJsonData(value);
            this[strKey] = objData;
        }

        public void Clear()
        {
            if (this.IsObject)
            {
                ((IDictionary)this).Clear();
            }
            else if (this.IsArray)
            {
                ((IList)this).Clear();
            }
        }

        public bool EqualsTo<T>(T x)
        {
            JsonData objOtherData = this.ToJsonData(x);
            return this.Equals(objOtherData);
        }

        public bool Equals(JsonData x)
        {
            if (this == x)
            {
                //二者是同一个引用，指向同一片内存区域，那么肯定相等，无需后续逻辑
                return true;
            }

            bool result;
            if (x == null)
            {
                result = false;
            }
            else if (x.type != this.type)
            {
                result = false;
            }
            else
            {
                switch (this.type)
                {
                    case JsonType.None:
                        result = false;
                        break;
                    case JsonType.Object:
                        result = this.inst_object.Equals(x.inst_object);
                        break;
                    case JsonType.Array:
                        result = this.inst_array.Equals(x.inst_array);
                        break;
                    case JsonType.String:
                        result = this.inst_string.Equals(x.inst_string);
                        break;
                    case JsonType.Int:
                        result = this.inst_int.Equals(x.inst_int);
                        break;
                    case JsonType.Uint:
                        result = this.inst_uint.Equals(x.inst_uint);
                        break;
                    case JsonType.Long:
                        result = this.inst_long.Equals(x.inst_long);
                        break;
                    case JsonType.Double:
                        result = this.inst_double.Equals(x.inst_double);
                        break;
                    case JsonType.Boolean:
                        result = this.inst_boolean.Equals(x.inst_boolean);
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            return result;
        }

        public JsonType GetJsonType()
        {
            return this.type;
        }

        public void SetJsonType(JsonType type)
        {
            if (this.type != type)
            {
                switch (type)
                {
                    case JsonType.Object:
                        this.inst_object = new Dictionary<string, JsonData>();
                        this.object_list = new List<KeyValuePair<string, JsonData>>();
                        break;
                    case JsonType.Array:
                        this.inst_array = new List<JsonData>();
                        break;
                    case JsonType.String:
                        this.inst_string = null;
                        break;
                    case JsonType.Int:
                        this.inst_int = 0;
                        break;
                    case JsonType.Uint:
                        this.inst_uint = 0;
                        break;
                    case JsonType.Long:
                        this.inst_long = 0L;
                        break;
                    case JsonType.Double:
                        this.inst_double = 0.0;
                        break;
                    case JsonType.Boolean:
                        this.inst_boolean = false;
                        break;
                }
                this.type = type;
            }
        }

        public string ToJson()
        {
            string result;
            if (this.json != null)
            {
                result = this.json;
            }
            else
            {
                StringWriter stringWriter = new StringWriter();
                JsonWriter objJsonWriter = new JsonWriter(stringWriter);

#if DEBUG
                objJsonWriter.PrettyPrint = true;
                objJsonWriter.Validate = true;
#endif

                JsonData.WriteJson(this, objJsonWriter);
                this.json = stringWriter.ToString();
                result = this.json;
            }
            return result;
        }

        public void ToJson(JsonWriter writer)
        {
            bool validate = writer.Validate;
            writer.Validate = false;
            JsonData.WriteJson(this, writer);
            writer.Validate = validate;
        }

        public override string ToString()
        {
            string result;
            switch (this.type)
            {
                case JsonType.Object:
                    result = "JsonData object";
                    break;
                case JsonType.Array:
                    result = "JsonData array";
                    break;
                case JsonType.String:
                    result = this.inst_string;
                    break;
                case JsonType.Int:
                    result = this.inst_int.ToString();
                    break;
                case JsonType.Uint:
                    result = this.inst_uint.ToString();
                    break;
                case JsonType.Long:
                    result = this.inst_long.ToString();
                    break;
                case JsonType.Double:
                    result = this.inst_double.ToString();
                    break;
                case JsonType.Boolean:
                    result = this.inst_boolean.ToString();
                    break;
                default:
                    result = "Uninitialized JsonData";
                    break;
            }
            return result;
        }

        public static JsonData CreateEmptyObj()
        {
            return new JsonData();
        }

        public string FilePath
        {
            get { return m_strFilePath; }
            set { m_strFilePath = value; }
        }

        public static JsonData ReadFromFile(string strPath)
        {
            JsonData objEmpty = CreateEmptyObj();
            objEmpty.m_strFilePath = strPath;

            try
            {
#if UNITY_EDITOR  ||  UNITY_STANDALONE_WIN
                //判断给定的文件是否已存在
                if (!File.Exists(strPath))
                {
                    Debug.AssertFormat(false, "Can't find file {0}", strPath);
                    return objEmpty;
                }

                StreamReader objStreamReader = new StreamReader(strPath);
                string strContent = objStreamReader.ReadToEnd();
                objStreamReader.Close();
#elif UNITY_ANDROID
                string path = strPath;
                //string path = "file:///" + strPath;
                WWW www = new WWW(path);
                float fTimeOut = 5.0f;
                float fBeginTime = Time.time;
                while (!www.isDone)
                {
                    if (Time.time - fBeginTime > fTimeOut)
                    {
                        Debug.LogError("ReadFromFile TimeOut! File:" + path);
                        return JsonData.CreateEmptyObj();
                    }
                    continue;
                }
                string strContent = www.text;
#endif

                if (string.IsNullOrEmpty(strContent))
                {
                    Debug.AssertFormat(false, "file content is empty {0}", strPath);
                    return objEmpty;
                }

                JsonData objJson = JsonMapper.ToObject(strContent);
                if (null == objJson)
                {
                    return objEmpty;
                }
                objJson.m_strFilePath = strPath;
                return objJson;
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("ReadFromFile {0} failed!, Exception:{1}", strPath, ex.ToString());
                return objEmpty;
            }
        }

        public static bool IsNullOrEmpty(JsonData objData)
        {
            if (null == objData)
            {
                return true;
            }

            return objData.IsEmpty();
        }

        public bool IsEmpty()
        {
            switch (this.GetJsonType())
            {
                case JsonType.None:
                    return true;
                case JsonType.Array:
                case JsonType.Object:
                    return this.Count <= 0;
                default:
                    break;
            }

            return false;
        }

        public bool ContainsKey(object key)
        {
            if (!IsObject)
            {
                return false;
            }

            return this.EnsureDictionary().Contains(key);
        }

        public ICollection GetKeys()
        {
            return this.EnsureDictionary().Keys;
        }

        public bool TryGetValue(string key,out JsonData value)
        {
            return this.inst_object.TryGetValue(key,out value);
        }

        private string json
        {
            get { return m_strJson; }

            set
            {
                if (value == null)
                {
                    //清空自身json字符串的同时，把祖先节点的一并清空
                    List<JsonData> listAncestor = new List<JsonData>();
                    this.GetAllAncestor(ref listAncestor);
                    foreach (JsonData pAncestor in listAncestor)
                    {
                        if (null != pAncestor && null != pAncestor.json)
                        {
                            pAncestor.json = null;
                        }
                    }
                }

                m_strJson = value;
            }
        }

        private void OnAddToParent(JsonData pParent)
        {
            if (null == pParent)
            {
                return;
            }

            if (m_listParent.Contains(pParent))
            {
                return;
            }

            m_listParent.Add(pParent);

            //校验非法情况：若节点自己是自己的祖先，那么parent的链中将会出现环。这样在写回文件时也会造成死循环
            List<JsonData> listAncestor = new List<JsonData>();
            this.GetAllAncestor(ref listAncestor);
            if (listAncestor.Contains(this))
            {
                string strMsg = string.Format("There are endless loop in parent link. data:{0}, path:{1}"
                    , this.ToString()
                    , this.FilePath);

                JsonException.Throw(new InvalidOperationException(strMsg));
            }

            pParent.json = null;
        }

        private void OnRemoveFromParent(JsonData pParent)
        {
            if (null == pParent)
            {
                return;
            }

            if (!m_listParent.Contains(pParent))
            {
                return;
            }

            pParent.json = null;
            m_listParent.Remove(pParent);
        }

        //获取所有的祖先节点
        private void GetAllAncestor(ref List<JsonData> listAncestor)
        {
            foreach (JsonData pParent in m_listParent)
            {
                if (null == pParent)
                {
                    continue;
                }

                if (listAncestor.Contains(pParent))
                {
                    //非法情况：若节点自己是自己的祖先，那么parent的链中将会出现环。此处跳过这种环
                    continue;
                }

                listAncestor.Add(pParent);
                pParent.GetAllAncestor(ref listAncestor);
            }
        }
    } //类结尾
} //命名空间结尾
