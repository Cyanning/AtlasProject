using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
