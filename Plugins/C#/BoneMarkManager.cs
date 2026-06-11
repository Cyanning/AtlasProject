using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Plugins.C_.models;


namespace Plugins.C_
{
    public class BoneMarkManager : MonoBehaviour
    {
        public int markType;

        private ClickEvent _clickEvent;

        // 孔洞资源
        private AssetBundle _foramensAsset;
        private GameObject _foramens;

        // 缓存模型贴图数据 方便来回切换
        private Material[] _materialChanged;
        private Dictionary<string, BoneMaps> _textures;

        // 着色器属性
        private static readonly int ShaderIDUvx = Shader.PropertyToID("_uvx");
        private static readonly int ShaderIDUvy = Shader.PropertyToID("_uvy");
        private static readonly int ShaderIDBgcolor = Shader.PropertyToID("bs");
        private static readonly int ShaderIDTranslucent = Shader.PropertyToID("_bskg");
        private static readonly int ShaderIDTexDisplayed = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTexInvisible = Shader.PropertyToID("_zzao");

        private static readonly string[] ForamenPathes =
        {
            "StreamingAssets/Test/encypt_foramensmale", "StreamingAssets/Test/encypt_foramensfemale"
        };

        private static readonly string[] ForamenNames =
        {
            "ForamensMale", "ForamensFemale"
        };

        private void Start()
        {
            _clickEvent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickEvent>();
        }

        public void SettingBonemarkMode()
        {
            if (markType < 3)
            {
                markType++;

                if (_materialChanged == null || _materialChanged.Length == 0) GetMaterialChanged();
                ChangeMapsForBone();

                if (markType == 1 && _foramens == null) LoadForamens();
            }
            else
            {
                markType = 0;
                RecoverMapsForBone();
            }
        }

        private void GetMaterialChanged()
        {
            var materials = new HashSet<Material>();
            foreach (var obj in _clickEvent.mObj.GetComponentsInChildren<Transform>())
            {
                if (obj.childCount > 0) continue;
                var material = obj.GetComponent<Renderer>().material;
                if (!material.name.StartsWith("Guge")) continue;
                materials.Add(material);
            }

            if (materials.Count == 0) return;
            _materialChanged = materials.ToArray();
            _textures = new Dictionary<string, BoneMaps>();
        }

        private void ChangeMapsForBone()
        {
            foreach (var material in _materialChanged)
            {
                var markName = GetMarkName(material);
                if (!_textures.ContainsKey(markName)) GetSeriesMapsforMaterial(markName);

                material.shader = Shader.Find("ame3");
                material.SetColor(ShaderIDBgcolor, Color.white);
                material.SetInt(ShaderIDTranslucent, 1);
                material.SetTexture(ShaderIDTexInvisible, _textures[markName].invisible[markType]);
                material.SetTexture(ShaderIDTexDisplayed, _textures[markName].displayed[markType]);
            }
        }

        private void RecoverMapsForBone()
        {
            foreach (var material in _materialChanged)
            {
                var markName = GetMarkName(material);
                if (!_textures.ContainsKey(markName)) GetSeriesMapsforMaterial(markName);

                material.shader = Shader.Find("ameop");
                material.SetTexture(ShaderIDTexDisplayed, _textures[markName].essence);
            }
        }

        private static string GetMarkName(Object material)
        {
            return material.name.EndsWith(" (Instance)") ? material.name[..^11] : material.name;
        }

        private void GetSeriesMapsforMaterial(string markName)
        {
            _textures.Add(markName, new BoneMaps());

            var mapPath = Path.Combine(Application.dataPath, $"model/Maps/Guge/{markName}.jpg");
            if (File.Exists(mapPath))
            {
                var texE = new Texture2D(1, 1);
                texE.LoadImage(File.ReadAllBytes(mapPath));
                _textures[markName].essence = texE;
            }

            for (var i = 1; i < 5; i++)
            {
                var mapInvisiblePath = Path.Combine(
                    Application.dataPath, $"model/Maps/bone_mark_maps/{markName}_mark{i}.png");
                var mapDisplayedPath = Path.Combine(
                    Application.dataPath, $"model/Maps/bone_mark_maps/{markName}_mark{i}_cover.png");

                if (!File.Exists(mapInvisiblePath) || !File.Exists(mapDisplayedPath)) continue;

                var texI = new Texture2D(1, 1);
                texI.LoadImage(File.ReadAllBytes(mapInvisiblePath));
                _textures[markName].invisible[i] = texI;

                var texD = new Texture2D(1, 1);
                texD.LoadImage(File.ReadAllBytes(mapDisplayedPath));
                _textures[markName].displayed[i] = texD;
            }
        }

        private void LoadForamens()
        {
            var atlas = gameObject.GetComponent<AtlasEditor>().atlas;
            var fileStream = new MyStream(
                Path.Combine(Application.dataPath, ForamenPathes[atlas.gender]),
                FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 64, false
            );

            _foramensAsset = AssetBundle.LoadFromStream(fileStream);

            _foramens = Instantiate(_foramensAsset.LoadAsset<GameObject>(ForamenNames[atlas.gender]));
            _foramens.transform.position = _clickEvent.mObj.transform.position;
            _foramens.transform.rotation = _clickEvent.mObj.transform.rotation;
            _foramens.transform.localScale = _clickEvent.mObj.transform.localScale;

            foreach (var foramen in _foramens.gameObject.GetComponentsInChildren<Transform>())
            {
                if (foramen.childCount > 0 || foramen.name[^8..^5] != "~22") continue;

                var modelValue = foramen.name[^7..];
                if (atlas.modelDisplayed.Contains(modelValue))
                {
                    _clickEvent.AllObject.Add(modelValue, foramen.gameObject);
                }
                else
                {
                    foramen.gameObject.SetActive(false);
                }

                _clickEvent.AddObjectClickEvent(foramen.gameObject);
            }

            fileStream.Close();
        }

        public Bonemark FindBonemarkData(Transform chickedModel, Vector2 uv)
        {
            // 检测是否是骨性标志shader
            if (!chickedModel.TryGetComponent(out Renderer render))
                return null;

            var material = render.material;
            if (material == null || material.shader.name != "ame3")
                return null;

            // 是否能拿到骨性标志贴图
            var tex = material.GetTexture(ShaderIDTexInvisible) as Texture2D;
            if (tex == null)
                return null;

            // Todo 设置选中颜色

            var x = Mathf.Clamp((int)(uv.x * tex.width), 0, tex.width - 1);
            var y = Mathf.Clamp((int)(uv.y * tex.height), 0, tex.height - 1);

            var color = tex.GetPixel(x, y);

            return new Bonemark
            {
                type = markType,
                color =
                    $"{Mathf.RoundToInt(color.r * 255)},{Mathf.RoundToInt(color.g * 255)},{Mathf.RoundToInt(color.b * 255)}",
                uvx = x,
                uvy = y
            };
        }
    }
}
