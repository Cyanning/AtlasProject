using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Plugins.models;
using Plugins.orm.Models;

// #if UNITY_EDITOR
using UnityEditor;
// #endif

namespace Plugins
{
    internal static class BoneAssets
    {
        public static string GetForamensAbPath(int gender)
        {
            var foramenname = gender == 1 ? "foramensfemale" : "foramensmale";
            return Path.Combine(Application.dataPath, $"StreamingAssets/Windows/encypt_{foramenname}");
        }

        public static Shader GetShader(string shaderName)
        {
// #if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"Assets/model/Shader/{shaderName}.shader");
// #endif
        }

        public static Texture2D GetGugeOriginTexture(string textureName)
        {
// #if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"Assets/model/Maps/Guge/{textureName}.jpg"
            );
// #endif
        }

        public static Texture2D GetBonemarkTexture(string textureName)
        {
// #if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"Assets/model/Maps/bonemark_maps/{textureName}.png"
            );
// #endif
        }
    }

    internal sealed class BoneRenderer
    {
        public readonly Renderer Renderer;
        public Texture2D Origin;
        public readonly Texture2D[] Identifier;
        public readonly Texture2D[] Surface;

        public BoneRenderer(Renderer renderer, int markKindsSum)
        {
            Renderer = renderer;
            Origin = null;
            Identifier = new Texture2D[markKindsSum];
            Surface = new Texture2D[markKindsSum];
        }
    }

    public class BoneMarkManager : MonoBehaviour
    {
        private ClickEvent _clickEvent;

        // 类型区分
        private const string OriginShaderName = "aseop";
        private const string BoneShaderName = "ame3";
        private const int MarkTypeRange = 4;
        public int MarkType { get; private set; }

        // 孔洞资源
        private GameObject _foramens;

        // 缓存模型贴图数据 方便来回切换
        private BoneRenderer[] _boneRenderers;
        private Shader _originShader;
        private Shader _markShader;

        // 着色器属性
        // private static readonly int ShaderIDUvx = Shader.PropertyToID("_uvx");
        // private static readonly int ShaderIDUvy = Shader.PropertyToID("_uvy");
        private static readonly int ShaderIDBgcolor = Shader.PropertyToID("bs");
        private static readonly int ShaderIDTranslucent = Shader.PropertyToID("_bskg");
        private static readonly int ShaderIDTexDisplayed = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTexIdentifier = Shader.PropertyToID("_zzao");

        private static readonly string[] ForamenNames = { "ForamensMale", "ForamensFemale" };

        private void Start()
        {
            _clickEvent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickEvent>();
            _originShader = BoneAssets.GetShader(OriginShaderName);
            _markShader = BoneAssets.GetShader(BoneShaderName);
        }

        /// <summary>
        /// 切换骨骼模型群时调用
        /// </summary>
        public void BoneFamilyChanged()
        {
            if (MarkType != 0)
            {
                MarkType = 0;
                SetBonemark();
            }
            _boneRenderers = null;
            _clickEvent.SetModelDisplayed();
        }

        /// <summary>
        /// 循环设置当前骨性标志类型
        /// </summary>
        public void SwitchBonemarkMode()
        {
            MarkType += MarkType < MarkTypeRange ? 1 : -MarkTypeRange;
        }

        public void SetBonemark()
        {
            if (_boneRenderers == null || _boneRenderers.Length == 0)
            {
                CacheMaterials();
            }

            ChangeTextures();

            // Todo 暂时不使用孔洞模型
            // if (MarkType == 1)
            // {
            //     LoadForamens();
            // }
            // else if (_foramens != null)
            // {
            //     Destroy(_foramens);
            // }
        }

        /// <summary>
        /// 缓存所有材质
        /// </summary>
        private void CacheMaterials()
        {
            // 获取当前所有骨骼的材质球
            var materials = new List<BoneRenderer>();
            foreach (var obj in _clickEvent.mObj.GetComponentsInChildren<Transform>())
            {
                if (obj.childCount > 0 || !BodyStruct.GetFromPrefab(obj.name, out var body) || body.SystemNum != 0)
                    continue;

                // 缓存材质文件
                var boneRenderer = obj.GetComponent<Renderer>();
                var bnMat = new BoneRenderer(boneRenderer, MarkTypeRange);
                var mat = bnMat.Renderer.material;

                // 缓存默认贴图
                var markName = mat.name.Replace(" (Instance)", "");
                if (mat.name == _originShader.name)
                {
                    bnMat.Origin = mat.GetTexture(ShaderIDTexDisplayed) as Texture2D;
                }
                else
                {
                    bnMat.Origin = BoneAssets.GetGugeOriginTexture(markName);
                }

                // 缓存标志贴图
                for (var i = 0; i < MarkTypeRange; i++)
                {
                    bnMat.Identifier[i] = BoneAssets.GetBonemarkTexture($"{markName}_mark{i + 1}");
                    bnMat.Surface[i] = BoneAssets.GetBonemarkTexture($"{markName}_mark{i + 1}_cover");
                }

                materials.Add(bnMat);
            }

            _boneRenderers = materials.ToArray();
        }

        /// <summary>
        /// 切换贴图
        /// </summary>
        private void ChangeTextures()
        {
            if (MarkType == 0)
            {
                foreach (var material in _boneRenderers)
                {
                    var mat = material.Renderer.material;
                    mat.shader = _originShader;
                    mat.SetTexture(ShaderIDTexDisplayed, material.Origin);
                }
            }
            else
            {
                var textureIndex = MarkType - 1;
                foreach (var material in _boneRenderers)
                {
                    var mat = material.Renderer.material;
                    mat.shader = _markShader;
                    mat.SetColor(ShaderIDBgcolor, Color.white);
                    mat.SetInt(ShaderIDTranslucent, 1);
                    mat.SetTexture(ShaderIDTexDisplayed, material.Surface[textureIndex]);
                    mat.SetTexture(ShaderIDTexIdentifier, material.Identifier[textureIndex]);
                }
            }
        }

        /// <summary>
        /// 加载孔洞模型
        /// </summary>
        private void LoadForamens()
        {
            var canvas = gameObject.GetComponent<ICanvasEditor>();

            using var fileStream = new MyStream(
                BoneAssets.GetForamensAbPath(canvas.ModelGender),
                FileMode.Open, FileAccess.Read, FileShare.None,
                1024 * 64, false
            );

            var foramensAb = AssetBundle.LoadFromStream(fileStream);
            _foramens = Instantiate(foramensAb.LoadAsset<GameObject>(ForamenNames[canvas.ModelGender]));
            _foramens.transform.position = _clickEvent.mObj.transform.position;
            _foramens.transform.rotation = _clickEvent.mObj.transform.rotation;
            _foramens.transform.localScale = _clickEvent.mObj.transform.localScale;
            foramensAb.Unload(false);

            foreach (var foramen in _foramens.GetComponentsInChildren<Transform>())
            {
                if (
                    foramen.childCount > 0
                    || !BodyStruct.GetFromPrefab(foramen.name, out var body)
                    || body.SystemNum != 12
                ) continue;

                var value = body.Value.ToString();
                if (canvas.ModelDisplayed.Contains(value))
                {
                    _clickEvent.AllObject.Add(value, foramen.gameObject);
                }
                else
                {
                    foramen.gameObject.SetActive(false);
                }

                _clickEvent.AddObjectClickEvent(foramen.gameObject);
            }
        }

        ///<summary>
        /// 查找当前点击位置的标志
        /// </summary>
        /// <param name="chickedModel">当前点击的对象</param>
        /// <param name="uv">鼠标位置的uv</param>
        /// <param name="bonemark">输出Bonemark实例</param>
        /// <returns>是否成功获取Bonemark</returns>
        public bool FindBonemarkData(GameObject chickedModel, Vector2 uv, out Bonemarks bonemark)
        {
            bonemark = null;

            // 不在骨性标志状态中不执行
            if (MarkType == 0)
            {
                return false;
            }

            // 检测预制体name是否合法
            if (!BodyStruct.GetFromPrefab(chickedModel.name, out var body))
                return false;

            // 若为孔洞模型
            if (body.SystemNum == 12)
            {
                bonemark = new Bonemarks
                {
                    Type = MarkType, Name = body.Name, PlaneValue = body.Value
                };
                return true;
            }

            // 若为骨骼模型
            if (TryGetColorCode(chickedModel, uv, out var colorCode))
            {
                bonemark = new Bonemarks
                {
                    Type = MarkType, Value = body.Value, Color = colorCode, Uvx = uv.x, Uvy = uv.y
                };
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取贴图上的色块的颜色值
        /// </summary>
        /// <param name="chickedModel">目标模型</param>
        /// <param name="uv">点位uv</param>
        /// <param name="colorCode">10进制整数RGB颜色值</param>
        /// <returns>成功获取？</returns>
        private static bool TryGetColorCode(GameObject chickedModel, Vector2 uv, out string colorCode)
        {
            colorCode = null;

            // 检测shader
            if (!chickedModel.TryGetComponent(out Renderer render))
                return false;

            var material = render.material;

            // 检测是否是骨性标志shader
            if (material == null || material.shader.name != BoneShaderName)
                return false;

            // 是否能拿到骨性标志贴图
            var tex = material.GetTexture(ShaderIDTexIdentifier) as Texture2D;
            if (tex == null)
                return false;

            var color = tex.GetPixel(
                Mathf.Clamp((int)(uv.x * tex.width), 0, tex.width - 1),
                Mathf.Clamp((int)(uv.y * tex.height), 0, tex.height - 1)
            );

            var r = Mathf.RoundToInt(color.r * 255);
            var g = Mathf.RoundToInt(color.g * 255);
            var b = Mathf.RoundToInt(color.b * 255);
            colorCode = $"{r},{g},{b}";

            // 白色不算标志点
            return !IsSameColor(colorCode);
        }

        /// <summary>
        /// 验证boneMark的uv与color数据是否匹配
        /// </summary>
        /// <param name="bonemark">必须是有颜色的实例</param>
        /// <returns>是否匹配</returns>
        public bool ColorCodeVerification(Bonemarks bonemark)
        {
            var models = _clickEvent.mObj.GetComponentsInChildren<Transform>();
            if (!bonemark.BePainting) return false;

            foreach (var model in models)
            {
                if (!BodyStruct.GetFromPrefab(model.name, out var body) || body.Value != bonemark.Value)
                    continue;

                if (TryGetColorCode(model.gameObject, new Vector2(bonemark.Uvx, bonemark.Uvy), out var color))
                {
                    return color == bonemark.Color;
                }
            }

            return false;
        }

        public void InitCameraTransform(BodyStruct body)
        {
            foreach (var objItem in _clickEvent.AllObject)
            {
                var value = body.Value.ToString();
                if (objItem.Key == value)
                {
                    var thisCollider = objItem.Value.gameObject.GetComponent<ModelInteraction>();
                    thisCollider.ModelMoving();
                    break;
                }
            }
        }

        /// <param name="targetColor">待验证的颜色</param>
        /// <param name="resultColor">被比较的颜色，不传则与白色比较</param>
        /// <returns></returns>
        public static bool IsSameColor(string targetColor, string resultColor = null)
        {
            var target = targetColor.Split(",").Select(int.Parse).ToArray();
            if (string.IsNullOrEmpty(resultColor))
            {
                foreach (var colorNum in target)
                {
                    if (colorNum < 250) return false;
                }
            }
            else
            {
                var result = targetColor.Split(",").Select(int.Parse).ToArray();
                for (var i = 0; i < 3; i++)
                {
                    if (Mathf.Abs(result[i] - target[i]) > 5)
                        return true;
                }
            }
            return false;
        }
    }
}
