using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [Serializable]
    public class TypeItem
    {
        public int idNum;
        public string tittle;
    }

    [Serializable]
    public class OptionalConfig
    {
        private const string ConfigPath = "Assets/Editor/OptionalConfig.json";
        public TypeItem[] boneMarkTypes;
        public TypeItem[] atlasTypes;

        public static bool ReadConfig(out OptionalConfig config)
        {
            var configFile = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfigPath);
            if (configFile is null)
            {
                config = null;
                return false;
            }

            config = JsonUtility.FromJson<OptionalConfig>(configFile.text);
            return true;
        }

        public static string[] GetContext(IEnumerable<TypeItem> typeItems)
        {
            var context = from typeItem in typeItems select typeItem.tittle;
            return context.ToArray();
        }

        public static List<int> IdNumsSelected(TypeItem[] typeItems, bool[] flags)
        {
            var selectedItems = new List<int>();

            for (var i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    selectedItems.Add(typeItems[i].idNum);
                }
            }

            return selectedItems;
        }

        public static bool[] FlagsSelected(TypeItem[] typeItems, List<int> selectedItems)
        {
            var itemSum = typeItems.Length;
            var flags = new bool[itemSum];
            for (var i = 0; i < itemSum; i++)
            {
                if (!selectedItems.Contains(typeItems[i].idNum)) continue;

                flags[i] = true;
                break;
            }

            return flags;
        }
    }
}
