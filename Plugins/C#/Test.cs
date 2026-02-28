using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class Test : MonoBehaviour
    {
        private Transform splitMoveTrans;

        void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 startPos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(startPos);
                if (Physics.Raycast(ray, out RaycastHit rayHitInfo))//使用默认射线长度和其他默认参数
                {
                    foreach (ObjectColorModel objInfo in ModelInteraction.lastObject)
                    {
                        if (objInfo.Obj.transform.name.Equals(rayHitInfo.transform.name))
                        {
                            splitMoveTrans = rayHitInfo.transform;
                            break;
                        }
                    }
                }

            }
            else if (Input.GetMouseButton(0))
            {
                if (splitMoveTrans)
                {
                    float distance = transform.position.magnitude;
                    Vector2 mousePoint = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    Debug.Log("mousePositionDelta = "+mousePoint);
                    float x = mousePoint.x / 14000f * distance;
                    float y = mousePoint.y / 14000f * distance;

                    splitMoveTrans.Translate(transform.right * x, Space.World);
                    splitMoveTrans.Translate(transform.up * y, Space.World);
                }

            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (splitMoveTrans) {
                    splitMoveTrans = null;
                }
            }



        }

    }
}
