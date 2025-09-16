using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using UnityEditor;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class ShowMark : MonoBehaviour
    {
        private ClickEvent clickEvent;

        private GameObject rootPrefabObj;

        private Dictionary<string, GameObject> AllObject;

        //public GameObject LastClickObject;
        /*    public GameObject parent;
        */
        //贴图类型 1 部位，2 表面，3 标志
        string markType = "1";

        //当前用到的材质贴图
        //Dictionary<string, Texture2D> allMaterials;
        //当前骨标对象的骨头材质
        public Dictionary<string, Material> boneMarkMaterialObjs;

        //退出骨标后需要现在的骨头记录
        private List<String> showBoneRecords;

        //当前的骨标骨头val
        private string[] currentBoneValue;

        //本次展示标志需要下载的所有贴图
        public Dictionary<string, string> downMarkImgs;

        public void SyncAllObj(Dictionary<string, GameObject> AllObject)
        {
            this.AllObject = AllObject;
        }

        //初始化 并隐藏骨骼以外的系统
        public void SyncModelData(GameObject rootPrefabObj, Dictionary<string, GameObject> AllObject)
        {
            this.rootPrefabObj = rootPrefabObj;
            this.AllObject = AllObject;

            if (showBoneRecords == null)
            {
                showBoneRecords = new List<string>();
            }

            int childCount = rootPrefabObj.transform.childCount;
            //隐藏除骨骼以外的其他系统，并记录显示的骨骼，用来还原
            for (int i = 0; i < childCount; i++)
            {
                Transform transform = rootPrefabObj.transform.GetChild(i);
                string modelName = transform.name;
                //Debug.Log($"model name {modelName}");
                if (modelName.StartsWith("骨骼系统") || modelName.StartsWith("肌肉系统"))
                {
                    GetShowObjRecords(transform);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }

        private void GetShowObjRecords(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                if (childTransform.childCount > 0)
                {
                    GetShowObjRecords(childTransform);
                }
                else
                {
                    if (childTransform.gameObject.activeSelf)
                    {
                        showBoneRecords.Add(GetModelValue(childTransform.name));
                        childTransform.gameObject.SetActive(false);
                    }
                }
            }
        }

        //单独显示骨头骨标
        public void LoadShowModelBoneMark(string modelValue)
        {
            currentBoneValue = modelValue.Split(';');
            boneMarkMaterialObjs ??= new Dictionary<string, Material>();
            //Debug.Log($"AllObject size {AllObject.Count}");
            //Debug.Log($"currentBoneValue size {currentBoneValue.Count<string>()}");

            foreach (string val in currentBoneValue)
            {
                //Debug.Log("show 1111");
                if (AllObject.ContainsKey(val))
                {
                    //Debug.Log("show 2222");
                    if (!boneMarkMaterialObjs.ContainsKey(val))
                    {
                        //Debug.Log($"show value {val}");
                        GameObject gObj = AllObject[val];

                        gObj.SetActive(true);
                        //是骨骼
                        if (val.StartsWith("10"))
                        {
                            Renderer renderer = gObj.transform.GetComponent<Renderer>();
                            Material material = renderer.material;
                            boneMarkMaterialObjs.Add(val, material);

                            downMarkImgs ??= new Dictionary<string, string>();
                            for (int i = 1; i < 5; i++)
                            {
                                string materialName = $"{GetMaterialName(material.name)}_mark{i}";
                                string zzaoUrl =
                                    $"https://wazitan.obs.cn-east-3.myhuaweicloud.com/wankang/images/bone_marks/{materialName}.png";
                                string zzaoKey = $"{materialName}_zzao{i}";
                                if (!downMarkImgs.ContainsKey(zzaoKey))
                                {
                                    downMarkImgs.Add(zzaoKey, zzaoUrl);
                                }

                                //if (i != 4)
                                //{
                                string albeUrl =
                                    $"https://wazitan.obs.cn-east-3.myhuaweicloud.com/wankang/images/bone_marks/{materialName}_cover.png";
                                string albeKey = $"{materialName}_albe{i}";
                                if (!downMarkImgs.ContainsKey(albeKey))
                                {
                                    downMarkImgs.Add(albeKey, albeUrl);
                                }
                                //}
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"没有对象 -{val}-");
                }
            }
        }

        public void ClearBoneMarkData()
        {
            boneMarkMaterialObjs.Clear();
        }


        //切换骨性类型
        public void ChangeBoneMarkWithType(string markType)
        {
            this.markType = markType;
            if (boneMarkMaterialObjs.Count > 0)
            {
                SetTypeTexture();
                //StartCoroutine(DownloadTexture());
            }
            else
            {
                Debug.Log("切换骨标类型失败，没有找到骨标对象");
            }
        }

        public void ChangeBoneMark(string data)
        {
            //Debug.Log("ChangeBoneMark 1111");
            MarkModel boneMarkModel = SerializerObj(data);

            GameObject targetObj = AllObject[boneMarkModel.Value];
            Material material = targetObj.transform.GetComponent<Renderer>().material;
            foreach (KeyValuePair<string, Material> item in boneMarkMaterialObjs)
            {
                if (item.Value.Equals(material))
                {
                    item.Value.SetInt("_bskg", 0);
                }
                else
                {
                    item.Value.SetInt("_bskg", 1);
                }
            }

            material.SetFloat("_uvx", Convert.ToSingle(boneMarkModel.UVX));
            material.SetFloat("_uvy", Convert.ToSingle(boneMarkModel.UVY));
            //Debug.Log($"uvx = {boneMarkModel.UVX}, uvy = {boneMarkModel.UVY}");

            TransVisualAngle(boneMarkModel);
        }

        public void CancelSelectMark()
        {
            if (boneMarkMaterialObjs != null)
            {
                foreach (KeyValuePair<string, Material> boneItem in boneMarkMaterialObjs)
                {
                    boneItem.Value.SetInt("_bskg", 1);
                }
            }
        }

        //更新骨标视角
        public void UpdateMarkCameraVisualAngle(string data)
        {
            TransVisualAngle(SerializerObj(data));
        }

        //退出
        public void ExitBoneMark()
        {
            ClearBoneMark();
            //Debug.Log("ExitBoneMark 1111");
            int childCount = rootPrefabObj.transform.childCount;
            //还原
            for (int i = 0; i < childCount; i++)
            {
                GameObject systemObj = rootPrefabObj.transform.GetChild(i).gameObject;
                string modelName = systemObj.transform.name;
                //Debug.Log(modelName);
                if (modelName.StartsWith("骨骼系统"))
                {
                    if (showBoneRecords != null)
                    {
                        if (currentBoneValue != null)
                        {
                            foreach (string boneValue in currentBoneValue)
                            {
                                if (showBoneRecords.Contains(boneValue))
                                {
                                    showBoneRecords.Remove(boneValue);
                                }
                                else
                                {
                                    if (AllObject.ContainsKey(boneValue))
                                    {
                                        AllObject[boneValue].SetActive(false);
                                    }
                                    else
                                    {
                                        Debug.Log($"AllObject 不包含 current show key {boneValue}");
                                    }
                                }
                            }

                            currentBoneValue = null;
                        }

                        foreach (string boneModelValue in showBoneRecords)
                        {
                            if (AllObject.ContainsKey(boneModelValue))
                            {
                                AllObject[boneModelValue].SetActive(true);
                            }
                            else
                            {
                                Debug.Log($"AllObject 不包含 records的 key {boneModelValue}");
                            }
                        }

                        showBoneRecords.Clear();
                    }
                }
                else
                {
                    systemObj.SetActive(true);
                }
            }
        }


        private string[] nullAlbeMaterials = { "Guge19", "Guge18" };

        //骨连接类型，Texture albe 不一样
        private bool CheckNullAlbeMaterial(string materialName)
        {
            return nullAlbeMaterials.Contains(materialName);
        }

        //清除骨标贴图，需要把原始的骨头贴图放到resource下，打包
        public void ClearBoneMark()
        {
            //Debug.Log("ClearBoneMark 1111");
            //if (allMaterials == null) {
            //    return;
            //}
            //if (allMaterials.Count > 0)
            //{
            Shader shaderAmeop = Shader.Find("ameop");
            //Debug.Log($"ameop shader is {shaderAmeop == null}");
            foreach (KeyValuePair<string, Material> items in boneMarkMaterialObjs)
            {
                string materialName = GetMaterialName(items.Value.name);
                items.Value.shader = shaderAmeop;
                if (CheckNullAlbeMaterial(materialName))
                {
                    items.Value.SetTexture("_albe", null);
                }
                else
                {
                    Texture2D textureAlbe = GetClickEvent().ab1.LoadAsset<Texture2D>(materialName);
                    //Texture2D textureAlbe = Resources.Load<Texture2D>($"Texture/{materialName}");
                    //Debug.Log($"{materialName} texture2d is {textureAlbe == null}");
                    //Debug.Log($"ClearBoneMark 2222 {textureAlbe == null}");
                    items.Value.SetTexture("_albe", textureAlbe);
                }
                //Destroy(items.Value);
            }

            boneMarkMaterialObjs.Clear();
            downMarkImgs.Clear();
            //allMaterials.Clear();

            if (gameObject.TryGetComponent<DownLoader>(out DownLoader downLoader))
            {
                Destroy(downLoader);
            }
            //}
        }

        public void StartDownloadImg(string types)
        {
            if (downMarkImgs != null && downMarkImgs.Count > 0)
            {
                if (ClickEvent.TEST_PLUGIN)
                {
                    EditorUtility.DisplayProgressBar("提示", "贴图下载中……", 0f);
                }

                List<string> downloadUrl = new List<string>();
                foreach (KeyValuePair<string, string> item in downMarkImgs)
                {
                    string type = item.Key.Substring(item.Key.Length - 1);
                    if (types.Contains(type))
                    {
                        downloadUrl.Add(item.Value);
                    }
                }

                if (downloadUrl.Count > 0)
                {
                    markType = types.Substring(0, 1);
                    DownLoader downLoader = gameObject.AddComponent<DownLoader>();
                    downLoader.StartDownload(downloadUrl, GetSaveCacheFolderPath(), DownloadResult);
                }
            }
        }

        private void DownloadResult()
        {
            if (ClickEvent.TEST_PLUGIN)
            {
                EditorUtility.DisplayProgressBar("提示", "贴图下载完成", 1f);
                EditorUtility.ClearProgressBar();

                ChangeBoneMarkWithType(markType);
            }
            else
            {
#if UNITY_IOS
                        iOSPlugin.OnDownLoadBoneMarkFinished("");
#endif
#if UNITY_WEBGL
                        JSPlugin.OnDownLoadBoneMarkFinished("");
#endif
#if UNITY_ANDROID
                        AndPlugin("OnDownLoadBoneMarkFinished","");
#endif
            }
        }

        private void SetTypeTexture()
        {
            Shader shaderAme3 = Shader.Find("ame3");
            foreach (KeyValuePair<string, Material> boneItem in boneMarkMaterialObjs)
            {
                Material material = boneItem.Value;
                material.shader = shaderAme3;
                //Debug.Log($"value = {boneItem.Key} ; material name {material.name}");
                string materialName = $"{GetMaterialName(material.name)}_mark{markType}";
                string imgZZaoLocal = GetLocalFilePath(downMarkImgs[$"{materialName}_zzao{markType}"]);

                Texture2D zzaoTexture = LoadTextureFromFile(imgZZaoLocal);

                if (zzaoTexture != null)
                {
                    material.SetTexture("_zzao", zzaoTexture);
                }

                //如果不是起止点
                //if(!markType.Equals("4"))
                //{
                string imgAlbeLocal = GetLocalFilePath(downMarkImgs[$"{materialName}_albe{markType}"]);
                Debug.Log(imgZZaoLocal);
                Texture2D albeTexture = LoadTextureFromFile(imgAlbeLocal);
                if (albeTexture != null)
                {
                    material.SetTexture("_albe", albeTexture);
                }
                //}

                material.SetInt("_bskg", 1);
                ColorUtility.TryParseHtmlString("#74FFCB", out Color nowColor);
                material.SetColor("_bs", nowColor);
            }
        }

        ////下载和附加骨性图
        //private IEnumerator DownloadTexture()
        //{
        //    allMaterials ??= new Dictionary<string, Texture2D>();
        //    //Shader shaderAme3 = GetClickEvent().ab1.LoadAsset<Shader>("ame3");
        //    Shader shaderAme3 = Shader.Find("ame3");
        //    //Debug.Log($"shaderAme3 is null = {shaderAme3 == null}");

        //    foreach (KeyValuePair<string, Material> boneItem in boneMarkMaterialObjs)
        //    {
        //        Material material = boneItem.Value;
        //        material.shader = shaderAme3;
        //        //Debug.Log($"value = {boneItem.Key} ; material name {material.name}");
        //        string materialName = $"{GetMaterialName(material.name)}_mark{markType}";

        //        if (allMaterials.ContainsKey(materialName))
        //        {
        //            material.SetTexture("_zzao", allMaterials[materialName]);
        //            //Debug.Log("material name 1" + material.name);
        //        }
        //        else
        //        {
        //            string url = $"https://wazitan.obs.cn-east-3.myhuaweicloud.com/wankang/images/bone_marks/{materialName}.png";
        //            //Debug.Log(url);

        //            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        //            {
        //                yield return webRequest.SendWebRequest();

        //                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        //                {
        //                    Debug.Log(webRequest.error);
        //                }
        //                else
        //                {
        //                    //Debug.Log("material name " + material.name);
        //                    // 获取下载的贴图
        //                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
        //                    material.SetTexture("_zzao", downloadedTexture);

        //                    //解决切换快导致线程延迟，会重复添加
        //                    if (!allMaterials.ContainsKey(materialName))
        //                    {
        //                        allMaterials.Add(materialName, downloadedTexture);
        //                    }

        //                    // 保存到本地可写入的目录
        //                    //File.WriteAllBytes(filePath, downloadedTexture.EncodeToPNG());
        //                    //UnityEngine.Object.Destroy(downloadedTexture);
        //                }
        //            }
        //        }

        //        if (allMaterials.ContainsKey($"{materialName}_cover")) {
        //            material.SetTexture("_albe", allMaterials[$"{materialName}_cover"]);
        //        } else {

        //            string urlCover = $"https://wazitan.obs.cn-east-3.myhuaweicloud.com/wankang/images/bone_marks/{materialName}_cover.png";
        //            //Debug.Log(urlCover);
        //            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(urlCover))
        //            {
        //                yield return webRequest.SendWebRequest();

        //                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        //                {
        //                    Debug.Log(webRequest.error);
        //                }
        //                else
        //                {
        //                    //Debug.Log("cover material name " + material.name);
        //                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
        //                    material.SetTexture("_albe", downloadedTexture);
        //                    if (!allMaterials.ContainsKey($"{materialName}_cover"))
        //                    {
        //                        allMaterials.Add($"{materialName}_cover", downloadedTexture);
        //                    }

        //                }
        //            }
        //        }
        //        //Debug.Log($"allMaterials size {allMaterials.Count}");
        //        if (allMaterials.Count > 0) {
        //            //Debug.Log("set materal shader");
        //            //material.shader = shaderAme3;
        //            material.SetInt("_bskg", 1);

        //            ColorUtility.TryParseHtmlString("#74FFCB", out Color nowColor);
        //            material.SetColor("_bs", nowColor);
        //        }

        //    }


        //    #if UNITY_IOS
        //                                            iOSPlugin.OnDownLoadBoneMarkFinished("");
        //    #endif
        //    #if UNITY_WEBGL
        //                                                   JSPlugin.OnDownLoadBoneMarkFinished("");
        //    #endif
        //    #if UNITY_ANDROID
        //                    AndPlugin("OnDownLoadBoneMarkFinished","");
        //    #endif
        //}

        private void AndPlugin(string methodName, params object[] args)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
            // AndroidJavaObject _ajc = new AndroidJavaObject("com.wazitan.anatomy.activity.UnityPlayerActivity");
            bool success = _ajc.Call<bool>(methodName, args);
        }

        private MarkModel SerializerObj(string data)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MarkModel));
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            return (MarkModel)serializer.ReadObject(mStream);
        }

        //转换视角
        private void TransVisualAngle(MarkModel boneMarkModel)
        {
            Vector3 startCameraPosition = new Vector3(Convert.ToSingle(boneMarkModel.CameraPositionX),
                Convert.ToSingle(boneMarkModel.CameraPositionY), Convert.ToSingle(boneMarkModel.CameraPositionZ));
            Vector3 startCameraLookPosition = GetClickEvent().GetIntersectWithLineAndPlane(startCameraPosition,
                new Vector3(Convert.ToSingle(boneMarkModel.CameraRotationX),
                    Convert.ToSingle(boneMarkModel.CameraRotationY), Convert.ToSingle(boneMarkModel.CameraRotationZ)),
                new Vector3(0, 0, 1), new Vector3(5, 5, 0));
            GetClickEvent().startAnimation(startCameraPosition, startCameraLookPosition);
        }

        private ClickEvent GetClickEvent()
        {
            if (clickEvent == null)
            {
                clickEvent = GameObject.Find("Camera").GetComponent<ClickEvent>();
            }

            return clickEvent;
        }


        // 加载Texture的辅助函数
        private Texture2D LoadTextureFromFile(string filePath)
        {
            Texture2D texture = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                texture = new Texture2D(1, 1); // 初始化为一个小尺寸，稍后Unity会重新调整它
                texture.LoadImage(fileData); // 这会改变texture的尺寸
            }

            return texture;
        }

        private string GetMaterialName(string materialName)
        {
            string removeKey = "(Instance)";
            if (materialName.Contains(removeKey))
            {
                return materialName.Replace(removeKey, "").Trim();
            }
            else
            {
                return materialName.Trim();
            }
        }

        private string GetModelValue(string modelName)
        {
            if (modelName.Contains("~"))
            {
                return modelName.Substring(modelName.LastIndexOf("~") + 1).Trim();
            }
            else
            {
                return modelName.Trim();
            }
        }

        public string GetLocalFilePath(string url)
        {
            string fileName = Path.GetFileName(new Uri(url).LocalPath);
            return Path.Combine(GetSaveCacheFolderPath(), fileName);
        }

        private string GetSaveCacheFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, "ImageCache");
        }
    }
}
