using System;
using Editor.PrefabEditor;

namespace Editor
{
    public class NameConverter
    {
        private readonly string _prefix;

        public NameConverter(string prefix)
        {
            _prefix = prefix;
        }

        public RendererWrapper ConvertRenderer(RendererWrapper oldRenderer)
        {
            var newRenderer = new RendererWrapper
            {
                value = oldRenderer.value, materials = new()
            };

            foreach (var oldMaterial in oldRenderer.materials)
            {
                newRenderer.materials.Add(ConvertMaterial(oldMaterial));
            }

            return newRenderer;
        }

        private MaterialWrapper ConvertMaterial(MaterialWrapper oldMaterial)
        {
            return new MaterialWrapper
            {
                name = ConvertName(oldMaterial.name), albe = ConvertName(oldMaterial.albe)
                , normal = ConvertName(oldMaterial.normal)
            };
        }

        private string ConvertName(string oldName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
            {
                return "";
            }

            return oldName.StartsWith(_prefix, StringComparison.Ordinal)
                ? oldName
                : $"{_prefix}{oldName}";
        }
    }
}
