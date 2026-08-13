using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugins.models.orm;

namespace Plugins.models
{
    [Serializable]
    public readonly struct BodyStruct : IEquatable<BodyStruct>
    {
        public readonly string Name;
        public readonly int Value;

        public static BodyStruct Default => new(0, string.Empty);

        public int GenderNum => (Value < 1000000 ? Value / 1000 : Value / 10000) % 10;

        public int SystemNum => (Value < 1000000 ? Value / 10000 : Value / 100000) - 10;

        public BodyStruct(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public BodyStruct(int value)
        {
            Value = value;
            Name = string.Empty;
        }

        public BodyStruct(string title)
        {
            if (title is null)
            {

                Name = string.Empty;
                Value = 0;
                return;
            }

            string valueText;
            string nameText;
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

            Name = int.TryParse(valueText, out Value) ? nameText : title;
        }

        public static bool GetFromPrefab(string title, out BodyStruct body)
        {
            body = new(title);
            return body.Value >= 100000 && !string.IsNullOrWhiteSpace(body.Name);
        }

        public bool Equals(BodyStruct other)
        {
            return Value == other.Value && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is BodyStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Name);
        }

        // 支持 == 和 != 运算符直接比较
        public static bool operator ==(BodyStruct left, BodyStruct right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(BodyStruct left, BodyStruct right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? $"{Value}" : $"{Name}~{Value}";
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
                        Info tab => new BodyStruct(tab.Value, tab.Name),
                        BodyStruct obj => obj, _ => throw new NotSupportedException()
                    }
                );
            }
        }

        public static BodyStructWrapper Load(string path)
        {
            var jsonContent = File.ReadAllText(path);
            return JsonUtility.FromJson<BodyStructWrapper>(jsonContent);
        }

        public void Saved(string path)
        {
            var jsonContent = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, jsonContent);
        }

        public int[] ValuesAsInt()
        {
            return elements.Select(static e => e.Value).ToArray();
        }

        public string[] ValuesAsStr()
        {
            return elements.Select(static e => e.Value.ToString()).ToArray();
        }
    }
}
