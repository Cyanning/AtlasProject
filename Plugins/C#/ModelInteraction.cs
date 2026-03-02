using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class ModelInteraction : MonoBehaviour
    {
        public static readonly List<ObjectColorModel> LastObject = new ();
        private static GameObject _mainCamera;
        private static Color _lastColor;
        private static AtlasCanvas _atlasCanvas;

        public Vector3 center;
        public long beginTime;
        public long lastClickTime;

        private FingerTouchForLittle _fingerTouch;
        private ClickEvent _clickEvent;
        private static readonly int StateColor = Shader.PropertyToID("_Color");
        private static readonly int Transprent = Shader.PropertyToID("_Transprent");

        public void Start()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            _fingerTouch = _mainCamera.GetComponent<FingerTouchForLittle>();
            _clickEvent = _mainCamera.GetComponent<ClickEvent>();
            _atlasCanvas = GameObject.FindGameObjectWithTag("GameController").GetComponent<AtlasCanvas>();
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
                    tweener.onComplete = static delegate
                    {
                        ClickEvent.isMoveAnima = false;
                        ClickEventTrigger();
                    };
                    tweener.onKill = static delegate { };
                }
                else
                {
                    //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                    //_clickEvent.SetAlpha(clickModelName);
                    ClickEventTrigger();
                }
            }
            else
            {
                //string clickModelName = transform.name.Substring(transform.name.LastIndexOf("~") + 1);
                //_clickEvent.SetAlpha(clickModelName);
                ClickEventTrigger();
            }


            //_clickEvent.cameraMoveType = 0;

            if (!ClickEvent.isAutoDisable)
            {
                _lastColor = transform.gameObject.GetComponent<MeshRenderer>().material.GetColor(StateColor);
                if (_clickEvent.showType == 0 && LastObject.Count > 0 &&
                    LastObject[^1].Obj.transform.name.Equals(transform.name))
                {
                    return;
                }

                //Boolean isClicked = false;
                foreach (var m in LastObject)
                {
                    if (m.Obj == transform.gameObject)
                    {
                        _lastColor = m.LastColor;
                    }
                }


                if (!ClickEvent.isManyChoose || _clickEvent.showType == 2)
                {
                    foreach (var m in LastObject)
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

                    LastObject.Clear();
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

                var model = new ObjectColorModel {Obj = transform.gameObject, LastColor = _lastColor};
                if (_lastColor != nowColor)
                {
                    LastObject.Add(model);
                }
            }
            // else
            // {
            //     var clickModelName =
            //         transform.name[(transform.name.LastIndexOf("~", StringComparison.Ordinal) + 1)..];
            // }
        }

        private static void ClickEventTrigger()
        {
            _atlasCanvas.CreateActiveLabel();
        }

        private static string GetGameObjectPath(Transform outTransform)
        {
            var tree = outTransform.name;
            while (outTransform.parent != null)
            {
                outTransform = outTransform.parent;
                tree = outTransform.name + "/" + tree;
            }

            return tree;
        }


        //修改选中mesh的颜色 ，所有mesh默认为：白色
        private static void SetMeshColor(Renderer meshRenderer, Color32 color, bool isShowLittle)
        {
            if (meshRenderer != null)
            {
                if (isShowLittle)
                {
                    ColorUtility.TryParseHtmlString("#00FF00", out var nowColor);
                    meshRenderer.material.SetColor(StateColor, nowColor);
                    //meshRenderer.material.SetTexture("_MainTex", null);
                }
                else
                {
                    //meshRenderer.material.EnableKeyword("_EMISSION");
                    meshRenderer.material.SetColor(StateColor, color);
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
                meshRenderer.material.SetFloat(Transprent, alpha);
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

        public Vector3 GetEndPointByTrigonometric1(Vector3 angle, Vector3 startPoint, float distance)
        {
            var endPoint = GetEndPoint1(startPoint, angle, distance);

            return endPoint;
        }

        private Vector3 GetEndPoint1(Vector3 startPoint, Vector3 angle, float distance)
        {
            var endPoint = new Vector3();
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
                endPoint.x = startPoint.x + (float)(distance * Math.Cos(radianX));
            }
            else
            {
                endPoint.x = startPoint.x - (float)(distance * Math.Cos(radianX));
            }

            if (transform.position.y > startPoint.y)
            {
                endPoint.y = startPoint.y - (float)(distance * Math.Cos(radianY));
            }
            else
            {
                endPoint.y = startPoint.y + (float)(distance * Math.Cos(radianY));
            }

            if (transform.position.z > startPoint.z)
            {
                endPoint.z = startPoint.z + (float)(distance * Math.Sin(radianZ));
            }
            else
            {
                endPoint.z = startPoint.z - (float)(distance * Math.Sin(radianZ));
            }

            return endPoint;
        }
    }
}
