using UnityEngine;
using UnityEngine.EventSystems;
using System;


namespace Plugins.C_
{
    public class ClickWhiteArea : MonoBehaviour
    {
        private long LastTouchTime = 0;

        // Use this for initialization
        void Start()
        {
        }


        void Update()
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        LastTouchTime = 0;
                    }
                    else
                    {
                        LastTouchTime = GetTimeStamp();
                    }
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        LastTouchTime = 0;
                    }
                    else
                    {
                        if (GetTimeStamp() - LastTouchTime <= 200)
                        {
                            LastTouchTime = 0;
#if UNITY_IOS
              iOSPlugin.ClickWhiteArea("");
#elif UNITY_WEBGL
              JSPlugin.ClickWhiteArea("");

#else
                            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
                            // AndroidJavaObject _ajc = new AndroidJavaObject("com.wazitan.anatomy.activity.UnityPlayerActivity");
                            bool success = _ajc.Call<bool>("clickWhiteArea", new object[] { "" });
#endif
                        }
                        else
                        {
                            LastTouchTime = 0;
                        }
                    }
                }
            }

            else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                // Debug.Log("clickWhiteArea");
//#if UNITY_IOS
//             iOSPlugin.ClickWhiteArea("");
//#elif UNITY_WEBGL
//              JSPlugin.ClickWhiteArea("");

//#elif UNITY_ANDROID
//            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//            AndroidJavaObject _ajc = jc.GetStatic<AndroidJavaObject>("currentActivity");
//            // AndroidJavaObject _ajc = new AndroidJavaObject("com.wazitan.anatomy.activity.UnityPlayerActivity");
//            bool success = _ajc.Call<bool>("clickWhiteArea", new object[] { "" });
//#endif
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
    }
}
