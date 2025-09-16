using UnityEngine;
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
        public Dictionary<int, Texture2D> Invisible = new ();
        public Dictionary<int, Texture2D> Displayed = new ();
    }

    public class BoneMarkManager : MonoBehaviour
    {
        private int _markType;
        private Material[] _materialChanged;
        private Dictionary<string, BoneMaps> _textures;
        private readonly int _shaderIDBgcolor = Shader.PropertyToID("bs");
        private readonly int _shaderIDTranslucent = Shader.PropertyToID("_bskg");
        private readonly int _shaderIDTexDisplayed = Shader.PropertyToID("_albe");
        private readonly int _shaderIDTexInvisible = Shader.PropertyToID("_zzao");

        private const string MaleForamensPath =  "StreamingAssets/Test/encypt_maleforamens";
        private const string FemaleForamensPath =  "StreamingAssets/Test/encypt_femaleforamens";

        public int SettingBonemarkMode()
        {
            if (_markType is >= 0 and < 4)
            {
                _markType++;
                if (_materialChanged == null || _materialChanged.Length == 0)
                {
                    GetMaterialChanged();
                }
                ChangeMapsForBone();
            }
            else
            {
                _markType = 0;
                RecoverMapsForBone();
            }

            return _markType;
        }

        private void GetMaterialChanged()
        {
            var materials = new HashSet<Material>();
            foreach (var obj in gameObject.GetComponent<ClickEvent>().mObj.GetComponentsInChildren<Transform>())
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
                material.SetColor(_shaderIDBgcolor, Color.white);
                material.SetInt(_shaderIDTranslucent, 1);
                material.SetTexture(_shaderIDTexInvisible, _textures[markName].Invisible[_markType]);
                material.SetTexture(_shaderIDTexDisplayed, _textures[markName].Displayed[_markType]);
            }
        }

        private void RecoverMapsForBone()
        {
            foreach (var material in _materialChanged)
            {
                var markName = GetMarkName(material);
                if (!_textures.ContainsKey(markName)) GetSeriesMapsforMaterial(markName);

                material.shader = Shader.Find("ameop");
                material.SetTexture(_shaderIDTexDisplayed, _textures[markName].essence);
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

        public void LoadForamens(string[] foramens,int gender)
        {
            var foramensPath = gender == 0 ? MaleForamensPath : FemaleForamensPath;
            var fileStream = new MyStream(foramensPath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 64, false);
            myLoadedAssetBundle = AssetBundle.LoadFromStream(fileStream);

            mObj = Instantiate(myLoadedAssetBundle.LoadAsset<GameObject>(modelPrefabName));

            fileStream.Close();
        }
    }
}
