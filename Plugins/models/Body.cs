using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Plugins.models.orm;

namespace Plugins.models
{
    [Serializable]
    public class BodyStruct : IEquatable<BodyStruct>
    {
        public readonly string name;
        public readonly int value;
        private readonly string _key;

        public BodyStruct(int value, string name)
        {
            this.value = value;
            this.name = name;
            _key = $"{name}~{value}";
        }

        public BodyStruct(int value)
        {
            this.value = value;
            name = "";
            _key = $"{name}~{value}";
        }

        public BodyStruct(string title)
        {
            string valueText;
            string nameText;

            if (title is null) return;

            if (title.Contains("~") && title.Split('~') is { Length: 2 } temp)
            {
                valueText = temp[1];
                nameText = temp[0];
            }
            else
            {
                valueText = title.Trim();
                nameText = "";
            }

            name = int.TryParse(valueText, out value) ? nameText : title;

            _key = $"{name}~{value}";
        }

        public bool Equals(BodyStruct other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            return value == other.value && name == other.name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BodyStruct);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_key);
        }

        public static bool GetFromPrefab(string title, out BodyStruct body)
        {
            body = new BodyStruct(title);
            return body.value > 0 && body.name.Length > 0;
        }

        public int GenderNum()
        {
            return (value < 1000000 ? value / 1000 : value / 10000) % 10;
        }

        public int SystemNum()
        {
            return (value < 1000000 ? value / 10000 : value / 100000) - 10;
        }
    }

    [Serializable]
    public class BodyStructWrapper
    {
        public int gender;
        public List<BodyStruct> elements;

        public BodyStructWrapper() { }

        // 有参构造方法：自动把不同元素转换为BodyStruct对象
        public BodyStructWrapper(int gender, IEnumerable elements)
        {
            this.gender = gender;
            this.elements = new List<BodyStruct>();

            foreach (var e in elements)
            {
                this.elements.Add(
                    e switch
                    {
                        int num => new BodyStruct(num),
                        string str => new BodyStruct(str),
                        BodyStruct obj => obj,
                        Info tab => new BodyStruct(tab.Value, tab.Name),
                        _ => throw new NotSupportedException()
                    }
                );
            }
        }

        public static BodyStructWrapper Load(string path)
        {
            var jsonContent = path.StartsWith("Assets")
                ? AssetDatabase.LoadAssetAtPath<TextAsset>(path).text
                : File.ReadAllText(path);
            return JsonUtility.FromJson<BodyStructWrapper>(jsonContent);
        }

        public void Saved(string path)
        {
            var jsonContent = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, jsonContent);
        }

        public int[] ValuesAsInt()
        {
            return elements.Select(static e => e.value).ToArray();
        }

        public string[] ValuesAsStr()
        {
            return elements.Select(static e => e.value.ToString()).ToArray();
        }
    }
}
