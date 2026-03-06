using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
        private const string ConfigJsonPath = "Editor/OptionalConfig.json";
        public TypeItem[] boneMarkTypes;
        public TypeItem[] atlasTypes;

        public static string[] GetContext(IEnumerable<TypeItem> typeItems)
        {
            var context = from typeItem in typeItems select typeItem.tittle;
            return context.ToArray();
        }

        public static IEnumerable<int> GetMultiSelected(TypeItem[] typeItems, bool[] flags)
        {
            for (var i = 0; i < flags.Length; i++)
            {
                if (flags[i]) yield return typeItems[i].idNum;
            }
        }

        public static bool ReadConfig(out OptionalConfig config)
        {
            var configPath = Path.Combine(Application.dataPath, ConfigJsonPath);
            if (File.Exists(configPath))
            {
                config = JsonUtility.FromJson<OptionalConfig>(File.ReadAllText(configPath));
                return true;
            }

            config = null;
            return false;
        }
    }
}
