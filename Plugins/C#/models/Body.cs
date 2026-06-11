using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Plugins.C_.models
{
    [Serializable]
    public class BodyStruct : IEquatable<BodyStruct>
    {
        public string name;
        public int value;

        private readonly string _key;

        public BodyStruct(string name, int value)
        {
            this.name = name;
            this.value = value;
            _key = $"{name}~{value}";
        }

        public BodyStruct(int value)
        {
            this.value = value;
            _key = value.ToString();
        }

        public BodyStruct(string value)
        {
            Value = value;
            _key = value;
        }

        public static bool ByPrefabName(string prefabName, out BodyStruct body)
        {
            var temp = prefabName.Split('~');
            if (temp.Length == 2 && temp[1].Length is 6 or 7)
            {
                body = new BodyStruct(temp[0], Convert.ToInt32(temp[1]));
                return true;
            }

            body = null;
            return false;
        }

        public string Value
        {
            get => value.ToString();
            set => this.value = Convert.ToInt32(value);
        }

        public int GenderNum()
        {
            return (value < 1000000 ? value / 1000 : value / 10000) % 10;
        }

        public int SystemNum()
        {
            return (value < 1000000 ? value / 10000 : value / 100000) - 10;
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
    }

    [Serializable]
    public class BodyStructWrapper
    {
        public int gender;
        public List<BodyStruct> elements;

        public BodyStructWrapper(int gender = -1, IEnumerable values = null)
        {
            this.gender = gender;

            elements = new List<BodyStruct>();
            if (values == null) return;

            foreach (var value in values)
            {
                elements.Add(
                    value switch
                    {
                        int vi => new BodyStruct(vi),
                        string vs => new BodyStruct(vs),
                        BodyStruct vb => vb,
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

        public void ItemsRefresh()
        {
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i] = new BodyStruct(elements[i].name, elements[i].value);
            }
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
