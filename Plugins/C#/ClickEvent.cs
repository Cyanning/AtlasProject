using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.Serialization.Json;
using UnityEngine.EventSystems;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Linq;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class ClickEvent : MonoBehaviour
    {
        public GameObject[] cube1; //3D物体集合
        public GameObject mObj;
        public GameObject mLittleObj;
        public GameObject mLivingObj;
        public GameObject mSectionObj;
        public GameObject mCamera;

        public string showModelStr = "";

        //private Vector3 StartScale = new Vector3(10, 10, 10);
        // public ArrayList AllObject = new ArrayList(); //所有对象集合
        public Dictionary<string, GameObject> AllObject = new Dictionary<string, GameObject>(); //所有对象集合

        public Dictionary<string, Texture> textureList = new Dictionary<string, Texture>(); //所有对象集合

        public Vector3 StartPos = new Vector3(0, 0, 0);
        public Vector3 StartRotation = new Vector3(0, 180, 0);
        public float scale;

        // Start is called before the first frame update

        public static float minX = 0;
        public static float minY = 0;
        public static float minZ = 0;

        public static float maxX = 0;
        public static float maxY = 0;
        public static float maxZ = 0;
        public static Vector3 RotationCenter;
        public int NowOperateIndex = -1;
        ShowModel showModel;

        public Vector3 startCameraPosition;
        public Vector3 startCameraLookPosition;
        public GameObject startCameraMoveObj;
        public string downLoadUrl; //模型下载地址
        public string assetsUrl;

        //public int cameraMoveType = 0;//0 不移动 1 向obj移动 2 向指定位置移动
        //public int tempMoveType = 0;

        public bool isPlayAnimation = false;
        public bool isShowLittle = false;
        public bool isShowLiving = false;
        public bool isClickCenter = false;

        public static bool isAutoDisable = false; //是否自动隐藏
        public static bool isManyChoose = false; //是否多选
        public static bool isTouchSplit = false; //是否拆分

        public static bool isMoveAnima = false;
        public bool isShowFeMale = false; //是否显示女性
        public bool isDownLoad = false;
        public bool isResetStatus = false;

        public Animator animator;
        private AnimationClip animationClip;

        public long startTime = 0;

        //public bool canTouch = false;
        public Vector3 LastCameraPosition;
        public UnityEngine.Quaternion LastCameraRotation;
        public string LastShowModel;
        public string BeforeHideOthersShowModel;
        public GameObject animObj;
        public GameObject littleObj;
        public int showType = 0; //1 肌肉运动2 微观3 活体 4 肌肉附着点 7骨标、起止点

        public GameObject tagCamera;
        public GameObject muscleMarkParent;
        public long beginAnimTime = 0;
        public bool repeatDownload = false; // 防止重复下载
        AssetBundle ab;

        public float screenWidth = 0;

        public float screenHeight = 0;

        //1public List<VectorLine> atlasLines = new List<VectorLine>();
        public GameObject scrollRight;
        public GameObject scrollLeft;
        public Camera getImageCamera;
        public GameObject getImageCameraObject;
        private Coroutine nowCoroutine;
        public string modelPath;

        //数据测试
        private static readonly bool TEST_MODEL = true;

        //逻辑测试
        public static readonly bool TEST_PLUGIN = true;

        //男女模型预制体名称
        private string modelPrefabName = "BodyMale";

        private List<string> TestDatas = null;
        public static bool type = true;
        private int TestDataPosition = 0;

        private void OnTest()
        {
            //运行后打开Canvas 的Graphic Raycaster 才能生效
            Debug.Log($"Test {TestDataPosition}");
            if (type)
            {
                type = false;

                //GetCenter();

                GoCameraView();
                //ShowBoneMark(TestDatas[TestDataPosition]);
            }
            else
            {
                type = true;

                MoveToTopHalfView();
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            //为相机添加3个组件
            if (gameObject.GetComponent<PhysicsRaycaster>() == null)
            {
                gameObject.AddComponent<PhysicsRaycaster>();
            }


            if (gameObject.GetComponent<StandaloneInputModule>() == null)
            {
                gameObject.AddComponent<StandaloneInputModule>();
            }


            SetAssetsPath(TestGetData.TEST_NEW_RESOURCE_PATH);
            SetModelPath(
                GameObject.Find("Canvas").GetComponent<AtlasWorkflows>().atlas.gender == 1
                    ? TestGetData.TEST_NEW_MODEL_FEMALE_PATH
                    : TestGetData.TEST_NEW_MODEL_MALE_PATH
            );

            //骨性标志
            //if (TEST_PLUGIN)
            //{
            //    //运行前先确定模式是否已经设置为可读写模式（inspector->Model->Read / Writer Enabled 设置为true）
            getShowType(
                "{\"sex\":0,\"camera_position_x\":0,\"camera_position_y\":0,\"camera_position_z\":-1,\"camera_rotation_x\":0,\"camera_rotation_y\":0,\"camera_rotation_z\":0.0,\"position_x\":0.0,\"position_y\":0.0,\"position_z\":0.0,\"rotation_x\":0.0,\"rotation_y\":0.0,\"rotation_z\":0.0,\"scale\":10.0,\"model_name\":\"\"}");
            //    TestDatas ??= new List<string>();
            //    //TestDatas.Add("1000010");
            //    //TestDatas.Add("1000229;1000257;1000417");
            //    //TestDatas.Add("1000032");
            //    //TestDatas.Add("1000237;1000424");
            //    //TestDatas.Add("1000449;1000505;1000512");
            //    TestDatas.Add("1000002;1000499;1000003;1000005;1000592;1000002");


            //ShowBoneMark(TestDatas[TestDataPosition],"1,2,3,4");

            //    //TestBehaviourScript test = gameObject.GetComponent<TestBehaviourScript>();
            //    //test.SetTestData(AllObject);
            //    //test.LoadBoneMark(TestDatas[TestDataPosition]);
            //}

            //肌肉起止
            //if (TEST_PLUGIN)
            //{

            //    GameObject camera = GameObject.Find("Camera");
            //    LastCameraPosition = camera.transform.position;
            //    LastCameraRotation = camera.transform.rotation;
            //    TestMuscleMark muscleMark = camera.GetComponent<TestMuscleMark>();

            //    string showModel = "1000002;1000499;1000003;1000005;1000592;1000002";
            //    getShowType("{\"sex\":0,\"camera_position_x\":0,\"camera_position_y\":0,\"camera_position_z\":-1,\"camera_rotation_x\":0,\"camera_rotation_y\":0,\"camera_rotation_z\":0.0,\"position_x\":0.0,\"position_y\":0.0,\"position_z\":0.0,\"rotation_x\":0.0,\"rotation_y\":0.0,\"rotation_z\":0.0,\"scale\":10.0,\"model_name\":\"\"}");
            //    ShowBoneMark(showModel, "4");
            //    muscleMark.SyncModel(AllObject);


            //    GoCameraView(1.2f);
            //    showType = 8;
            //}

            //string animateTest = "{\"model_name\":\"2-5_shouzhi_ququ\",\"device_type\":\"IOS\",\"camera_position_x\":244.3,\"camera_position_y\":36.6,\"camera_position_z\":191,\"camera_rotation_x\":3.705,\"camera_rotation_y\":-127.626,\"camera_rotation_z\":-0.274}";
            //StartDownLoadForShow(animateTest);
        }


        //通过传过来的模型地址，判断对应预制体的名字
        private void DefineModelName(string url)
        {
            modelPrefabName = url.EndsWith("encypt_malemodel") ? "BodyMaleStatic" : "BodyFemaleStatic";
        }

        private void SetModelPath(string url)
        {
            DefineModelName(url);

            if (!modelPath.Equals(url))
            {
                if (mObj != null)
                {
                    Destroy(mObj);
                }

                if (myLoadedAssetBundle != null)
                {
                    myLoadedAssetBundle.Unload(true);

                    myLoadedAssetBundle = null;
                }

                mObj = null;
                AllObject.Clear();
                Resources.UnloadUnusedAssets();
                modelPath = url;
            }
        }


        private void SetAssetsPath(string url)
        {
            assetsUrl = url;
        }


        public void ClearDataAssetsBundle(string s)
        {
            if (ab1 != null)
            {
                ab1.Unload(true);
                ab1 = null;
            }

            if (myLoadedAssetBundle != null)
            {
                myLoadedAssetBundle.Unload(true);
                myLoadedAssetBundle = null;
            }
        }


        public AssetBundle ab1;
        AssetBundle myLoadedAssetBundle;

        public void LoadFile()
        {
            //GameObject go;
            // Debug.Log("资源路径为："+ assetsUrl);

            if (ab1 == null && assetsUrl != null)
            {
                ab1 = AssetBundle.LoadFromFile(assetsUrl);
                // Debug.Log("ab1 里的资源: " + string.Join(", ", ab1.GetAllAssetNames()));
            }

            // Debug.Log("模型路径为:" + modelPath);
            var fileStream = new MyStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 64, false);
            myLoadedAssetBundle = AssetBundle.LoadFromStream(fileStream);

            mObj = Instantiate(myLoadedAssetBundle.LoadAsset<GameObject>(modelPrefabName));

            fileStream.Close();
            //Debug.Log("模型路径为end: mObj = " + mObj == null);

            //go = GameObject.Find(modelPrefabName);
            mObj.transform.localScale = new Vector3(0.0013f, 0.0013f, 0.0013f);
            mObj.transform.position = new Vector3(0, 0, 0);
            mObj.transform.rotation = Quaternion.Euler(new Vector3(-90, 180, 0));

            var t = Time.realtimeSinceStartup;
            //float m_LoadTime = 0f;

            var transforms = mObj.transform.GetComponentsInChildren<Transform>();


            var modelDisplay =
                GameObject.Find("Canvas").GetComponent<AtlasWorkflows>().atlas.modelDisplayed;
            foreach (var childTransform in transforms)
            {
                if (!childTransform.name.Contains("~") || childTransform.childCount > 0) continue;

                var modelValue = GetModelValue(childTransform.name);
                if (modelDisplay.Contains(modelValue))
                {
                    AllObject.Add(modelValue, childTransform.gameObject);
                }
                else
                {
                    childTransform.gameObject.SetActive(false);
                }

                AddObjectClickEvent(childTransform.gameObject);
            }
            //m_LoadTime = Time.realtimeSinceStartup - t;
            //Debug.Log("加载时间:" + m_LoadTime);
        }

        public void SetOrientionPortrait(string isPortrait)
        {
            if (isPortrait.Equals("true"))
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
            }
            else
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
            }
        }


        public void ShowAnimation(string path)
        {
            string[] sArray = path.Split(';');

            GameObject obj = (GameObject)Instantiate(Resources.Load(sArray[0]));

            obj.transform.position = new Vector3(0, 0, 0);
            obj.transform.localScale = new Vector3(Convert.ToSingle(sArray[1]), Convert.ToSingle(sArray[1]),
                Convert.ToSingle(sArray[1]));

            obj.AddComponent<FingerTouchForLittle>();
            FinishLoad();
            GetCenter();
        }


        public void StartDownLoadForShow(string path)
        {
            /*        if (!repeatDownload)
                    {*/

            nowCoroutine = StartCoroutine(ShowAnimationForDownLoad(path));
            // }
        }


        private void SendProgress(int progress)
        {
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.SendProgress(progress + "");
#endif
#if UNITY_WEBGL
                                   JSPlugin.SendProgress(progress+"");
#endif
#if UNITY_ANDROID
                    AndPlugin("sendProgress", progress.ToString());

#else

#endif
            }
        }

        private void SendAnimationProgress(int progress)
        {
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.SendAnimationProgress(progress + "");
#endif
#if UNITY_WEBGL
                                   JSPlugin.SendAnimationProgress(progress+"");
#endif
#if UNITY_ANDROID
                    AndPlugin("sendAnimationProgress", progress.ToString());

#else

#endif
            }
        }


        public IEnumerator ShowAnimationForDownLoad(string path)
        {
            repeatDownload = true;
            isDownLoad = true;
            showType = 1;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ShowModel));
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(path));

            showModel = (ShowModel)serializer.ReadObject(mStream);
            string url = "https://obs.xlhcq.com/wankang/muscle_animations_m/";

            if (showModel.DeviceType != null && showModel.DeviceType.Equals("IOS"))
            {
                url = url + showModel.ModelName + "_ios";
            }
            else if (showModel.DeviceType != null && showModel.DeviceType.Equals("Web"))
            {
                url = "https://d.nanxiani.cn/jiepou/muscleAnimation/Web/";
            }
            else
            {
                url = url + showModel.ModelName + "_android";
            }

            //url = url + showModel.ModelName;
            downLoadUrl = url;
            Debug.Log(url);
            WWW www = WWW.LoadFromCacheOrDownload(url, 2);

            while (!www.isDone)
            {
                int LoadPro = (((int)(www.progress * 100)) % 100);

                if (isDownLoad && downLoadUrl.Equals(url))
                {
                    SendProgress(LoadPro);
                }

                yield return 1;
            }

            yield return www;

            if (!String.IsNullOrEmpty(www.error))
            {
                if (!TEST_PLUGIN)
                {
#if UNITY_IOS
                                iOSPlugin.ReturnDownLoadError(www.error);
#endif
#if UNITY_WEBGL
                                               JSPlugin.downLoadError(www.error);
#endif
#if UNITY_ANDROID
                            AndPlugin("DownLoadError", www.error);
#else
                    Debug.Log(123);

#endif


                    yield break;
                }
            }

            if (isDownLoad && downLoadUrl.Equals(url))
            {
                if (ab != null)
                {
                    ab.Unload(false);
                }

                ab = www.assetBundle;

                var prefab = ab.LoadAsset<GameObject>(showModel.ModelName);
                GameObject obj = (GameObject)Instantiate(prefab);
                ab.Unload(false);
                if (animObj != null)
                {
                    animObj.SetActive(false);

                    Destroy(animObj);
                }

                animObj = obj;
                animObj.SetActive(true);
                SendProgress(100);

                FinishLoad();

                obj.transform.position = new Vector3(0, 0, 0);

                startCameraPosition = new Vector3(Convert.ToSingle(showModel.CameraPositionX),
                    Convert.ToSingle(showModel.CameraPositionY), Convert.ToSingle(showModel.CameraPositionZ));
                startCameraLookPosition = GetIntersectWithLineAndPlane(startCameraPosition,
                    new Vector3(showModel.CameraRotationX, showModel.CameraRotationY, showModel.CameraRotationZ),
                    new Vector3(0, 0, 1), new Vector3(5, 5, 0));
                animator = obj.GetComponent<Animator>();
                isPlayAnimation = true;

                if (animator != null)
                {
                    AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
                    if (animationClips != null && animationClips.Length > 0)
                    {
                        animationClip = animationClips[0];
                    }
                }

                startAnimation(startCameraPosition, startCameraLookPosition);
                RotationCenter = new Vector3(showModel.RotationCenterX, showModel.RotationCenterY,
                    showModel.RotationCenterZ);
                GetCenter();
                repeatDownload = false;
            }
        }


        public void CancelEnter(string s)
        {
            StopCoroutine(nowCoroutine);
        }

        public GameObject getObject(string name)
        {
            if (showType == 0)
            {
                if (AllObject.ContainsKey(name))
                {
                    return AllObject[name];
                }
                else
                {
                    return null;
                }
            }

            else
            {
                if (AllObject.ContainsKey(name))
                {
                    return AllObject[name];
                }
                else
                {
                    return null;
                }
            }
        }

        public void RemoveObjectClickEvent(GameObject itemObject)
        {
            /*       EventTrigger trigger = itemObject.GetComponent<EventTrigger>();
                   if (trigger != null)
                   {
                       Destroy(trigger);

                   }*/
        }

        public void AddObjectClickEvent(GameObject itemObject)
        {
            var box = itemObject.GetComponent<MeshCollider>();
            if (box == null)
            {
                box = itemObject.AddComponent<MeshCollider>();
            }

            //box.convex = true;
            // ThreeD_Object为3D物体挂载的脚本
            var item = itemObject.GetComponent<ThreeD_Object>();
            item ??= itemObject.AddComponent<ThreeD_Object>();

            var trigger = itemObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = itemObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
                var click = new UnityEngine.Events.UnityAction<BaseEventData>(item.OnClickCubeItem);
                entry.callback.AddListener(click);
                trigger.triggers.Add(entry);
                //Debug.Log("添加点击事件" + itemObject.name);
            }

            // Debug.Log("添加点击事件"+ entry);
        }


        // Update is called once per frame
        void Update()
        {
            if (isPlayAnimation)
            {
                if (animator != null)
                {
                    float playProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    SendAnimationProgress(Convert.ToInt16(playProgress * 100));
                }
            }
        }

        public void ChangeScene(string SceneName)
        {
            SceneManager.LoadScene(SceneName);
            Vector3 position = mObj.transform.position;
            Vector3 rotation = mObj.transform.eulerAngles;
            Vector3 scale = mObj.transform.localScale;
            GameObject camera = GameObject.Find("Camera");
            Vector3 cameraPos = camera.transform.position;
            Vector3 cameraRot = camera.transform.rotation.eulerAngles;
            modelName.Clear();
            GetModelName(mObj);
            //if (modelName.EndsWith(";"))
            //{
            //    modelName = modelName.Substring(0, modelName.Length - 1);
            //}


            string returnJson = "{\"" + "position_x\":" + position.x + ",\"" + "position_y\":" + position.y + ",\"" +
                                "position_z\":" + position.z + ",\"" + "rotation_x\":" + rotation.x + ",\"" +
                                "rotation_y\":" + rotation.y + ",\"" + "rotation_z\":" + rotation.z + ",\"" +
                                "camera_position_x\":" + cameraPos.x + ",\"" + "camera_position_y\":" + cameraPos.y +
                                ",\"" + "camera_position_z\":" + cameraPos.z + ",\"" + "camera_rotation_x\":" +
                                cameraRot.x + ",\"" + "camera_rotation_y\":" + cameraRot.y + ",\"" +
                                "camera_rotation_z\":" + cameraRot.z + ",\"" + "scale\":" + scale.x + ",\"" +
                                "model_name\":\"" + modelName + "\"}";
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.ReturnSceneData(returnJson);
#endif
#if UNITY_WEBGL
                       JSPlugin.ReturnSceneData(returnJson);
#endif
#if UNITY_ANDROID
                    AndPlugin("returnSceneData", returnJson);
#else
                Debug.Log(123);

#endif
            }
        }


        public void startAnimation(Vector3 endPosition, Vector3 lookPosition)
        {
            ClickEvent.isMoveAnima = true;
            GameObject camera = GameObject.Find("Camera");

            Tweener tweener = camera.transform.DOMove(endPosition, 1.2f);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate() { ClickEvent.isMoveAnima = false; };
            tweener.onKill = delegate() { ClickEvent.isMoveAnima = false; };
            tweener.onUpdate = delegate() { camera.transform.LookAt(lookPosition); };
        }

        public void getShowType(string str)
        {
            showModelStr = str;

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ShowModel));
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(showModelStr));

            showModel = (ShowModel)serializer.ReadObject(mStream);

            if (mObj == null)
            {
                LoadFile();
            }

            isResetStatus = true;

            showType = 0;
            mObj.SetActive(true);

            if (AllObject.Count == 0)
            {
                FindObjects(mObj, false, true);
            }
            // else
            // {
            //     if (TEST_MODEL)
            //     {
            //         var modelString = new StringBuilder();
            //         foreach (KeyValuePair<string, GameObject> items in AllObject)
            //         {
            //             GameObject obj = items.Value;
            //             if (obj.transform.childCount == 0)
            //             {
            //                 //obj.AddComponent<SplitModel>();
            //                 modelString.Append(items.Key).Append(";");
            //             }
            //         }
            //         showModel.ModelName = modelString.ToString();
            //     }
            //     else
            //     {
            //         HideBody("");
            //     }
            //
            // }


            string[] sArray = showModel.ModelName.Split(';');


            foreach (string i in sArray)
            {
                SetObjectStatus(getObject(i), true);
            }


            startCameraPosition = new Vector3(Convert.ToSingle(showModel.CameraPositionX),
                Convert.ToSingle(showModel.CameraPositionY), Convert.ToSingle(showModel.CameraPositionZ));

            startCameraLookPosition = GetIntersectWithLineAndPlane(startCameraPosition,
                new Vector3(showModel.CameraRotationX, showModel.CameraRotationY, showModel.CameraRotationZ),
                new Vector3(0, 0, 1), new Vector3(5, 5, 0));
            /*        cameraMoveType = 2;
                    tempMoveType = 2;*/
            startAnimation(startCameraPosition, startCameraLookPosition);
            if (showModel.AlphaName != null && !showModel.AlphaName.Equals(""))
            {
                string[] aArray = showModel.AlphaName.Split(';');
                foreach (string i in aArray)
                {
                    SetAlpha(i);
                }
            }

            if (showModel.Sex == 1)
            {
                isShowFeMale = true;
            }

            //hideGenderObj();
            FinishLoad();
            GetCenter();

            //AddToOperateListNew();
            NowOperateIndex = 0;
            GameObject camera = GameObject.Find("Camera");
            camera.transform.LookAt(mObj.transform);
            GetShowModel();

            //yield return 1;
        }


        public void AddAreaAndSystem(string modelName)
        {
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                GameObject obj = getObject(i);
                if (obj != null && !obj.active)
                {
                    obj.SetActive(true);
                }
                else
                {
                    if (obj == null)
                    {
                        Debug.Log(i + "不存在！");
                    }
                }
            }

            /*        if (addModelName.EndsWith(";"))
                    {

                        addModelName = addModelName.Substring(0, addModelName.Length - 1);
                    }*/
            //AddToOperateList(1, addModelName);
            FinishLoad();
            GetCenter();
            //AddToOperateListNew();
            GetShowModel();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.OnSceneLoaded("");
#endif
#if UNITY_WEBGL
                       JSPlugin.OnSceneLoaded("");
#endif
#if UNITY_ANDROID
                    AndPlugin("onSceneLoaded", "");

#else
                Debug.Log(123);
#endif
            }
        }

        public void RemoveAreaAndSystem(string modelName)
        {
            string[] sArray = modelName.Split(';');
            for (int i = 0; i < sArray.Length; i++)
            {
                GameObject obj = getObject(sArray[i]);
                if (obj != null && obj.active)
                {
                    obj.SetActive(false);
                }
                else
                {
                    if (obj == null)
                    {
                        Debug.Log(i + "不存在！" + sArray[i]);
                    }
                }
            }
            //foreach (string i in sArray)
            //{

            //    GameObject obj = getObject(i);
            //    if (obj != null && obj.active)
            //    {
            //        obj.SetActive(false);
            //    }
            //    else
            //    {
            //        if (obj == null)
            //        {
            //            Debug.Log(i + "不存在！");

            //        }
            //    }

            //}
            //AddToOperateList(2, modelName);
            FinishLoad();
            GetCenter();
            GetShowModel();
            ///AddToOperateListNew();
        }

        public void SetObjectStatus(GameObject obj, bool state)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.transform.childCount != 0)
            {
                return;
            }

            obj.SetActive(state);
            //return;
            if (!TEST_PLUGIN)
            {
                if (state)
                {
                    if (obj.transform.parent.name.Equals("000"))
                    {
                    }
                    else
                    {
                        RemoveObjectClickEvent(obj);
                        AddObjectClickEvent(obj);
                    }
                }
                else
                {
                    RemoveObjectClickEvent(obj);
                }
            }
        }

        public void ReSetModel(string modelName)
        {
            /*        FindObjects(mObj, false,false);
                    string[] sArray = modelName.Split(';');
                    foreach (string i in sArray)
                    {
                        FindObjects(getObject(i), true,false);

                    }*/


            if (showType == 0)
            {
                transform.position = new Vector3(0.03660000115633011F, -0.11699999868869782F, -1.159000039100647F);
                isResetStatus = true;
                transform.LookAt(GetIntersectWithLineAndPlane(transform.position,
                    new Vector3(354.2341003417969f, 358.21148681640627f, 0), new Vector3(0, 0, 1),
                    new Vector3(5, 5, 0)));
            }
            else
            {
                if (startCameraMoveObj == null)
                {
                    transform.position = startCameraPosition;
                    transform.LookAt(startCameraLookPosition);
                }
                else
                {
                    if (showType != 0)
                    {
                        transform.position = startCameraPosition;

                        transform.LookAt(startCameraLookPosition);
                    }
                    else
                    {
                        MeshRenderer renderer = startCameraMoveObj.GetComponent<MeshRenderer>();

                        transform.position = new Vector3(renderer.bounds.center.x, renderer.bounds.center.y,
                            renderer.bounds.center.z - 0.3F);
                        transform.LookAt(new Vector3(renderer.bounds.center.x, renderer.bounds.center.y,
                            renderer.bounds.center.z));
                    }
                }
            }
            // cameraMoveType = 0;
        }

        public void Show(string modelName)
        {
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                if (getObject(i) == null)
                {
                    continue;
                }

                SetObjectStatus(getObject(i), true);
            }

            //AddToOperateListNew();
            GetCenter();
            GetShowModel();
        }

        public void Hide(string modelName)
        {
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                SetObjectStatus(getObject(i), false);
            }
            //AddToOperateListNew();

            GetCenter();
            GetShowModel();
        }

        public void ShowNotAddOperate(string modelName)
        {
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                SetObjectStatus(getObject(i), true);
            }

            GetCenter();
        }

        public void HideNotAddOperate(string modelName)
        {
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                SetObjectStatus(getObject(i), false);
            }

            GetCenter();
        }

        public void HideOthers(string modelName)
        {
            SaveHideOthersShowModel("");
            string[] sArray = modelName.Split(';');
            //string hideModelName = "";
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;
                if (hideObj.active && hideObj.transform.childCount == 0)
                {
                    if (!sArray.Contains(items.Key))
                    {
                        SetObjectStatus(hideObj, false);
                    }
                }
            }
            /*        if (hideModelName.EndsWith(";"))
                    {
                        hideModelName = hideModelName.Substring(0, hideModelName.Length - 1);
                    }*/


            foreach (string s in sArray)
            {
                // FindObjects(getObject(s), true, false);
                SetObjectStatus(getObject(s), true);
            }
            //AddToOperateList(2, hideModelName);
            //AddToOperateListNew();

            GetCenter();
            GetShowModel();
        }

        public void CancelHideOthers(string modelName)
        {
            ShowBeforeHideOthersBody("");
        }

        private string lastSingleShow = "";
        private Vector3 lastSingleShowCameraPosition;
        private Vector3 lastSingleShowCameraRotation;
        private Vector3 lastSingleShowCenter;

        public void CancelSingleShow(string modelName)
        {
            ClickEvent.isMoveAnima = true;


            // float lenth = (mCamera.transform.position - lastSingleShowCameraPosition).magnitude;


            Tweener tweener = mCamera.transform.DOMove(lastSingleShowCameraPosition, 0.5f);
            /*       Tweener tweener2 = mCamera.transform.DORotate(lastSingleShowCameraRotation,0.5f);

                   tweener2.SetUpdate(true);
                   tweener2.SetEase(Ease.Linear);
                   tweener2.SetAutoKill(true);*/

            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate()
            {
                Tweener tweener1 = mCamera.transform.DORotate(lastSingleShowCameraRotation, 0.3f);
                tweener1.onComplete = delegate()
                {
                    isMoveAnima = false;

                    string[] sArray = lastSingleShow.Split(';');
                    foreach (string s in sArray)
                    {
                        SetObjectStatus(getObject(s), true);
                    }

                    GetShowModel();
                };
            };
            tweener.onUpdate = delegate()
            {
                mCamera.transform.LookAt(lastSingleShowCenter);
                /* mCamera.transform.rotation = Quaternion.Euler(mCamera.transform.position - lastSingleShowCenter);*/
            };
            tweener.onKill = delegate() { };
            //mCamera.transform.rotation = Quaternion.Euler(lastSingleShowCameraRotation);
            //startAnimation5(lastSingleShowCameraPosition, lastSingleShowCameraRotation);
        }

        public void startAnimation5(Vector3 endPoint, Vector3 endRotation)
        {
            ClickEvent.isMoveAnima = true;


            Tweener tweener = mCamera.transform.DOMove(endPoint, 0.5f);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate() { isMoveAnima = false; };
            tweener.onKill = delegate() { };

            mCamera.transform.rotation = Quaternion.Euler(endRotation);
        }

        public void SingleShow(string modelName)
        {
            lastSingleShow = GetLastShowModel();
            lastSingleShowCameraPosition = mCamera.transform.position;
            lastSingleShowCameraRotation = mCamera.transform.rotation.eulerAngles;

            //lastSingleShowCenter = GetIntersectWithLineAndPlane(lastSingleShowCameraPosition, lastSingleShowCameraRotation, new Vector3(0, 0, 1), new Vector3(5, 5, 0));

            //Debug.Log("交点x:" + lastSingleShowCenter.x + "y:" + lastSingleShowCenter.y + "z:" + lastSingleShowCenter.z);


            string[] sArray = modelName.Split(';');
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;
                if (hideObj.active && hideObj.transform.childCount == 0)
                {
                    if (!sArray.Contains(items.Key))
                    {
                        SetObjectStatus(hideObj, false);
                    }
                    //bool isContains = false;
                    //foreach (string s in sArray)
                    //{
                    //    if (s.Equals(items.Key))
                    //    {
                    //        isContains = true;
                    //    }
                    //}
                    //if (!isContains)
                    //{
                    //    SetObjectStatus(hideObj, false);
                    //}
                }
            }

            foreach (string s in sArray)
            {
                // FindObjects(getObject(s), true, false);
                SetObjectStatus(getObject(s), true);
            }

            //AddToOperateList(2, hideModelName);
            //AddToOperateListNew();

            //旋转移动动画 start
            //GetCenter();
            //lastSingleShowCenter = MoveToObject(modelName);
            //旋转移动动画 end

            GetShowModel();

            //AddToOperateListNew();
            //OperateStatusChange();
        }


        public void SetTouchSplit(string split)
        {
            isTouchSplit = split.Equals("true");
        }

        public void SetManyChoose(string manyChoose)
        {
            if (manyChoose.Equals("true"))
            {
                isManyChoose = true;
            }
            else
            {
                isManyChoose = false;
            }
        }

        public void HideModel(string modelName)
        {
            // FindObjects(getObject(modelName), false,false);
            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                SetObjectStatus(getObject(i), false);
            }
            //AddToOperateListNew();

            //AddToOperateList(2, modelName);
            GetCenter();
            GetShowModel();
        }

        public void ShowModel(string modelName)
        {
            // FindObjects(getObject(modelName), true, false);
            SetObjectStatus(getObject(modelName), true);
            //AddToOperateList(1, modelName);
            //AddToOperateListNew();

            GetCenter();
            GetShowModel();
        }

        public void HideAllModel(string modelName)
        {
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject obj = items.Value;

                if (obj.active && "component".Equals(obj.name))
                {
                    SetObjectStatus(obj, false);
                }
            }
        }


        public void SetAutoDisable(string autoDisable)
        {
            if (autoDisable.Equals("true"))
            {
                isAutoDisable = true;
            }
            else
            {
                isAutoDisable = false;
            }
        }

        public void CancelAlphaOthers(string modelName)
        {
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;

                if (hideObj.transform.childCount == 0)
                {
                    MeshRenderer meshRenderer = hideObj.GetComponent<MeshRenderer>();

                    SetAlpha(meshRenderer, 1F);
                }
            }
        }

        public void SetAlphaOthers(string modelName)
        {
            string[] sArray = modelName.Split(';');

            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;
                if (hideObj.active && hideObj.transform.childCount == 0)
                {
                    MeshRenderer renderer = hideObj.GetComponent<MeshRenderer>();

                    if (sArray.Contains(items.Key))
                    {
                        SetAlpha(renderer, 1);
                    }
                    else
                    {
                        SetAlpha(renderer, 0.5F);
                    }
                }
            }
            //AddToOperateListNew();
        }

        public GameObject LastAlphaGameObject;

        public void SetAlpha(string modelName)
        {
            if (modelName == null || modelName.Equals(""))
            {
                return;
            }

            string[] sArray = modelName.Split(';');

            foreach (string i in sArray)
            {
                GameObject obj = getObject(i);

                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                SetAlpha(renderer, 0.5F);
            }
        }


        public void SetAlpha(string modelName, float data)
        {
            string[] sArray = modelName.Split(';');

            foreach (string i in sArray)
            {
                GameObject obj = getObject(i);

                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                SetAlpha(renderer, data);
            }
        }

        public void SetAllNotAlpha(string data)
        {
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;

                if (hideObj.active)
                {
                    MeshRenderer meshRenderer = hideObj.GetComponent<MeshRenderer>();

                    SetAlpha(meshRenderer, 1F);
                }
            }
        }


        public void SetNotAlpha(string modelName)
        {
            string[] sArray = modelName.Split(';');

            foreach (string i in sArray)
            {
                GameObject obj = getObject(i);

                if (obj == null)
                {
                    continue;
                }

                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                SetAlpha(renderer, 1F);
            }
        }

        StringBuilder modelName = new StringBuilder();

        public void Collect(string data)
        {
            Vector3 position = mObj.transform.position;
            Vector3 rotation = mObj.transform.eulerAngles;
            Vector3 scale = mObj.transform.localScale;
            GameObject camera = GameObject.Find("Camera");
            Vector3 cameraPos = camera.transform.position;
            Vector3 cameraRot = camera.transform.rotation.eulerAngles;
            modelName.Clear();
            GetModelName(mObj);

            //if (modelName.ToString().EndsWith(";"))
            //{
            //    //modelName = modelName.Substring(0, modelName.Length - 1);
            //}
            string returnJson = "{\"" + "position_x\":" + position.x + ",\"" + "position_y\":" + position.y + ",\"" +
                                "position_z\":" + position.z + ",\"" + "rotation_x\":" + rotation.x + ",\"" +
                                "rotation_y\":" + rotation.y + ",\"" + "rotation_z\":" + rotation.z + ",\"" +
                                "camera_position_x\":" + cameraPos.x + ",\"" + "camera_position_y\":" + cameraPos.y +
                                ",\"" + "camera_position_z\":" + cameraPos.z + ",\"" + "camera_rotation_x\":" +
                                cameraRot.x + ",\"" + "camera_rotation_y\":" + cameraRot.y + ",\"" +
                                "camera_rotation_z\":" + cameraRot.z + ",\"" + "scale\":" + scale.x + ",\"" +
                                "model_name\":\"" + modelName + "\"}";
            // Debug.Log("加载了" + returnJson);
            string baseData = GetScreenShot();
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.ReturnCollectInfo(returnJson, baseData);
#endif
#if UNITY_WEBGL
                       JSPlugin.ReturnCollectInfo(returnJson,baseData);
#endif
#if UNITY_ANDROID
                    AndPlugin("returnCollectInfo", returnJson, baseData);

#else
                Debug.Log(123);

#endif
            }
        }


        public void GetCameraPhoto(string data)
        {
            if (!TEST_PLUGIN)
            {
                string baseData = GetScreenShot();
#if UNITY_IOS
                        iOSPlugin.ReturnCameraPhoto(baseData);
#endif
#if UNITY_WEBGL
                       JSPlugin.ReturnCameraPhoto(baseData);
#endif
#if UNITY_ANDROID
                    AndPlugin("returnCameraPhoto", baseData);

#else
                Debug.Log(123);

#endif
            }
        }


        void GetModelName(GameObject obj)
        {
            GetAllTransformName(obj);
            //Debug.Log(modelName.ToString());
            if (modelName != null && modelName.Length > 0)
            {
                modelName = modelName.Remove(0, 1);
            }
            //Debug.Log(modelName.ToString());
        }

        private void GetAllTransformName(GameObject obj)
        {
            int i = 0;

            while (i < obj.transform.childCount)
            {
                Transform parent = obj.transform.GetChild(i);
                if (parent.childCount > 0)
                {
                    GetAllTransformName(parent.gameObject);
                }
                else if (parent.childCount == 0 && parent.gameObject.active)
                {
                    /*          if (parent.transform.name.Contains("~@"))
                              {
                                  modelName += parent.transform.name.Substring(parent.transform.name.LastIndexOf("~@") + 1) + ";";
                              }
                              else {*/
                    modelName.Append(";").Append(GetModelValue(parent.transform.name));
                    //modelName += GetModelValue(parent.transform.name) + ";";
                    //   if（
                    // }
                }

                i++;
            }
        }

        public static void ShowToast(string text, AndroidJavaObject activity = null)
        {
            //  Debug.Log(text);
            if (activity == null)
            {
                AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject javaString = new AndroidJavaObject("java.lang.String", text);
                    Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString,
                        Toast.GetStatic<int>("LENGTH_SHORT")).Call("show");
                }
            ));
        }

        public static AndroidJavaObject ToJavaString(string CSharpString)
        {
            return new AndroidJavaObject("java.lang.String", CSharpString);
        }

        public Dictionary<string, GameObject> getNowList()
        {
            if (showType == 0)
            {
                return AllObject;
            }

            else
            {
                return AllObject;
            }
        }


        public Vector3 MoveToObject(string model)
        {
            string[] sArray = model.Split(';');


            float maxX = -999;
            float maxY = -999;
            float minX = 999;
            float minY = 999;
            float minZ = 999;
            float maxZ = -999;

            foreach (string s in sArray)
            {
                GameObject obj = getObject(s);
                if (obj != null)
                {
                    if (obj.active)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null && obj.transform.childCount == 0)
                        {
                            Bounds bounds = renderer.bounds;


                            float top = (float)((bounds.center.y + bounds.size.y / 2));
                            float bottom = (float)((bounds.center.y - bounds.size.y / 2));
                            float left = (float)((bounds.center.x - bounds.size.x / 2));
                            float right = (float)((bounds.center.x + bounds.size.x / 2));
                            float front = (float)((bounds.center.z + bounds.size.z / 2));
                            float back = (float)((bounds.center.z - bounds.size.z / 2));
                            if (top > maxY)
                            {
                                maxY = top;
                            }

                            if (top < minY)
                            {
                                minY = top;
                            }

                            if (bottom > maxY)
                            {
                                maxY = bottom;
                            }

                            if (bottom < minY)
                            {
                                minY = bottom;
                            }

                            if (left < minX)
                            {
                                minX = left;
                            }

                            if (left > maxX)
                            {
                                maxX = left;
                            }

                            if (right > maxX)
                            {
                                maxX = right;
                            }

                            if (right < minX)
                            {
                                minX = right;
                            }

                            if (front < minZ)
                            {
                                minZ = front;
                            }

                            if (front > maxZ)
                            {
                                maxZ = front;
                            }

                            if (back > maxZ)
                            {
                                maxZ = back;
                            }

                            if (back < minZ)
                            {
                                minZ = back;
                            }
                        }
                    }
                }
            }

            Vector3 center = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, minZ + (maxZ - minZ) / 2);


            double moveDistance = (maxX - minX) * (maxY - minY) * 1000000 / 27.05297 * 0.12f;
            if (moveDistance < 0.12)
            {
                moveDistance = 0.12;
            }

            Vector3 tmp = new Vector3(0, 0, -1 * ((float)moveDistance));


            /*            Vector3 end = center + mCamera.transform.rotation * tmp;
            */
            Vector3 end = new Vector3(center.x, center.y, -1 * ((float)moveDistance));
            ClickEvent.isMoveAnima = true;


            Tweener tweener = mCamera.transform.DOMove(end, 0.5f);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate() { isMoveAnima = false; };
            tweener.onUpdate = delegate() { mCamera.transform.LookAt(new Vector3(center.x, center.y, 0)); };
            tweener.onKill = delegate() { };


            return center;
        }


        public Vector3 shotCenter;

        public float getShowSize(string data)
        {
            maxX = -999;
            maxY = -999;
            minX = 999;
            minY = 999;
            minZ = 999;
            maxZ = -999;
            string[] datas = data.Split(';');
            foreach (string s in datas)
            {
                GameObject hideObj = getObject(s);
                //Debug.Log(s);
                if (hideObj.active && hideObj.transform.childCount == 0)
                {
                    Renderer meshRenderer = hideObj.GetComponent<MeshRenderer>();

                    Bounds bounds = meshRenderer.bounds;

                    float top = (float)(bounds.center.y + bounds.size.y / 2);
                    float bottom = (float)(bounds.center.y - bounds.size.y / 2);
                    float left = (float)(bounds.center.x - bounds.size.x / 2);
                    float right = (float)(bounds.center.x + bounds.size.x / 2);
                    float front = (float)(bounds.center.z + bounds.size.z / 2);
                    float back = (float)(bounds.center.z - bounds.size.z / 2);
                    if (top > maxY)
                    {
                        maxY = top;
                    }

                    if (top < minY)
                    {
                        minY = top;
                    }

                    if (bottom > maxY)
                    {
                        maxY = bottom;
                    }

                    if (bottom < minY)
                    {
                        minY = bottom;
                    }

                    if (left < minX)
                    {
                        minX = left;
                    }

                    if (left > maxX)
                    {
                        maxX = left;
                    }

                    if (right > maxX)
                    {
                        maxX = right;
                    }

                    if (right < minX)
                    {
                        minX = right;
                    }

                    if (front < minZ)
                    {
                        minZ = front;
                    }

                    if (front > maxZ)
                    {
                        maxZ = front;
                    }

                    if (back > maxZ)
                    {
                        maxZ = back;
                    }

                    if (back < minZ)
                    {
                        minZ = back;
                    }
                }
            }

            shotCenter = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, minZ + (maxZ - minZ) / 2);
            if (Math.Abs(maxY - minY) > Math.Abs(maxX - minX))
            {
                if (Math.Abs(maxY - minY) <= 0.006F)
                {
                    return 0.006F;
                }
                else
                {
                    return Math.Abs(maxY - minY);
                }
            }
            else
            {
                if (Math.Abs(maxX - minX) <= 0.006F)
                {
                    return 0.006F;
                }
                else
                {
                    return Math.Abs(maxX - minX);
                }
            }
        }

        /// <summary>
        /// 获得中心点
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector3 GetCenter()
        {
            if (isPlayAnimation)
            {
                return new Vector3(0, 0, 0);
            }

            maxX = -999;
            maxY = -999;
            minX = 999;
            minY = 999;
            minZ = 999;
            maxZ = -999;

            foreach (KeyValuePair<string, GameObject> items in getNowList())
            {
                GameObject obj = items.Value;
                if (obj != null)
                {
                    if (obj.active)
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null && obj.transform.childCount == 0)
                        {
                            Bounds bounds = renderer.bounds;

                            float top = (float)((bounds.center.y + bounds.size.y / 2));
                            float bottom = (float)((bounds.center.y - bounds.size.y / 2));
                            float left = (float)((bounds.center.x - bounds.size.x / 2));
                            float right = (float)((bounds.center.x + bounds.size.x / 2));
                            float front = (float)((bounds.center.z + bounds.size.z / 2));
                            float back = (float)((bounds.center.z - bounds.size.z / 2));
                            if (top > maxY)
                            {
                                maxY = top;
                            }

                            if (top < minY)
                            {
                                minY = top;
                            }

                            if (bottom > maxY)
                            {
                                maxY = bottom;
                            }

                            if (bottom < minY)
                            {
                                minY = bottom;
                            }

                            if (left < minX)
                            {
                                minX = left;
                            }

                            if (left > maxX)
                            {
                                maxX = left;
                            }

                            if (right > maxX)
                            {
                                maxX = right;
                            }

                            if (right < minX)
                            {
                                minX = right;
                            }

                            if (front < minZ)
                            {
                                minZ = front;
                            }

                            if (front > maxZ)
                            {
                                maxZ = front;
                            }

                            if (back > maxZ)
                            {
                                maxZ = back;
                            }

                            if (back < minZ)
                            {
                                minZ = back;
                            }
                        }
                    }
                }
            }

            RotationCenter = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, minZ + (maxZ - minZ) / 2);


            isClickCenter = false;

            return RotationCenter;
        }

        public void SendAnimationProgress(string progress)
        {
            if (animator != null && animationClip != null)
            {
                animator.Play(animationClip.name, 0, Convert.ToSingle(progress));
            }
        }

        public void ChangeAnimationState(string state)
        {
            isPlayAnimation = Convert.ToBoolean(state);
            if (isPlayAnimation)
            {
                animator.speed = 1;
            }
            else
            {
                animator.speed = 0;
            }
        }

        public void FinishLoad()
        {
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.FinishLoad("");
#endif
#if UNITY_WEBGL
                               JSPlugin.FinishLoad("");
#endif
#if UNITY_ANDROID
                    AndPlugin("finishLoad", "");
#else

#endif
            }
        }


        public void GetShowModel()
        {
            string showModelName = GetAllObjectKeys();

            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.GetShowModel(showModelName);
#endif
#if UNITY_WEBGL
                     JSPlugin.GetShowModel(showModelName);
#endif
#if UNITY_ANDROID
                    AndPlugin("GetShowModel", showModelName.ToString());

#else
#endif
            }
        }

        public void GetParent(string model)
        {
            GameObject obj = getObject(model);
            if (obj != null)
            {
                string clickModelName = GetModelValue(obj.transform.name) + ";";
                Transform parent = obj.transform;
                while (parent.parent != null)
                {
                    parent = parent.transform.parent;
                    clickModelName += GetModelValue(parent.name) + ";";
                }

                if (clickModelName.EndsWith(";"))
                {
                    clickModelName = clickModelName.Substring(0, clickModelName.Length - 1);
                }

                if (!TEST_PLUGIN)
                {
#if UNITY_IOS
                iOSPlugin.ReturnParentName(clickModelName);
#elif UNITY_WEBGL
                JSPlugin.ReturnParentName(clickModelName);
#else
                    AndPlugin("returnParentName", clickModelName);
#endif
                }
            }
            else
            {
                Debug.Log(model + " GetParent Obj is NUll");
            }
        }

        public void ShowSearchResult(string modelName)
        {
            if (mObj == null)
            {
                /*        mObj = (GameObject)Instantiate(Resources.Load("model/component/component"));
                        mObj.transform.position = new Vector3(0, 0, 0);
                        mObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                        mObj.AddComponent<SkeletonManager>();*/
                LoadFile();
            }


            mObj.SetActive(true);

            /*        foreach (KeyValuePair<string, GameObject> items in AllObject)
                    {
                        GameObject hideObj = items.Value;
                        if (hideObj != null)
                        {

                            MeshRenderer renderer = hideObj.GetComponent<MeshRenderer>();
                            if (renderer != null)
                            {
                                Material material = renderer.material;
                                material.DisableKeyword("_EMISSION");
                            }
                        }


                    }*/


            if (AllObject.Count == 0)
            {
                FindObjects(mObj, false, true);
            }
            else
            {
                HideBody("");
            }


            string[] sArray = modelName.Split(';');
            foreach (string i in sArray)
            {
                Debug.Log("search model = " + i);
                SetObjectStatus(getObject(i), true);
            }

            GetCenter();
            FinishLoad();
            //AddToOperateListNew();

            GetShowModel();
            FinishLoadSearchResult();
        }

        void FindObjects(GameObject obj, bool state, bool isSaveObject)
        {
            //Debug.Log("FindObjects obj="+obj+"  state="+state+"  save = "+isSaveObject);
            if (obj != null)
            {
                int i = 0;
                if (isSaveObject)
                {
                    /*     if (obj.transform.name.Contains("~@"))
                         {
                             AllObject.Add(obj.transform.name.Substring(obj.transform.name.LastIndexOf("~@") + 1), obj);

                         }
                         else {*/
                    string modelValue = GetModelValue(obj.transform.name);
                    if (showType == 0)
                    {
                        if (!AllObject.ContainsKey(modelValue))
                        {
                            AllObject.Add(modelValue, obj);
                        }
                    }
                    else if (showType == 2)
                    {
                    }
                    else if (showType == 3)
                    {
                    }
                    else if (showType == 5)
                    {
                    }


                    // }
                }

                if (obj.transform.childCount == 0)
                {
                    SetObjectStatus(obj, state);
                }

                else
                {
                    while (i < obj.transform.childCount)
                    {
                        Transform parent = obj.transform.GetChild(i);

                        if (parent.childCount > 0)
                        {
                            FindObjects(parent.gameObject, state, isSaveObject);
                        }
                        else if (parent.childCount == 0)
                        {
                            if (isSaveObject)
                            {
                                string modelValue = GetModelValue(parent.transform.name);
                                if (showType == 0)
                                {
                                    if (!AllObject.ContainsKey(modelValue))
                                    {
                                        AllObject.Add(modelValue, parent.gameObject);
                                    }
                                }
                                else if (showType == 2)
                                {
                                }
                                else if (showType == 3)
                                {
                                }
                                else if (showType == 5)
                                {
                                }

                                // }
                            }

                            SetObjectStatus(parent.gameObject, state);
                        }

                        i++;
                    }
                }
            }
        }

        public void MoveToObj(string modelName)
        {
            string[] sArray = modelName.Split(';');
            if (sArray.Length > 0)
            {
                startCameraMoveObj = getObject(sArray[0]);


                MeshRenderer renderer = startCameraMoveObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Vector3 center = renderer.bounds.center;
                    startAnimation(new Vector3(center.x, center.y, center.z - 0.3f), center);
                }

                /*     cameraMoveType = 1;
                     tempMoveType = 1;*/
            }
        }

        public void SetSelected(string modelName)
        {
            CancelSelect("");
            string[] sArray = modelName.Split(';');
            if (sArray.Length != 0)
            {
                foreach (string i in sArray)
                {
                    GameObject gameObject = getObject(i);
                    if (gameObject == null)
                    {
                        Debug.Log("SetSelected is null value = " + i);
                    }
                    else
                    {
                        UnityEngine.Color color = gameObject.GetComponent<Renderer>().material.GetColor("_Color");
                        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();

                        UnityEngine.Color nowColor;
                        string path = GetGameObjectPath(gameObject.transform);
                        if (path.StartsWith($"{modelPrefabName}(Clone)/淋巴系统~"))
                        {
                            //}

                            //if (path.StartsWith("全身人体(Clone)/淋巴系统~"))
                            //{
                            ColorUtility.TryParseHtmlString("#FF0000", out nowColor);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#7eb678", out nowColor);
                        }

                        //Debug.Log("set selected renderer = "+ renderer);
                        SetMeshColor(renderer, nowColor);


                        ObjectColorModel model = new ObjectColorModel();
                        model.obj = gameObject;
                        model.LastColor = color;
                        if (color != nowColor)
                        {
                            ThreeD_Object.LastObject.Add(model);
                        }
                    }
                }
            }
        }

        public void CancelSelect(string str)
        {
            if (ThreeD_Object.LastObject != null)
            {
                foreach (ObjectColorModel m in ThreeD_Object.LastObject)
                {
                    if (m.obj != null)
                    {
                        if (showType == 3)
                        {
                            if (m.obj.transform.parent.name.Equals("000"))
                            {
                                m.obj.GetComponent<MeshRenderer>().material
                                    .SetColor("_Color", new Color32(255, 255, 255, 255));
                            }
                            else
                            {
                                m.obj.GetComponent<MeshRenderer>().material
                                    .SetColor("_Color", new Color32(255, 255, 255, 0));
                            }
                        }
                        else
                        {
                            m.obj.GetComponent<MeshRenderer>().material.SetColor("_Color", m.LastColor);
                        }
                    }
                }

                ThreeD_Object.LastObject.Clear();
            }

            GetCenter();
        }

        private string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        public void FinishLoadSearchResult()
        {
            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.FinishLoadSearchResult("");
#endif
#if UNITY_WEBGL
                       JSPlugin.FinishLoadSearchResult("");
#endif
#if UNITY_ANDROID
                       AndPlugin("finishLoadSearchResult", "");

#else

#endif
            }
        }

        public void ToCenter(string modelName)
        {
            ReSetModel("");
            string[] sArray = modelName.Split(';');
            float maxX = 0;
            float minX = 0;
            float maxY = 0;
            float minY = 0;
            float maxZ = 0;
            float minZ = 0;
            if (sArray.Length != 0)
            {
                foreach (string i in sArray)
                {
                    GameObject gameObject = getObject(i);

                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    if (renderer != null && gameObject.transform.childCount == 0)
                    {
                        Bounds bounds = renderer.bounds;

                        float top = (float)((bounds.center.y + bounds.size.y / 2));
                        float bottom = (float)((bounds.center.y - bounds.size.y / 2));
                        float left = (float)((bounds.center.x - bounds.size.x / 2));
                        float right = (float)((bounds.center.x + bounds.size.x / 2));
                        float front = (float)((bounds.center.z + bounds.size.z / 2));
                        float back = (float)((bounds.center.z - bounds.size.z / 2));
                        if (top > maxY)
                        {
                            maxY = top;
                        }

                        if (top < minY)
                        {
                            minY = top;
                        }

                        if (bottom > maxY)
                        {
                            maxY = bottom;
                        }

                        if (bottom < minY)
                        {
                            minY = bottom;
                        }

                        if (left < minX)
                        {
                            minX = left;
                        }

                        if (left > maxX)
                        {
                            maxX = left;
                        }

                        if (right > maxX)
                        {
                            maxX = right;
                        }

                        if (right < minX)
                        {
                            minX = right;
                        }

                        if (front < minZ)
                        {
                            minZ = front;
                        }

                        if (front > maxZ)
                        {
                            maxZ = front;
                        }

                        if (back > maxZ)
                        {
                            maxZ = back;
                        }

                        if (back < minZ)
                        {
                            minZ = back;
                        }
                    }
                }

                Vector3 center = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2,
                    minZ + (maxZ - minZ) / 2);
                float area = (maxX - minX) * (maxY - minY);

                var camera = GameObject.Find("Camera");
                Quaternion rotation = Quaternion.Euler(camera.transform.position.y, camera.transform.position.x, 0);

                Vector3 tmp = new Vector3(0, 0, (float)(-1 * (5 * area / 0.00276F)));
                Vector3 position = rotation * tmp + center;

                camera.transform.rotation = rotation;
                camera.transform.position = position;
            }
        }

        public string GetScreenShot()
        {
            getImageCameraObject.SetActive(true);
            Rect rect = new Rect(0, 0, 720, 1080);

            // 创建一个RenderTexture对象
            RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 0);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机

            getImageCamera.targetTexture = rt;
            getImageCamera.clearFlags = CameraClearFlags.SolidColor;
            getImageCamera.Render();

            // 激活这个rt, 并从中中读取像素。
            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            screenShot.ReadPixels(rect, 0, 0); // 注：这个时候，它是从RenderTexture.active中读取像素
            screenShot.Apply();
            RenderTexture.ReleaseTemporary(rt);
            getImageCameraObject.SetActive(false);


            // 重置相关参数，以使用camera继续在屏幕上显示
            getImageCamera.targetTexture = null;
            //ps: camera2.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            // 最后将这些纹理数据，成一个png图片文件
            byte[] bytes = screenShot.EncodeToPNG();
            //File.WriteAllBytes("F:/test.jpg", bytes);
            string pic = Convert.ToBase64String(bytes);
            //File.WriteAllText("F:/test.txt", pic);

            return pic;
        }


        /// <summary>
        /// 抓取整个屏幕截图
        /// </summary>
        /// <returns></returns>
        public Texture2D CaptureScreen()
        {
            //用屏幕的宽度和高度创建一个新的纹理
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            StartCoroutine(CaptureScreenshot(texture));

            return texture;
        }

        IEnumerator CaptureScreenshot(Texture2D texture)
        {
            //只在每一帧渲染完成后才读取屏幕信息

            //读取屏幕像素信息并存储为纹理数据
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply(); // 这一句必须有，像素信息并没有保存在2D纹理贴图中

            yield return new WaitForEndOfFrame();
        }

        public byte[] GetScreenShot1()
        {
            Rect rect = new Rect(0, 0, 1024, 1024);

            // 创建一个RenderTexture对象
            RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
            Camera camera = Camera.main;

            camera.targetTexture = rt;
            camera.Render();
            //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。
            //ps: camera2.targetTexture = rt;
            //ps: camera2.Render();
            //ps: -------------------------------------------------------------------

            // 激活这个rt, 并从中中读取像素。
            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(rect, 0, 0); // 注：这个时候，它是从RenderTexture.active中读取像素
            screenShot.Apply();

            // 重置相关参数，以使用camera继续在屏幕上显示
            camera.targetTexture = null;
            //ps: camera2.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            GameObject.Destroy(rt);
            // 最后将这些纹理数据，成一个png图片文件
            byte[] bytes = screenShot.EncodeToPNG();

            return bytes;
        }

        private float xVelocitx = 0.0F;
        private float yVelocity = 0.0F;
        private float zVelocitz = 0.0F;
        float smoothPositionTime = 0.4F;

        private float xAnglex = 0.0F;
        private float yAngley = 0.0F;

        float smoothAngleTime = 0.1f;

        public void MoveToObj(GameObject obj)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                // 位移平滑阻尼
                float newPositionx = Mathf.SmoothDamp(transform.position.x, RotationCenter.x, ref xVelocitx,
                    smoothPositionTime);
                float newPositiony = Mathf.SmoothDamp(transform.position.y, RotationCenter.y, ref yVelocity,
                    smoothPositionTime);
                float newPositionz = Mathf.SmoothDamp(transform.position.z, RotationCenter.z - 0.3F, ref zVelocitz,
                    smoothPositionTime);
                transform.position = new Vector3(newPositionx, newPositiony, newPositionz);
                transform.LookAt(new Vector3(RotationCenter.x, RotationCenter.y, RotationCenter.z));
            }
            else
            {
                // 位移平滑阻尼
                float newPositionx = Mathf.SmoothDamp(transform.position.x, renderer.bounds.center.x, ref xVelocitx,
                    smoothPositionTime);
                float newPositiony = Mathf.SmoothDamp(transform.position.y, renderer.bounds.center.y, ref yVelocity,
                    smoothPositionTime);
                float newPositionz = Mathf.SmoothDamp(transform.position.z, renderer.bounds.center.z - 0.3F,
                    ref zVelocitz, smoothPositionTime);
                transform.position = new Vector3(newPositionx, newPositiony, newPositionz);
                transform.LookAt(new Vector3(renderer.bounds.center.x, renderer.bounds.center.y,
                    renderer.bounds.center.z));
                if (newPositionx == renderer.bounds.center.x && newPositiony == renderer.bounds.center.y &&
                    newPositionz == renderer.bounds.center.z)
                {
                    //cameraMoveType = 0;
                }
            }
        }

        public Vector3 GetFirstSeePoint(Vector3 position, Vector3 rotation)
        {
            /*       Ray ray = new Ray(new Vector3(1,1,-1.3F),new Vector3(0,0,1));

                    //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;//

                    if (Physics.Raycast(ray, out hit))
                    {
                        Vector3 vector = hit.point;//得到碰撞点的坐标
                                                   // 如果射线与平面碰撞，打印碰撞物体信息
                        Debug.Log("碰撞对象: " + hit.collider.name);
                        // 在场景视图中绘制射线
                        Debug.DrawLine(ray.origin, hit.point, UnityEngine.Color.black);
                        return vector;

                    }
                    else
                    {
                        Debug.Log("无碰撞对象: " );

                        return new Vector3(0, 0, 0);
                    }*/


            return new Vector3((float)(position.x - Math.Abs(position.z) / Math.Tan(Math.PI * (rotation.x / 180))),
                (float)(position.y -
                        Math.Abs(position.z) /
                        Math.Tan(Math.PI *
                                 (rotation.y /
                                  180))), /* (float)(position.z - position.z * Math.Cos(Math.PI * (rotation.z / 180)))*/
                0);
        }

        public Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal,
            Vector3 planePoint)
        {
            Vector3 dir = (Quaternion.Euler(direct) * Vector3.forward).normalized;
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(dir.normalized, planeNormal);
            return d * dir.normalized + point;
        }

        public static bool PlaneLineIntersectPoint(Vector3 planeVector, Vector3 planePoint, Vector3 lineVector,
            Vector3 linePoint, Vector3 rtn)
        {
            float vp1, vp2, vp3, n1, n2, n3, v1, v2, v3, m1, m2, m3, t, vpt;
            vp1 = planeVector.x;
            vp2 = planeVector.y;
            vp3 = planeVector.z;
            n1 = planePoint.x;
            n2 = planePoint.y;
            n3 = planePoint.z;
            v1 = lineVector.x;
            v2 = lineVector.y;
            v3 = lineVector.z;
            m1 = linePoint.x;
            m2 = linePoint.y;
            m3 = linePoint.z;

            vpt = v1 * vp1 + v2 * vp2 + v3 * vp3; //点乘
            //首先判断直线是否与平面平行(正确的浮点判断0方式)
            if (Math.Abs(vpt) <= 0.000001f)
            {
                rtn.x = rtn.y = rtn.z = 0;
                return false;
            }

            t = ((n1 - m1) * vp1 + (n2 - m2) * vp2 + (n3 - m3) * vp3) / vpt;
            rtn.x = m1 + v1 * t;
            rtn.y = m2 + v2 * t;
            rtn.z = m3 + v3 * t;
            return true;
        }

        public void MoveToPoint(Vector3 position, Vector3 endPosition)
        {
            /*        GameObject gameObject = getObject(modelName);
                    Renderer renderer = gameObject.GetComponent<Renderer>();*/


            // 位移平滑阻尼
            float newPositionx = Mathf.SmoothDamp(transform.position.x, position.x, ref xVelocitx, smoothPositionTime);
            float newPositiony = Mathf.SmoothDamp(transform.position.y, position.y, ref yVelocity, smoothPositionTime);
            float newPositionz = Mathf.SmoothDamp(transform.position.z, position.z, ref zVelocitz, smoothPositionTime);
            transform.position = new Vector3(newPositionx, newPositiony, newPositionz);

            transform.LookAt(new Vector3(endPosition.x, endPosition.y, endPosition.z));
            //transform.LookAt(new Vector3(0, 0, 0));
            if (newPositionx == position.x && newPositiony == position.y && newPositionz == position.z)
            {
                //cameraMoveType = 0;
            }
        }

        public void GetCameraInfo(string str)
        {
            string info = "位置为：x:" + transform.position.x + " y:" + transform.position.y + " z:" +
                          transform.position.z + "角度为：x:" + transform.rotation.x + " y:" + transform.rotation.y +
                          " z:" + transform.rotation.z;
#if UNITY_IOS
                                  iOSPlugin.ReturnCameraInfo(info);
#endif
#if UNITY_WEBGL
                                  JSPlugin.ReturnCameraInfo(info);
#endif

#if UNITY_ANDROID
                AndPlugin("returnCameraInfo", info);

#else

#endif
        }

        //修改选中mesh的颜色 ，所有mesh默认为：白色
        public void SetMeshColor(MeshRenderer meshRenderer, Color32 color)
        {
            if (meshRenderer != null)
            {
                if (isShowLittle || isShowLiving)
                {
                    UnityEngine.Color nowColor;
                    ColorUtility.TryParseHtmlString("#00FF00", out nowColor);
                    meshRenderer.material.SetColor("_Color", nowColor);
                }
                else
                {
                    meshRenderer.material.SetColor("_Color", color);
                }
            }
        }

        //设置透明度 数值是：0-1
        public void SetAlpha(MeshRenderer meshRenderer, float alpha)
        {
            if (meshRenderer != null)
            {
                //meshRenderer.material.SetFloat("_Transprent", alpha);
                Material oldMaterial = meshRenderer.material;

                if (alpha == 1)
                {
                    if (oldMaterial.shader.name.Equals("ametransbuckup"))
                    {
                        float lastOpacity = oldMaterial.GetFloat("_lastOpacity");
                        if (lastOpacity == 0)
                        {
                            Shader ameop = Shader.Find("ameop") ?? Shader.Find("ameop2");
                            //Debug.Log("222");
                            Material material = new Material(ameop);
                            material.renderQueue = oldMaterial.GetInt("_lastQueue");
                            material.SetTexture("_albe", oldMaterial.GetTexture("_albe"));
                            material.SetTexture("_normal", oldMaterial.GetTexture("_normal"));
                            material.SetTexture("_matcap", oldMaterial.GetTexture("_matcap"));
                            material.SetTexture("_TextureSample0", oldMaterial.GetTexture("_TextureSample0"));
                            material.SetColor("_Color", oldMaterial.GetColor("_Color"));
                            material.SetFloat("_matcapinstance", oldMaterial.GetFloat("_matcapinstance"));
                            material.SetFloat("_normalinstance", oldMaterial.GetFloat("_normalinstance"));
                            material.SetFloat("_Float0", oldMaterial.GetFloat("_Float0"));
                            material.SetFloat("_cullback", oldMaterial.GetFloat("_cullback"));

                            meshRenderer.material = material;
                        }
                        else
                        {
                            //Debug.Log("111");

                            Material material = new Material(Shader.Find("ametrans"));
                            material.renderQueue = oldMaterial.GetInt("_lastQueue");
                            material.SetTexture("_albe", oldMaterial.GetTexture("_albe"));
                            material.SetTexture("_normal", oldMaterial.GetTexture("_normal"));
                            material.SetTexture("_matcap", oldMaterial.GetTexture("_matcap"));
                            material.SetTexture("_TextureSample0", oldMaterial.GetTexture("_TextureSample0"));
                            material.SetColor("_Color", oldMaterial.GetColor("_Color"));
                            material.SetFloat("_matcapinstance", oldMaterial.GetFloat("_matcapinstance"));
                            material.SetFloat("_normalinstance", oldMaterial.GetFloat("_normalinstance"));
                            material.SetFloat("_opacity", oldMaterial.GetFloat("_lastOpacity"));
                            material.SetFloat("_Float0", oldMaterial.GetFloat("_Float0"));
                            material.SetFloat("_cullback", oldMaterial.GetFloat("_cullback"));

                            meshRenderer.material = material;
                        }
                    }
                }
                else
                {
                    if (!oldMaterial.shader.name.Equals("ametransbuckup"))
                    {
                        //Debug.Log("000");
                        Shader shader = Shader.Find("ametransbuckup");
                        if (shader == null)
                        {
                            Debug.Log("990");
                        }

                        Material material = new Material(shader);
                        material.renderQueue = 3000;
                        material.SetTexture("_albe", oldMaterial.GetTexture("_albe"));

                        material.SetTexture("_normal", oldMaterial.GetTexture("_normal"));

                        material.SetTexture("_matcap", oldMaterial.GetTexture("_matcap"));

                        material.SetTexture("_TextureSample0", oldMaterial.GetTexture("_TextureSample0"));

                        material.SetColor("_Color", oldMaterial.GetColor("_Color"));

                        material.SetFloat("_matcapinstance", oldMaterial.GetFloat("_matcapinstance"));

                        material.SetFloat("_normalinstance", oldMaterial.GetFloat("_normalinstance"));

                        material.SetFloat("_Float0", oldMaterial.GetFloat("_Float0"));

                        material.SetFloat("_cullback", oldMaterial.GetFloat("_cullback"));

                        material.SetFloat("_opacity", alpha);

                        float oldOpacity = 0;
                        try
                        {
                            string oldShaderName = oldMaterial.shader.name;
                            if (oldShaderName.Equals("asetrans") || oldShaderName.Equals("ametrans") ||
                                oldShaderName.Equals("bolicatmap") || oldShaderName.Equals("ametransbuckup"))
                            {
                                oldOpacity = oldMaterial.GetFloat("_opacity");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("996");
                            throw;
                        }

                        material.SetFloat("_lastOpacity", oldOpacity);
                        material.SetInt("_lastQueue", oldMaterial.renderQueue);

                        meshRenderer.material = material;
                    }
                }
            }
        }

        //回复成默认颜色和透明度
        public void SetOrigin(MeshRenderer meshRenderer, string textureName)
        {
            if (meshRenderer != null)
            {
                if (isShowLittle || isShowLiving)
                {
                    /*                Texture tex = Resources.Load<Texture>("Texture/" + textureName);
                                    meshRenderer.material.SetTexture("_MainTex", tex);*/

                    meshRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 255));
                }
                else
                {
                    Material material = meshRenderer.material;
                    material.DisableKeyword("_EMISSION");
                }
            }
        }

        public void ChangeBodyStatus(string data)
        {
            bool isShow = Convert.ToBoolean(data);
            mObj.SetActive(isShow);
            if (!isShow)
            {
                GameObject camera = GameObject.Find("Camera");
                LastCameraPosition = camera.transform.position;
                LastCameraRotation = camera.transform.rotation;
                camera.transform.position = new Vector3(0, 0, -10F);
            }
        }

        public void ShowBoneMark(string data, string types)
        {
            GameObject camera = GameObject.Find("Camera");
            LastCameraPosition = camera.transform.position;
            LastCameraRotation = camera.transform.rotation;
            ShowMark showMark = camera.GetComponent<ShowMark>();
            if (showMark == null)
            {
                showMark = camera.AddComponent<ShowMark>();
            }

            showType = 7;

            showMark.SyncModelData(mObj, AllObject);
            showMark.LoadShowModelBoneMark(data);
            showMark.StartDownloadImg(types);

            MoveToTopHalfView();
        }

        //data:  mark 从骨性标志 animation 从肌肉动作 muscle_mark
        public void ReturnShowBodyFrom(string data)
        {
            GameObject camera = GameObject.Find("Camera");

            if (data.Equals("mark"))
            {
                ShowMark showMark = camera.GetComponent<ShowMark>();
                if (showMark != null)
                {
                    showMark.ExitBoneMark();
                    //showMark.parentObj.SetActive(false);

                    //Destroy(showMark.parentObj);

                    GameObject.Destroy(showMark);
                }

                if (showModel != null)
                {
                    startCameraPosition = new Vector3(Convert.ToSingle(showModel.CameraPositionX),
                        Convert.ToSingle(showModel.CameraPositionY), Convert.ToSingle(showModel.CameraPositionZ));
                    startCameraLookPosition = GetIntersectWithLineAndPlane(startCameraPosition,
                        new Vector3(showModel.CameraRotationX, showModel.CameraRotationY, showModel.CameraRotationZ),
                        new Vector3(0, 0, 1), new Vector3(5, 5, 0));
                }

                //cameraMoveType = 0;
                showType = 0;
            }
            else if (data.Equals("animation"))
            {
                if (animObj != null)
                {
                    animObj.SetActive(false);
                    Destroy(animObj);
                }

                isPlayAnimation = false;
                if (showModel != null)
                {
                    startCameraPosition = new Vector3(Convert.ToSingle(showModel.CameraPositionX),
                        Convert.ToSingle(showModel.CameraPositionY), Convert.ToSingle(showModel.CameraPositionZ));
                    startCameraLookPosition = GetIntersectWithLineAndPlane(startCameraPosition,
                        new Vector3(showModel.CameraRotationX, showModel.CameraRotationY, showModel.CameraRotationZ),
                        new Vector3(0, 0, 1), new Vector3(5, 5, 0));
                }

                // cameraMoveType = 0;
            }
            else if (data.Equals("relationModel"))
            {
                /*     if (showModel != null)
                     {
                         startCameraPosition = new Vector3(Convert.ToSingle(showModel.CameraPositionX), Convert.ToSingle(showModel.CameraPositionY), Convert.ToSingle(showModel.CameraPositionZ));
                         startCameraLookPosition = GetIntersectWithLineAndPlane(startCameraPosition, new Vector3(showModel.CameraRotationX, showModel.CameraRotationY, showModel.CameraRotationZ), new Vector3(0, 0, 1), new Vector3(5, 5, 0));

                     }*/
                ShowLastBody("");
                //cameraMoveType = 2;
            }
            else if (data.Equals("muscle_mark"))
            {
            }
            else if (data.Equals("atlas"))
            {
            }
            else if (data.Equals("little"))
            {
            }

            showType = 0;

            mObj.SetActive(true);

            GetCenter();

            camera.transform.position = LastCameraPosition;

            camera.transform.rotation = LastCameraRotation;
        }

        public void HideBody(string modelName)
        {
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject obj = items.Value;
                if (obj.active && obj.transform.childCount == 0)
                {
                    obj.SetActive(false);
                }
            }
        }

        public string GetLastShowModel()
        {
            return GetAllObjectKeys();
        }

        public void SaveHideOthersShowModel(string modelName)
        {
            BeforeHideOthersShowModel = GetAllObjectKeys();
        }

        public void SaveLastShowModel(string modelName)
        {
            GameObject camera = GameObject.Find("Camera");
            LastCameraPosition = camera.transform.position;
            LastCameraRotation = camera.transform.rotation;

            LastShowModel = GetAllObjectKeys();
        }

        public void ShowLastBody(string data)
        {
            GameObject camera = GameObject.Find("Camera");
            camera.transform.position = LastCameraPosition;
            camera.transform.rotation = LastCameraRotation;
            if (LastShowModel != null && !LastShowModel.Equals(""))
            {
                string[] sArray = LastShowModel.Split(';');

                foreach (KeyValuePair<string, GameObject> items in AllObject)
                {
                    GameObject hideObj = items.Value;
                    if (hideObj.active && hideObj.transform.childCount == 0)
                    {
                        hideObj.SetActive(false);
                    }
                }

                foreach (string i in sArray)
                {
                    getObject(i).SetActive(true);
                }
                //SetAlpha(LastShowModel, 1f);
            }
        }

        public void ShowBeforeHideOthersBody(string data)
        {
            if (BeforeHideOthersShowModel != null && !BeforeHideOthersShowModel.Equals(""))
            {
                string[] sArray = BeforeHideOthersShowModel.Split(';');

                foreach (KeyValuePair<string, GameObject> items in AllObject)
                {
                    GameObject hideObj = items.Value;
                    if (hideObj.active && hideObj.transform.childCount == 0)
                    {
                        hideObj.SetActive(false);
                    }
                }

                foreach (string i in sArray)
                {
                    getObject(i).SetActive(true);
                }
                //SetAlpha(LastShowModel, 1f);
            }
        }


        public void ClearWaterMark()
        {
            GameObject backImage = GameObject.Find("Image");
            backImage.SetActive(false);
        }

        public void highLightShow(string modelName)
        {
            string[] sArray = modelName.Split(';');
            GameObject camera = GameObject.Find("Camera");
            LastCameraPosition = camera.transform.position;
            LastCameraRotation = camera.transform.rotation;
            //string hideModelName = modelName;
            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject hideObj = items.Value;
                if (hideObj.active && hideObj.transform.childCount == 0)
                {
                    MeshRenderer renderer = hideObj.GetComponent<MeshRenderer>();

                    if (sArray.Contains(items.Key))
                    {
                        SetAlpha(renderer, 1);
                    }
                    else
                    {
                        SetAlpha(renderer, 0.2F);
                    }
                }
            }

            /*        if (hideModelName.EndsWith(";"))
                    {
                        hideModelName = hideModelName.Substring(0, hideModelName.Length - 1);
                    }
                    if (!hideModelName.Equals("")) {

                        SetAlpha(hideModelName, 0.2f);


                    }*/
            foreach (string s in sArray)
            {
                getObject(s).SetActive(true);
                /*
                     MeshRenderer renderer = getObject(s).GetComponent<MeshRenderer>();
                     SetMeshColor(renderer, new Color32(0, 255, 0, 255));
                 */
            }

            GetCenter();
            //GetShowModel();
        }

        public void resetScence()
        {
            if (animObj != null)
            {
                animObj.SetActive(false);
                GameObject.DestroyImmediate(animObj);
            }

            if (littleObj != null)
            {
                littleObj.SetActive(false);
                GameObject.DestroyImmediate(littleObj);
            }

            if (showType == 0)
            {
                SetAllNotAlpha("");
            }

            if (mObj != null)
            {
                mObj.SetActive(false);
            }

            if (mLivingObj != null)
            {
                mLivingObj.SetActive(false);

                GameObject.DestroyImmediate(mLivingObj);

                mLivingObj = null;
            }

            if (mSectionObj != null)
            {
                mSectionObj.SetActive(false);
                GameObject.DestroyImmediate(mSectionObj);

                mSectionObj = null;
            }


            showModelStr = "";
            //OperateList = new List<OperateModelNew>();//操作流集合
            // AllObject = new Dictionary<string, GameObject>();
            isAutoDisable = false; //是否自动隐藏
            isManyChoose = false; //是否多选

            minX = 0;
            minY = 0;
            minZ = 0;

            maxX = 0;
            maxY = 0;
            maxZ = 0;
            NowOperateIndex = -1;
            showModel = null;

            //cameraMoveType = 0;//0 不移动 1 向obj移动 2 向指定位置移动
            //tempMoveType = 0;
            showType = 0;
            isPlayAnimation = false;
            isShowLittle = false;
            isShowLiving = false;
            animator = null;
            animationClip = null;
            isShowFeMale = false; //是否显示女性
            startTime = 0;
            //canTouch = false;

            LastShowModel = "";
            animObj = null;
            littleObj = null;
        }

        public void OpenMenuToMove(string isOpen)
        {
            GameObject camera = GameObject.Find("Camera");
            Vector3 tmp;

            if (isOpen.Equals("up"))
            {
                tmp = new Vector3(0, 0, -1 * (0.2F));
            }
            else
            {
                tmp = new Vector3(0, 0, 1 * (0.2F));
            }

            Vector3 end = camera.transform.position + camera.transform.rotation * tmp;
            ClickEvent.isMoveAnima = true;

            Tweener tweener = camera.transform.DOMove(end, 0.5f);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.onComplete = delegate() { ClickEvent.isMoveAnima = false; };
        }


        public void GoCameraView(float tweenerTime = 0.5f)
        {
            //getShowSize(showModelValue);
            GetCenter();
            GameObject camera = GameObject.Find("Camera");
            //camera.transform.position = LastCameraPosition;

            if (camera.transform.rotation != LastCameraRotation)
            {
                camera.transform.rotation = LastCameraRotation;
                tweenerTime = 0f;
            }

            //如果高度占用超过0.6，某些模型宽度会超出屏幕
            float viewHeightPercent = 0.5F;
            double moveDistance = GetViewPercentDistance(viewHeightPercent);

            Vector3 tmp = new Vector3(0, 0, (float)-moveDistance);
            Vector3 end = RotationCenter + camera.transform.rotation * tmp;

            isMoveAnima = true;

            Tweener tweener = camera.transform.DOMove(end, tweenerTime);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate() { isMoveAnima = false; };
            tweener.onKill = delegate() { };
        }

        public void MoveToTopHalfView()
        {
            GameObject camera = GameObject.Find("Camera");

            float tweenerTime = 0.5f;
            //如果旋转了相机，move 背面会出现上下颠倒的问题。所以如果旋转了，就归位，且不执行过场动画
            if (camera.transform.rotation != LastCameraRotation)
            {
                camera.transform.rotation = LastCameraRotation;
                tweenerTime = 0f;
            }

            GetCenter();

            float viewHeightPercent = 0.35F;
            float modelHeight = maxY - minY;
            double moveDistance = GetViewPercentDistance(viewHeightPercent);

            float centerMove = modelHeight / 2F + (0.5F - viewHeightPercent) / 2F * modelHeight * 2.5F;

            RotationCenter.y -= centerMove;
            Vector3 tmp = new Vector3(0, 0, (float)-moveDistance);
            Vector3 end = RotationCenter + camera.transform.rotation * tmp;


            isMoveAnima = true;

            Tweener tweener = camera.transform.DOMove(end, tweenerTime);
            tweener.SetUpdate(true);
            tweener.SetEase(Ease.Linear);
            tweener.SetAutoKill(true);
            tweener.onComplete = delegate() { isMoveAnima = false; };
            tweener.onKill = delegate() { };
        }

        //获取模型到相机的缩放视距
        private double GetViewPercentDistance(float viewHeightPercent)
        {
            float modelHeight = maxY - minY;
            float modelWidth = maxX - minX;

            //宽度参照模型1000032占用屏幕宽度的0.75
            float model2ViewMaxWidth = 0.004764621F;
            //高度参照模型1000229;1000257;1000417 最小高度
            float model2ViewMinHeight = 0.00104094797279686F;

            if (modelWidth / modelHeight > 1.5F)
            {
                viewHeightPercent = viewHeightPercent / modelWidth * modelHeight;
            }
            else
            {
                if (modelWidth > model2ViewMaxWidth)
                {
                    viewHeightPercent = viewHeightPercent / modelWidth * model2ViewMaxWidth;
                }

                if (modelHeight < model2ViewMinHeight)
                {
                    viewHeightPercent = viewHeightPercent * modelHeight / model2ViewMinHeight;
                }
            }

            //0.1647022/0.008915015 视距 = 目标模型高度 * 参照视距 / 参照模型高度（拿一个固定模型获取）* 屏幕高度百分比（因为是负方向，所以用/）
            return modelHeight * 0.10087269 / 0.008915015 / viewHeightPercent;
        }

        public void GetParentAndSelect(string str)
        {
            string clickModelName = GetModelValue(getObject(str).transform.name) + ";";
            Transform parent = getObject(str).transform;

            while (parent.parent != null)
            {
                parent = parent.transform.parent;
                clickModelName += GetModelValue(parent.name) + ";";
            }

            if (clickModelName.EndsWith(";"))
            {
                clickModelName = clickModelName.Substring(0, clickModelName.Length - 1);
            }

            if (!TEST_PLUGIN)
            {
#if UNITY_IOS
                        iOSPlugin.ClickModelName(clickModelName);
#elif UNITY_WEBGL
                           JSPlugin.ClickModelName(clickModelName);

#else
                AndPlugin("clickModel", clickModelName);

#endif
            }
        }

        public Vector3 GetEndPointByTrigonometric(GameObject showObj, Boolean angle, Vector3 StartPoint, float distance)
        {
            Renderer[] renders = showObj.GetComponentsInChildren<Renderer>();

            Bounds bounds = renders[0].bounds;
            Vector3 center = bounds.center;

            Vector3 EndPoint = getEndPoint(center.x, center.y, StartPoint.x, StartPoint.y, distance, angle);
            EndPoint.z = StartPoint.z;

            return EndPoint;
        }

        public Vector3 getEndPoint(double xa, double ya, double xb, double yb, double d, Boolean isReturn)
        {
            double xab, yab;
            double xbd, ybd;
            double xd, yd;

            xab = xb - xa;
            yab = yb - ya;

            xbd = Math.Sqrt((d * d) / ((yab / xab) * (yab / xab) + 1));

            if (xab > 0)
            {
                xbd = Math.Sqrt((d * d) / ((yab / xab) * (yab / xab) + 1));
            }
            else
            {
                xbd = -Math.Sqrt((d * d) / ((yab / xab) * (yab / xab) + 1));
            }

            if (isReturn)
            {
                xd = xb - xbd;
                yd = yb - yab / xab * xbd;
            }
            else
            {
                xd = xb + xbd;
                yd = yb + yab / xab * xbd;
            }

            return new Vector3(Convert.ToSingle(xd), Convert.ToSingle(yd));
        }

        private string GetModelValue(string modelName)
        {
            if (modelName.Contains("~"))
            {
                //return modelName.Substring(modelName.LastIndexOf("~") + 1).Trim();
                string modelValue = modelName.Substring(modelName.LastIndexOf("~") + 1).Trim();
                //if (modelValue.Length > 7)
                //{
                //    return modelValue.Substring(0, 7);
                //}
                //else {
                return modelValue;
                //}
            }
            else
            {
                return modelName.Trim();
            }
        }

        private string GetAllObjectKeys()
        {
            StringBuilder data = new StringBuilder();


            foreach (KeyValuePair<string, GameObject> items in AllObject)
            {
                GameObject obj = items.Value;
                if (obj.active && obj.transform.childCount == 0)
                {
                    data.Append(";").Append(items.Key);
                }
            }

            if (data.Length > 0)
            {
                data = data.Remove(0, 1);
            }

            return data.ToString();
        }

        private void AndPlugin(string methodName, params object[] args)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
            // AndroidJavaObject _ajc = new AndroidJavaObject("com.wazitan.anatomy.activity.UnityPlayerActivity");
            bool success = _ajc.Call<bool>(methodName, args);
        }


        public void ReturnParent()
        {
            GameObject[] obj = FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
            string data = "[";

            foreach (GameObject child in obj) //遍历所有gameobject
            {
                if (child.transform.parent != null)
                {
                    string childName = GetModelValue(child.name);
                    string parentName = GetModelValue(child.transform.parent.name);
                    if (child.transform.childCount == 0)
                    {
                        data += "{\"is_parent\":0,\"value\":\"" + childName + "\",\"parent_value\":\"" + parentName +
                                "\"},";
                    }
                    else
                    {
                        data += "{\"is_parent\":1,\"value\":\"" + childName + "\",\"parent_value\":\"" + parentName +
                                "\"},";
                    }
                }
            }

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "abv1.json"), "创建连接字符串开始");


            StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "abv1.json"), true);
            sw.Write(data);
            sw.Close();
        }
    }
}
