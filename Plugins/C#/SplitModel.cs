using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class SplitModel : MonoBehaviour
    {
        private Transform splitMoveTrans;
        private Vector3 offset;
        // Start is called before the first frame update
        void Start()
        {

        }

        void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 startPos = Input.mousePosition;

                foreach (ObjectColorModel objInfo in ModelInteraction.lastObject)
                {
                    if (objInfo.Obj.transform.name.Equals(transform.name))
                    {
                        Debug.Log("Hit--" + transform.name);//碰撞到的物体的名称
                        Vector3 mouseStart2DPos = new Vector3(startPos.x, startPos.y, transform.position.z);
                        Vector3 mouseModelOffset = transform.position - Camera.main.ScreenToWorldPoint(mouseStart2DPos);
                        splitMoveTrans = transform;
                        Debug.Log("model position = --" + mouseModelOffset);
                        offset = mouseModelOffset;
                        break;
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (splitMoveTrans)
                {
                    Vector2 startPos = Input.mousePosition;
                    Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, transform.position.z)) + offset;
                    //Vector3 movePosition = new Vector3(newPosition.x, newPosition.y, 1);
                    transform.position = new Vector3(-newPosition.x, -newPosition.y, -0.13F); // 保持z轴不变，仅平移x和y轴

                    Debug.Log("click move " + newPosition);
                }

            }
            else if (Input.GetMouseButtonUp(0))
            {

                if (splitMoveTrans != null)
                {
                    Debug.Log("click end");
                    splitMoveTrans = null;
                }
            }

        }
    }
}
