using UnityEngine;
using static Plugins.C_.ClickEvent;
using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// 点击屏幕实现缩放与旋转，移动
/// </summary>

namespace Plugins.C_
{
    public class FingerTouchForLittle : MonoBehaviour
    {
        private Touch oldTouch1; //上次触摸点1(手指1)
        private Touch oldTouch2; //上次触摸点2(手指2)
        public float distance = 1.3F;
        public float startZ = 1.3F;
        public bool isDrag = false;
        public int touchType = 0; //手势类型 1.旋转 2.缩放 3.移动

        private Touch beginTouch1; //开始触摸
        private Touch beginTouch2; //开始触摸
        public int supplyTouchType = 0; //0.所有手势 1.只支持单指旋转 2.只支持双指缩放 3.只支持双指移动
        public float touchDistance = 0;
        public float touchDistanceForReset = 0;
        public Vector2 startPos;
        public Vector2 centerPos;
        public Vector3 scaleRotation;
        public Camera mainCamera;
        public Boolean isTouchCenter;
        public Vector3 touchRotationCenter;
        private ClickEvent clickEvent;
        private ShowMark showMark;

        private Transform targetObj = null;
        private Vector3 mouseModelOffset = Vector3.zero;

        private void Awake()
        {
            clickEvent = GameObject.Find("Camera").GetComponent<ClickEvent>();
        }

        void Update()
        {
            //没有触摸
            if (Input.touchCount <= 0)
            {
                isDrag = false;
                isTouchCenter = false;
                return;
            }

            //如果动画执行中
            if (ClickEvent.isMoveAnima)
            {
                return;
            }

            //clickEvent.beforeOpenPosition = transform.position;
            //clickEvent.beforeOpenRotation = transform.rotation.eulerAngles;
            //图谱
            if (clickEvent.showType == 6)
            {
            }
            else if (clickEvent.showType == 0)
            {
                if (ClickEvent.isTouchSplit)
                {
                    return;
                }
            }

            //clickEvent.cameraMoveType = 0;
            /*       //单点触摸， 水平上下移动
                   if (Input.touchCount ==1&& Input.GetTouch(0).phase == TouchPhase.Moved)
                   {
                       var deltaposition = Input.GetTouch(0).deltaPosition;
                       transform.Translate(-deltaposition.x * 0.1f, 0f, -deltaposition.y * 0.1f);
                   }*/
            //单点触摸， 水平上下旋转
            if (1 == Input.touchCount)
            {
                if (supplyTouchType != 0 && supplyTouchType != 1)
                {
                    return;
                }

                if (touchType == 0)
                {
                    touchType = 1;
                    touchDistance = 0;
                }

                /*  if (Input.GetTouch(0).phase == TouchPhase.Moved)
                  {*/
                Touch touch = Input.GetTouch(0);
                Vector2 deltaPos = touch.deltaPosition;
                if (touch.phase == TouchPhase.Began)
                {
                    startPos = touch.position;
                    if (!clickEvent.isClickCenter)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(startPos);
                        if (isTouchSplit)
                        {
                            if (targetObj == null)
                            {
                                if (Physics.Raycast(ray, out RaycastHit rayHitInfo)) //使用默认射线长度和其他默认参数
                                {
                                    //Debug.Log("Hit--" + rayHitInfo.collider.gameObject.name);//碰撞到的物体的名称
                                    targetObj = rayHitInfo.transform;
                                    Vector3 mouseStart2DPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                                        Camera.main.WorldToScreenPoint(targetObj.position).z);
                                    mouseModelOffset = targetObj.position -
                                                       Camera.main.ScreenToWorldPoint(mouseStart2DPos);
                                }
                            }
                            else
                            {
                                Vector3 mouse2DPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                                    Camera.main.WorldToScreenPoint(targetObj.position).z);
                                var mouse3DPos = Camera.main.ScreenToWorldPoint(mouse2DPos);
                                targetObj.position = mouse3DPos + mouseModelOffset;
                            }
                        }
                        else
                        {
                            if (Physics.Raycast(ray, out RaycastHit raycastHit))
                            {
                                //Debug.Log("Raycast click");
                                if (clickEvent.showType == 7)
                                {
                                    isTouchCenter = false;
                                    //touchRotationCenter = raycastHit.transform.GetComponent<Renderer>().bounds.center;
                                    ClickBoneMark(raycastHit);
                                }
                                else
                                {
                                    isTouchCenter = true;
                                    touchRotationCenter = raycastHit.point;
                                    ExitBoneMark();
                                }
                            }
                            /*      Vector3 screenPosition = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));

                                  Vector3 vc = mainCamera.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, screenPosition.z));
                                  vc.z = 0;
                                  ClickEvent.RotationCenter = vc;*/
                        }
                    }
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    isDrag = true;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    float touchDistance = Vector2.Distance(startPos, touch.position);
                    if (touchDistance > 6)
                    {
                        isDrag = true;
                    }

                    if (!object.ReferenceEquals(targetObj, null))
                    {
                        targetObj = null;
                        return;
                    }
                }

                if (clickEvent.isResetStatus)
                {
                    touchDistanceForReset += Math.Abs(deltaPos.x + Math.Abs(deltaPos.y));
                }

                //transform.RotateAround(ClickEvent.RotationCenter, new Vector3(1, 0, 0), deltaPos.y/2);
                // transform.RotateAround(ClickEvent.RotationCenter, transform.right, deltaPos.y/2);
                // transform.RotateAround(ClickEvent.RotationCenter, new Vector3(0, ClickEvent.RotationCenter.y, 0), deltaPos.x/2);
                // transform.RotateAround(ClickEvent.RotationCenter, transform.up, -deltaPos.x/2);
                Vector3 rotationCenter;
                if (isTouchCenter)
                {
                    rotationCenter = touchRotationCenter;
                }
                else
                {
                    rotationCenter = ClickEvent.RotationCenter;
                }

                if (transform.up.y > 0)
                {
                    transform.RotateAround(rotationCenter, new Vector3(0, 1, 0), deltaPos.x / 3);
                }
                else
                {
                    transform.RotateAround(rotationCenter, new Vector3(0, 1, 0), -deltaPos.x / 3);
                }

                //Vector3(1, 0, 0)
                transform.RotateAround(rotationCenter, transform.right, -deltaPos.y / 3);
                //  transform.rotation = Quaternion.Euler(new Vector3(t ransform.rotation.eulerAngles.x + deltaPos.x / 2, transform.rotation.eulerAngles.y + deltaPos.y / 2, transform.rotation.eulerAngles.z));
                //   }
                /*     else {
                         ClickEvent clickEvent = GameObject.Find("Camera").GetComponent<ClickEvent>();
                         clickEvent.GetCenter();

                     }*/
                if (supplyTouchType == 1)
                {
                    touchDistance += Math.Abs(deltaPos.x + Math.Abs(deltaPos.y));
                }

                if (touchDistance >= 400)
                {
                    //clickEvent.isResetStatus = false;

#if UNITY_IOS
                iOSPlugin.ReturnTouchType(touchType + "");
#endif
#if UNITY_WEBGL
                JSPlugin.ReturnTouchType(touchType + "");
#endif
#if UNITY_ANDROID
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
                bool success = _ajc.Call<bool>(
                    "returnTouchType",
                    new
                    object[] { touchType + "" });
#else

#endif
                    touchDistance = 0;
                    touchType = 0;
                }

                if (clickEvent.isResetStatus && touchDistanceForReset >= 30)
                {
                    clickEvent.isResetStatus = false;
                    touchDistanceForReset = 0;
                }

                return;
            }

            isDrag = true;
            //多点触摸, 放大缩小
            Touch newTouch1 = Input.GetTouch(0);
            Touch newTouch2 = Input.GetTouch(1);

            //第2点刚开始接触屏幕, 只记录，不做处理
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                beginTouch2 = newTouch2;
                beginTouch1 = newTouch1;
                centerPos =
                    new Vector2(newTouch1.position.x + (newTouch2.position.x - newTouch1.position.x) / 2,
                        newTouch1.position.y + (newTouch2.position.y - newTouch1.position.y) / 2);

                return;
            }

            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

            //两个距离之差，为正表示放大手势， 为负表示缩小手势
            float offset = newDistance - oldDistance;

            if (offset <= 10 && offset >= -10)
            {
                float x = 0F;
                float y = 0F;

                touchType = 3;


                x = -newTouch1.deltaPosition.x / 14000f * getDistance();

                y = -newTouch1.deltaPosition.y / 14000f * getDistance();

                if (supplyTouchType == 0 || supplyTouchType == 3)
                {
                    transform.Translate(transform.right * x, Space.World);
                    transform.Translate(transform.up * y, Space.World);
                }

                if (clickEvent.isResetStatus)
                {
                    touchDistanceForReset += Math.Abs(
                        newTouch1.deltaPosition.x + Math.Abs(newTouch1.deltaPosition.y) + Math.Abs(
                            newTouch2.deltaPosition.x
                        ) + Math.Abs(newTouch2.deltaPosition.y)
                    );
                }

                if (supplyTouchType == 3)
                {
                    touchDistance += Math.Abs(
                        newTouch1.deltaPosition.x + Math.Abs(newTouch1.deltaPosition.y) + Math.Abs(
                            newTouch2.deltaPosition.x
                        ) + Math.Abs(newTouch2.deltaPosition.y)
                    );
                }
            }
            else
            {
                //放大因子， 一个像素按 0.01倍来算(100可调整)
                /* if (IsEnlarge(oldTouch1.position, oldTouch2.position, newTouch1.position, newTouch2.position))
                     {

                         if (distance >= 0.5F)
                         {
                             distance -= 0.1F;
                         }
                     }
                     else
                     {

                         if (distance < 5F)
                         {
                             distance += 0.1F;
                         }
                     }*/

                distance = offset / 500F * getDistance();

                /*            if (distance > 5)
                            {
                                distance = 5F;
                            }*/

                // ShowToast(getDistance() + "");
                // Quaternion rotation = Quaternion.Euler(transform.position.y, transform.position.x, 0);

                // float newPositionz = Mathf.SmoothDamp(transform.position.z,0, ref 0, 0.8F);
                Vector3 tmp = new Vector3(0, 0, -1 * (distance));
                /*    if (offset > 0)
                    {*/
                float halfFOV = (this.mainCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
                float aspect = this.mainCamera.aspect;
                float scaleH = distance * Mathf.Tan(halfFOV) * 2;
                float scaleW = scaleH * aspect;

                float cpRateX = centerPos.x / Screen.width - 0.5f;
                float cpRateY = centerPos.y / Screen.height - 0.5f;
                Vector3 pos = transform.localPosition;
                pos += transform.right * (scaleW * cpRateX);
                pos += transform.up * (scaleH * cpRateY);
                pos += transform.forward * distance;

                /*     Vector3 position = transform.position */ /*- transform.rotation * tmp */
                /*- Quaternion.Euler(threeDCenterPos - transform.position) * tmp;
                Vector3 lookPosition = cameraCenterPos + distance / distanceForCamera*(Vector3.Distance(threeDCenterPos,cameraCenterPos))  * (threeDCenterPos - cameraCenterPos).normalized;
              */
                if (supplyTouchType == 0 || supplyTouchType == 2)
                {
                    if (getDistance() <= 3 || offset > 0 || clickEvent.showType != 0)
                    {
                        //  transform.position = position;

                        // transform.LookAt(lookPosition);

                        this.transform.localPosition = pos;
                    }
                }

                touchType = 2;
                if (clickEvent.isResetStatus)
                {
                    touchDistanceForReset += Math.Abs(
                        newTouch1.deltaPosition.x + Math.Abs(newTouch1.deltaPosition.y) + Math.Abs(
                            newTouch2.deltaPosition.x
                        ) + Math.Abs(newTouch2.deltaPosition.y)
                    );
                }

                if (supplyTouchType == 2)
                {
                    touchDistance += Math.Abs(
                        newTouch1.deltaPosition.x + Math.Abs(newTouch1.deltaPosition.y) + Math.Abs(
                            newTouch2.deltaPosition.x
                        ) + Math.Abs(newTouch2.deltaPosition.y)
                    );
                }
            }

            if (touchDistance >= 800)
            {
                // clickEvent.isResetStatus = false;

#if UNITY_IOS
            iOSPlugin.ReturnTouchType(touchType + "");
#endif
#if UNITY_WEBGL
            JSPlugin.ReturnTouchType(touchType + "");
#endif
#if UNITY_ANDROID
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
            bool success = _ajc.Call<bool>("returnTouchType", new object[] { touchType + "" });
#endif
                touchDistance = 0;
                touchType = 0;
            }

            //记住最新的触摸点，下次使用
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;

            if (clickEvent.isResetStatus && touchDistanceForReset >= 30)
            {
                clickEvent.isResetStatus = false;
                touchDistanceForReset = 0;
            }
        }

        private void ClickBoneMark(RaycastHit raycastHit)
        {
            if (showMark == null)
            {
                showMark = GameObject.Find("Camera").GetComponent<ShowMark>();
            }

            if (showMark.boneMarkMaterialObjs != null)
            {
                //处理骨标点击
                Material material = raycastHit.transform.GetComponent<Renderer>().material;

                if (material.shader.name.Equals("ame3"))
                {
                    foreach (KeyValuePair<string, Material> item in showMark.boneMarkMaterialObjs)
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

                    Vector2 pixelUV = raycastHit.textureCoord;

                    Texture2D tex2 = material.GetTexture("_zzao") as Texture2D;

                    if (tex2 != null)
                    {
                        Vector2 uv = new Vector2(pixelUV.x, pixelUV.y);

                        // 将 UV 值转换为纹理坐标
                        int x = (int)(uv.x * tex2.width);
                        int y = (int)(uv.y * tex2.height);
                        // Y 坐标需要翻转，因为纹理坐标的原点在左上角
                        //int y = (int)((1.0f - uv.y) * tex2.height); // Y 坐标需要翻转，因为纹理坐标的原点在左上角
                        // 获取该坐标处的颜色
                        Color materialColor = tex2.GetPixel(x, y);
                        //string hexColor = $"#{ColorUtility.ToHtmlStringRGB(materialColor)}";
                        string hexColor =
                            $"{(int)(materialColor.r * 255)},{(int)(materialColor.g * 255)},{(int)(materialColor.b * 255)}";
                        if (hexColor.Equals("255,255,255"))
                        {
                            //Debug.Log("ClickBoneMark color white");
                        }
                        else
                        {
                            material.SetFloat("_uvx", pixelUV.x);
                            material.SetFloat("_uvy", pixelUV.y);

#if UNITY_IOS
                iOSPlugin.ReturnBoneMarkHex(hexColor);
#endif
#if UNITY_WEBGL
            JSPlugin.ReturnBoneMarkHex(hexColor);
#endif
#if UNITY_ANDROID
                        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
                        bool success = _ajc.Call<bool>("returnBoneMarkHex", new object[] { hexColor });
#endif
                        }
                    }
                }


                //Debug.Log($"Color at UV {uv}: {hexColor}");
            }
        }

        public void ExitBoneMark()
        {
            if (showMark != null)
            {
                showMark = null;
            }
        }

        public void SetSupplyTouchType(string type)
        {
            supplyTouchType = Convert.ToInt16(type);
            Debug.Log(supplyTouchType);
        }

        bool IsEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
        {
            float leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
            float leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));

            if (leng1 < leng2)
            {
                //放大手势
                return true;
            }
            else
            {
                //缩小手势
                return false;
            }
        }

        public float getDistance()
        {
            return transform.position.magnitude;
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
    }
}
