using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Plugins.C_.models;

namespace Plugins.C_
{
    [Serializable]
    public class BoneMaps
    {
        public Texture2D essence;
        public Dictionary<int, Texture2D> Invisible = new();
        public Dictionary<int, Texture2D> Displayed = new();
    }

    public class BoneMarkManager : MonoBehaviour
    {
        private int _markType;
        private Material[] _materialChanged;
        private Dictionary<string, BoneMaps> _textures;

        private AssetBundle _foramensAsset;
        private GameObject _foramens;
        private ClickEvent _clickEvent;

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
            for (var i = 0; i < transform.childCount; i++)
            {
                var userInterface = transform.GetChild(i);
                if (userInterface.name == "ChangeBonesBtn")
                {
                    userInterface.GetComponent<Button>().onClick.AddListener(SettingBonemarkMode);
                    break;
                }
            }

            _clickEvent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickEvent>();
        }

        private void SettingBonemarkMode()
        {
            if (_markType < 3)
            {
                _markType++;

                if (_materialChanged == null || _materialChanged.Length == 0) GetMaterialChanged();
                ChangeMapsForBone();

                if (_markType == 1 && _foramens == null) LoadForamens();
            }
            else
            {
                _markType = 0;
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
                material.SetTexture(ShaderIDTexInvisible, _textures[markName].Invisible[_markType]);
                material.SetTexture(ShaderIDTexDisplayed, _textures[markName].Displayed[_markType]);
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

        private static string GetMarkName(UnityEngine.Object material)
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
                _textures[markName].Invisible[i] = texI;

                var texD = new Texture2D(1, 1);
                texD.LoadImage(File.ReadAllBytes(mapDisplayedPath));
                _textures[markName].Displayed[i] = texD;
            }
        }

        private void LoadForamens()
        {
            var atlas = gameObject.GetComponent<AtlasWorkflows>().atlas;
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
    }
}
