using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Plugins.models;
using UnityEditor;


namespace Plugins
{
    public class BoneMarkManager : MonoBehaviour
    {
        public int markType;
        private const int TypeSum = 4;

        private ClickEvent _clickEvent;

        // 孔洞资源
        private AssetBundle _foramensAsset;
        private GameObject _foramens;

        // 缓存模型贴图数据 方便来回切换
        private Material[] _materialChanged;
        private Dictionary<string, BoneMaps> _textures;

        // 着色器属性
        // private static readonly int ShaderIDUvx = Shader.PropertyToID("_uvx");
        // private static readonly int ShaderIDUvy = Shader.PropertyToID("_uvy");
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
            if (markType < TypeSum)
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
                if (obj.childCount > 0 || !obj.name.Contains("~10")) continue;
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

                material.shader.name = "ame3";
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

                material.shader.name = "ameop";
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

            var mapBasic= AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/model/Maps/Guge/{markName}.jpg");
            if (mapBasic == null) return;
            _textures[markName].essence = mapBasic;

            for (var i = 1; i <= TypeSum; i++)
            {
                var mapInvisible = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    $"Assets/model/Maps/bone_mark_maps/{markName}_mark{i}.png"
                );
                var mapDisplayed = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    $"Assets/model/Maps/bone_mark_maps/{markName}_mark{i}_cover.png"
                );

                if (mapInvisible == null || mapDisplayed == null) continue;
                _textures[markName].invisible[i] = mapInvisible;
                _textures[markName].displayed[i] = mapDisplayed;
            }
        }

        private void LoadForamens()
        {
            var canvas = gameObject.GetComponent<ICanvasEditor>();
            var fileStream = new MyStream(
                Path.Combine(Application.dataPath, ForamenPathes[canvas.ModelGender]),
                FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 64, false
            );

            _foramensAsset = AssetBundle.LoadFromStream(fileStream);

            _foramens = Instantiate(_foramensAsset.LoadAsset<GameObject>(ForamenNames[canvas.ModelGender]));
            _foramens.transform.position = _clickEvent.mObj.transform.position;
            _foramens.transform.rotation = _clickEvent.mObj.transform.rotation;
            _foramens.transform.localScale = _clickEvent.mObj.transform.localScale;

            foreach (var foramen in _foramens.gameObject.GetComponentsInChildren<Transform>())
            {
                if (foramen.childCount > 0 || foramen.name[^8..^5] != "~22") continue;

                var modelValue = foramen.name[^7..];
                if (canvas.ModelDisplayed.Contains(modelValue))
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

        public bool FindBonemarkData(Transform chickedModel, Vector2 uv, out Bonemark bonemark)
        {
            bonemark = null;

            // 检测预制体是否合法
            if (!BodyStruct.GetFromPrefab(chickedModel.name, out var body))
                return false;

            // 判断是否是孔洞虚拟模型
            if (body.name.StartsWith("foramens_"))
            {
                bonemark = new Bonemark
                {
                    type = markType,
                    name = body.name,
                    planeValue = body.value
                };
                return true;
            }

            // 检测是否是骨性标志shader
            if (!chickedModel.TryGetComponent(out Renderer render))
                return false;
            var material = render.material;
            if (material == null || material.shader.name != "ame3")
                return false;

            // 是否能拿到骨性标志贴图
            var tex = material.GetTexture(ShaderIDTexInvisible) as Texture2D;
            if (tex == null)
                return false;

            // Todo 设置选中颜色

            var color = tex.GetPixel(
                Mathf.Clamp((int)(uv.x * tex.width), 0, tex.width - 1),
                Mathf.Clamp((int)(uv.y * tex.height), 0, tex.height - 1)
            );
            var r = Mathf.RoundToInt(color.r * 255);
            var g = Mathf.RoundToInt(color.g * 255);
            var b = Mathf.RoundToInt(color.b * 255);
            if (255 - r < 5 && 255 - g < 5 && 255 - b < 5)
                return false;

            bonemark = new Bonemark
            {
                type = markType,
                value = body.value,
                name = body.name,
                color = $"{r},{g},{b}",
                uvx = uv.x,
                uvy = uv.y
            };

            return true;
        }
    }
}
