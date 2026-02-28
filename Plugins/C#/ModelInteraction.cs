using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class ModelInteraction : MonoBehaviour
    {
        public static List<ObjectColorModel> lastObject = new List<ObjectColorModel>();
        public Vector3 center;
        public long beginTime;
        public long lastClickTime;

        public static Color lastColor;
        public bool clickState;

        private GameObject _mainCamera;
        private FingerTouchForLittle _fingerTouch;
        private ClickEvent _clickEvent;
        private static readonly int StateColor = Shader.PropertyToID("_Color");

        public void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            _fingerTouch = _mainCamera.GetComponent<FingerTouchForLittle>();
            _clickEvent = _mainCamera.GetComponent<ClickEvent>();
        }

        public void OnClickCubeItem(UnityEngine.EventSystems.BaseEventData data = null)
        {
            if (_fingerTouch.isDrag)
            {
                return;
            }

            if (_fingerTouch.supplyTouchType != 0)
            {
                return;
            }

            if (ClickEvent.isMoveAnima)
            {
                return;
            }
    
            if (_clickEvent.showType is not (0 or 2 or 3 or 5))
            {
                return;
            }

            clickState = !clickState;
            var targetRenderer = transform.gameObject.GetComponent<MeshRenderer>();

            if (_clickEvent.showType == 0)
            {
                var timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var nowTime = Convert.ToInt64(timeSpan.TotalSeconds);
                var isMove = nowTime - lastClickTime <= 0.8F;  // 是否开启移动动画

                if (_clickEvent.isResetStatus)
                {
                    isMove = true;
                    _clickEvent.isResetStatus = false;
                }

                lastClickTime = nowTime;
                var bounds = targetRenderer.bounds;
                center = bounds.center;
                ClickEvent.RotationCenter = center;
                _clickEvent.isClickCenter = true;
                if (isMove)
                {
                    var moveDistance = bounds.size.x * bounds.size.y * 1000000 / 27.05297 * 0.13f;
                    if (moveDistance < 0.13)
                    {
                        moveDistance = 0.13;
                    }

                    var tmp = new Vector3(0, 0, -1 * ((float)moveDistance));

                    //Sequence quence = DOTween.Sequence();

                    var end = center + _mainCamera.transform.rotation * tmp;
                    ClickEvent.isMoveAnima = true;

                    var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    _clickEvent.beginAnimTime = Convert.ToInt64(ts.TotalSeconds);

                    Tweener tweener = _mainCamera.transform.DOMove(end, 0.5f);
                    tweener.SetUpdate(true);
                    tweener.SetEase(Ease.Linear);
                    tweener.SetAutoKill(true);
                    tweener.onComplete = delegate()
                    {
                        ClickEvent.isMoveAnima = false;
                        SendClickToApp();
                    };
                    tweener.onKill = static delegate { };
                }
                else
                {
                    //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                    //_clickEvent.SetAlpha(clickModelName);
                    SendClickToApp();
                }
            }
            else
            {
                //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                //_clickEvent.SetAlpha(clickModelName);
                SendClickToApp();
            }


            //_clickEvent.cameraMoveType = 0;

            if (!ClickEvent.isAutoDisable)
            {
                var lastColor = transform.gameObject.GetComponent<MeshRenderer>().material.GetColor(StateColor);
                if (_clickEvent.showType == 0 && lastObject.Count > 0 &&
                    lastObject[^1].Obj.transform.name.Equals(transform.name))
                {
                    return;
                }

                //Boolean isClicked = false;
                foreach (var m in lastObject)
                {
                    if (m.Obj == transform.gameObject)
                    {
                        lastColor = m.LastColor;
                    }
                }


                if (!ClickEvent.isManyChoose || _clickEvent.showType == 2)
                {
                    foreach (var m in lastObject)
                    {
                        if (m.Obj is null) continue;
                        if (_clickEvent.showType == 3)
                        {
                            m.Obj.GetComponent<MeshRenderer>().material.SetColor(
                                StateColor,
                                m.Obj.transform.parent.name.Equals("000")
                                    ?new Color32(255, 255, 255, 255)
                                    :new Color32(255, 255, 255, 0)
                            );
                        }
                        else
                        {
                            m.Obj.GetComponent<MeshRenderer>().material.SetColor(StateColor, m.LastColor);
                        }
                    }

                    lastObject.Clear();
                }

                Color nowColor;
                var path = GetGameObjectPath(transform);
                if (path.Contains("淋巴系统~150001"))
                {
                    ColorUtility.TryParseHtmlString("#FF0000", out nowColor);
                }
                else
                {
                    ColorUtility.TryParseHtmlString("#7eb678", out nowColor);
                }

                if (_clickEvent.showType == 3)
                {
                    ColorUtility.TryParseHtmlString("#00FF00", out nowColor);
                }

                SetMeshColor(targetRenderer, nowColor, _clickEvent.isShowLittle);

                var model = new ObjectColorModel {Obj = transform.gameObject, LastColor = lastColor};
                if (lastColor != nowColor)
                {
                    lastObject.Add(model);
                }
            }
            else
            {
                var clickModelName =
                    transform.name[(transform.name.LastIndexOf("~", StringComparison.Ordinal) + 1)..];
            }
        }

        private void SendClickToApp()
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
        }

        //对象移动时调用
        void AnimationEnd(string f)
        {
            /*        GameObject camera = GameObject.Find("Camera");
                    camera.transform.rotation = Quaternion.Euler(0, -360, 0);
                    Debug.Log("end : " + f);*/
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
                    meshRenderer.material.SetColor(StateColor, new Color32(255, 255, 255, 255));
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
