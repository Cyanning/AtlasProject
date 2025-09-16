using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class ThreeD_Object : MonoBehaviour
    {
        public static Color LastColor;
        public static List<ObjectColorModel> LastObject = new List<ObjectColorModel>();
        public Vector3 center;
        public long beginTime;
        public long lastClickTime;

        public void OnClickCubeItem(UnityEngine.EventSystems.BaseEventData data = null)
        {
            GameObject camera = GameObject.Find("Camera");
            FingerTouchForLittle fingerTouch = camera.GetComponent<FingerTouchForLittle>();

            if (fingerTouch.isDrag)
            {
                return;
            }

            if (fingerTouch.supplyTouchType != 0)
            {
                return;
            }

            if (ClickEvent.isMoveAnima)
            {
                return;
            }

            ClickEvent clickEvent = camera.GetComponent<ClickEvent>();

            if (clickEvent.showType != 0 && clickEvent.showType != 2 && clickEvent.showType != 3 &&
                clickEvent.showType != 5)
            {
                return;
            }

            MeshRenderer renderer = transform.gameObject.GetComponent<MeshRenderer>();

            if (clickEvent.showType == 0)
            {
                TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long nowTime = Convert.ToInt64(timeSpan.TotalSeconds);
                bool isMove = false; // 是否开启移动动画
                if (nowTime - lastClickTime <= 0.8F)
                {
                    isMove = true;
                }

                if (clickEvent.isResetStatus)
                {
                    isMove = true;
                    clickEvent.isResetStatus = false;
                }

                lastClickTime = nowTime;
                Bounds bounds = renderer.bounds;
                center = bounds.center;
                ClickEvent.RotationCenter = center;
                clickEvent.isClickCenter = true;
                if (isMove)
                {
                    Vector3 begin = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);

                    Vector3 velocity = camera.transform.eulerAngles;
                    //Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, velocity).eulerAngles;
                    //Vector3 end = GetEndPointByTrigonometric1(velocity, begin, 1F);
                    double moveDistance = bounds.size.x * bounds.size.y * 1000000 / 27.05297 * 0.13f;
                    if (moveDistance < 0.13)
                    {
                        moveDistance = 0.13;
                    }

                    Vector3 tmp = new Vector3(0, 0, -1 * ((float)moveDistance));

                    //Sequence quence = DOTween.Sequence();

                    Vector3 end = center + camera.transform.rotation * tmp;
                    ClickEvent.isMoveAnima = true;

                    TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    clickEvent.beginAnimTime = Convert.ToInt64(ts.TotalSeconds);

                    Tweener tweener = camera.transform.DOMove(end, 0.5f);
                    tweener.SetUpdate(true);
                    tweener.SetEase(Ease.Linear);
                    tweener.SetAutoKill(true);
                    tweener.onComplete = delegate()
                    {
                        ClickEvent.isMoveAnima = false;
                        SendClickToApp();
                    };
                    tweener.onKill = delegate() { };
                }
                else
                {
                    //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                    //clickEvent.SetAlpha(clickModelName);
                    SendClickToApp();
                }
            }
            else
            {
                //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                //clickEvent.SetAlpha(clickModelName);
                SendClickToApp();
            }


            //clickEvent.cameraMoveType = 0;

            if (!ClickEvent.isAutoDisable)
            {
                Color lastColor = transform.gameObject.GetComponent<MeshRenderer>().material.GetColor("_Color");
                if (clickEvent.showType == 0 && LastObject.Count > 0 &&
                    LastObject[LastObject.Count - 1].obj.transform.name.Equals(transform.name))
                {
                    return;
                }

                //Boolean isClicked = false;
                foreach (ObjectColorModel m in LastObject)
                {
                    if (m.obj == transform.gameObject)
                    {
                        lastColor = m.LastColor;
                    }
                }


                if (!ClickEvent.isManyChoose || clickEvent.showType == 2)
                {
                    foreach (ObjectColorModel m in LastObject)
                    {
                        if (m.obj != null)
                        {
                            if (clickEvent.showType == 3)
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

                    LastObject.Clear();
                }

                Color nowColor;
                string path = GetGameObjectPath(transform);
                if (path.Contains("淋巴系统~150001"))
                {
                    ColorUtility.TryParseHtmlString("#FF0000", out nowColor);
                }
                else
                {
                    ColorUtility.TryParseHtmlString("#7eb678", out nowColor);
                }

                if (clickEvent.showType == 3)
                {
                    ColorUtility.TryParseHtmlString("#00FF00", out nowColor);
                }

                SetMeshColor(renderer, nowColor, clickEvent.isShowLittle);

                ObjectColorModel model = new ObjectColorModel();
                model.obj = transform.gameObject;
                model.LastColor = lastColor;
                if (lastColor != nowColor)
                {
                    LastObject.Add(model);
                }
            }
            else
            {
                string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);

#if UNITY_IOS
               iOSPlugin.ReturnSoonHideClickModel(clickModelName);
#elif UNITY_WEBGL
               JSPlugin.soonHideClickModel(clickModelName);
#elif UNITY_ANDROID
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
            bool success = _ajc.Call<bool>("soonHideClickModel", new object[] { clickModelName });
#else

#endif
            }
        }

        void SendClickToApp()
        {
            string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1) + ";";
            if (clickModelName.Equals("default;"))
            {
                clickModelName = transform.parent.parent.name.Replace("(Clone)", "") + "_" + transform.parent.name;
            }
            else
            {
                Transform parent = transform;

                while (parent.parent != null)
                {
                    parent = parent.transform.parent;
                    clickModelName += parent.name.Substring(parent.name.LastIndexOf("~") + 1) + ";";
                }

                if (clickModelName.EndsWith(";"))
                {
                    clickModelName = clickModelName.Substring(0, clickModelName.Length - 1);
                }
            }

#if UNITY_IOS
               iOSPlugin.ClickModelName(clickModelName);
#elif UNITY_WEBGL
               JSPlugin.ClickModelName(clickModelName);
#elif UNITY_ANDROID
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
        // AndroidJavaObject _ajc = new AndroidJavaObject("com.wazitan.anatomy.activity.UnityPlayerActivity");
        bool success = _ajc.Call<bool>("clickModel", new object[] { clickModelName });
#else
#endif
        }


        //对象移动时调用
        void AnimationEnd(string f)
        {
            /*        GameObject camera = GameObject.Find("Camera");
                    camera.transform.rotation = Quaternion.Euler(0, -360, 0);
                    Debug.Log("end : " + f);*/
        }

        void Update()
        {
            /*     if (isMoveAnima) {
                     GameObject camera = GameObject.Find("Camera");
                     iTween.LookTo(camera, center, 0.3F);
                 }*/
            /*       GameObject camera = GameObject.Find("Camera");
                   camera.DoLookAt(center,0f);*/
        }

        //对象移动时调用
        void AnimationUpdate(bool b)
        {
            /*        GameObject camera = GameObject.Find("Camera");
                    transform.LookAt(center);*/
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


        //修改选中mesh的颜色 ，所有mesh默认为：白色
        public void SetMeshColor(MeshRenderer meshRenderer, Color32 color, bool isShowLittle)
        {
            if (meshRenderer != null)
            {
                if (isShowLittle)
                {
                    Color nowColor;
                    ColorUtility.TryParseHtmlString("#00FF00", out nowColor);
                    meshRenderer.material.SetColor("_Color", nowColor);
                    //meshRenderer.material.SetTexture("_MainTex", null);
                }
                else
                {
                    //meshRenderer.material.EnableKeyword("_EMISSION");
                    meshRenderer.material.SetColor("_Color", color);
                    //Texture texture = meshRenderer.material.GetTexture("_MainTex");
                    //meshRenderer.material.SetTexture("_EmissionMap", texture);
                }
            }
        }

        //设置透明度 数值是：0-1
        public void SetAlpha(MeshRenderer meshRenderer, float alpha)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.SetFloat("_Transprent", alpha);
            }
        }

        //回复成默认颜色和透明度
        public void SetOrigin(MeshRenderer meshRenderer, string textureName, bool isShowLittle)
        {
            if (meshRenderer != null)
            {
                if (isShowLittle)
                {
                    meshRenderer.material.SetColor("_Color", new Color32(255, 255, 255, 255));
                }
                else
                {
                    meshRenderer.material.DisableKeyword("_EMISSION");
                }
            }
        }

        public Vector3 GetEndPointByTrigonometric1(Vector3 angle, Vector3 StartPoint, float distance)
        {
            Vector3 EndPoint = getEndPoint1(StartPoint, angle, distance);

            return EndPoint;
        }

        public Vector3 getEndPoint1(Vector3 startPoint, Vector3 angle, float distance)
        {
            Vector3 EndPoint = new Vector3();
            //角度转弧度
            var radianX = (angle.x * Math.PI) / 180;
            var radianY = (angle.y * Math.PI) / 180;
            var radianZ = (angle.z * Math.PI) / 180;
            /*        var radianX = angle.x;
                    var radianY = angle.y;
                    var radianZ = angle.z;*/
            //计算新坐标 r 就是两者的距离

            if (transform.position.x > startPoint.x)
            {
                EndPoint.x = startPoint.x + float.Parse((distance * Math.Cos(radianX)).ToString());
            }
            else
            {
                EndPoint.x = startPoint.x - float.Parse((distance * Math.Cos(radianX)).ToString());
            }

            if (transform.position.y > startPoint.y)
            {
                EndPoint.y = startPoint.y - float.Parse((distance * Math.Cos(radianY)).ToString());
            }
            else
            {
                EndPoint.y = startPoint.y + float.Parse((distance * Math.Cos(radianY)).ToString());
            }

            if (transform.position.z > startPoint.z)
            {
                EndPoint.z = startPoint.z + float.Parse((distance * Math.Sin(radianZ)).ToString());
            }
            else
            {
                EndPoint.z = startPoint.z - float.Parse((distance * Math.Sin(radianZ)).ToString());
            }

            return EndPoint;
        }
    }
}
