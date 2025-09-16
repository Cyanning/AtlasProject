using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 使用方式
 ctrl ：将鼠标移动到起始点，按ctrl 标记一个点，再按数字键盘的enter 保存这个点
 shift ：将鼠标移动到止点，按shift 标记一个点，再按数字键盘的enter 保存这个点
 ASDW:  调整标记的视角方向
 删除键：移除最新的点
 回车键：保存当前的模型组合的所有标记点到文件

 Show Model ：是当前显示的模型组，在Camera 绑定的组件 TestMuscleMark下设置
 */
namespace Plugins.C_
{
    public class TestMuscleMark : MonoBehaviour
{
    //单独显示模型组
    public string showModel;
    private string objName;
    //public string value;

    public static Vector3 RotationCenter;
    //public GameObject lastMark;
    public Dictionary<string, GameObject> AllObject = new Dictionary<string, GameObject>(); //所有对象集合
    public List<GameObject> markList = new List<GameObject>();
    public List<string> clickMarkNameList = new List<string>();
    public List<string> dataList = new List<string>();


    public Button BtnReload;
    // Start is called before the first frame update
    void Start()
    {
        if (BtnReload != null)
        {
            BtnReload.onClick.AddListener(ReloadData);
        }
    }


    private void ReloadData() {
        if (showModel == null) {
            return;
        }

        GameObject camera = GameObject.Find("Camera");
        ClickEvent clickEvent = camera.GetComponent<ClickEvent>();


        foreach(KeyValuePair<String, GameObject> keyValue in AllObject) {
            //if (showModel.Contains(keyValue.Key))
            //{
            //    keyValue.Value.SetActive(true);
            //}
            //else {
            //    if (keyValue.Value.activeSelf == true) {
            keyValue.Value.SetActive(false);
            //    }
            //}
        }



        ShowMark showMark = camera.GetComponent<ShowMark>();
        if (showMark == null)
        {
            showMark = camera.AddComponent<ShowMark>();
        }
        showMark.ClearBoneMark();
        showMark.SyncAllObj(AllObject);
        showMark.LoadShowModelBoneMark(showModel);
        showMark.StartDownloadImg("4");

        clickEvent.GoCameraView(1.5f);

        markList.Clear();
        clickMarkNameList.Clear();
        dataList.Clear();
    }


    public void SyncModel(Dictionary<string, GameObject> AllObject) {
        this.AllObject = AllObject;
    }

    public void SetShowMusclePoint(string showModel) {
        this.showModel = showModel;
        string[] sArray = showModel.Split(';');
        foreach (string i in sArray)
        {
            GameObject obj = getObject(i);
            SetObjectStatus(obj, true);
        }
    }

    public void ShowMuscle(GameObject m_obj)
    {
        FindObjects(m_obj, false, true);
        Debug.Log($"model size {AllObject.Count}");

        string[] sArray = showModel.Split(';');
        foreach (string i in sArray)
        {
            GameObject obj = getObject(i);
            SetObjectStatus(obj, true);
        }

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
    public GameObject getObject(string name)
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
    void FindObjects(GameObject obj, bool state, bool isSaveObject)
    {
        if (obj != null)
        {


            //if (isSaveObject)
            //{
            //    string objName = obj.transform.name.Substring(obj.transform.name.LastIndexOf("~") + 1);
            //    if (!objName.Equals(bodyModelName)) {
            //        if (!AllObject.ContainsKey(objName))
            //        {

            //            AllObject.Add(objName, obj);
            //        }
            //    }

            //}
            if (obj.transform.childCount == 0)
            {
                SetObjectStatus(obj, state);

            }
            else
            {
                int i = 0;
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

                            if (!AllObject.ContainsKey(parent.name.Substring(parent.name.LastIndexOf("~") + 1)))
                            {
                                AllObject.Add(parent.name.Substring(parent.name.LastIndexOf("~") + 1), parent.gameObject);

                            }

                        }
                        SetObjectStatus(parent.gameObject, state);
                    }
                    i++;
                }
            }
        }


    }
    public void AddObjectClickEvent(GameObject itemObject)
    {
        var box = itemObject.GetComponent<MeshCollider>();
        if (box == null)
        {
            itemObject.AddComponent<MeshCollider>();
        }
        //box.convex = true;



        // Debug.Log("添加点击事件"+ entry);

    }
    public void SetObjectStatus(GameObject obj, bool state)
    {
        if (obj == null)
        {

            return;

        }
        if (state)
        {
            obj.SetActive(state);

            AddObjectClickEvent(obj);

        }
        else
        {
            obj.SetActive(state);
            // obj.GetComponent<Renderer>().enabled = state;

        }


    }

    Ray ray;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {



            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;//

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 vector = hit.point;//得到碰撞点的坐标
                Debug.Log(">>>>>>>>>>>X:" + vector.x);
                Debug.Log(">>>>>>>>>>>Y:" + vector.y);
                Debug.Log(">>>>>>>>>>>Z:" + vector.z);
                if (markList.Count > dataList.Count)
                {
                    UnityEditor.EditorUtility.DisplayDialog("请先保存上个针的数据", "请先保存上个针的数据", "确认", "取消");


                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("添加新针", "x:" + vector.x + "\n y:" + vector.y + "\n z:" + vector.z, "确认", "取消");
                    GameObject mark = (GameObject)Instantiate(Resources.Load("mark/zhongdian"));
                    mark.transform.position = vector;
                    mark.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                    GameObject camera = GameObject.Find("Camera");
                    //mark.transform.rotation = camera.transform.rotation ;
                    mark.transform.rotation = GetAnchorQuaterion(vector, camera.transform.position);
                    clickMarkNameList.Add(GetModelValue(hit.transform.name));
                    markList.Add(mark);

                }

            }

        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;//

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 vector = hit.point;//得到碰撞点的坐标
                Debug.Log(">>>>>>>>>>>X:" + vector.x);
                Debug.Log(">>>>>>>>>>>Y:" + vector.y);
                Debug.Log(">>>>>>>>>>>Z:" + vector.z);
                if (markList.Count > dataList.Count)
                {
                    UnityEditor.EditorUtility.DisplayDialog("请先保存上个针的数据", "请先保存上个针的数据", "确认", "取消");


                }
                else
                {
                    GameObject camera = GameObject.Find("Camera");
                    float distance = Vector3.Distance(vector,camera.transform.position);
                    //0.02321759
                    //0.3847193
                    Debug.Log($"distance = {distance}");
                    UnityEditor.EditorUtility.DisplayDialog("添加新针", "x:" + vector.x + "\n y:" + vector.y + "\n z:" + vector.z, "确认", "取消");
                    GameObject mark = (GameObject)Instantiate(Resources.Load("mark/qidian"));
                    mark.transform.position = vector;
                    mark.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);


                    mark.transform.rotation = GetAnchorQuaterion(vector,camera.transform.position);
                    clickMarkNameList.Add(GetModelValue(hit.transform.name));
                    //mark.transform.rotation = Quaternion.Euler(0, 180, 0);
                    markList.Add(mark);
                }
            }

        }



        if (Input.GetKeyDown(KeyCode.Backspace))
        {


            if (markList.Count > 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("已删除上一个", "", "确认", "取消");
                GameObject.Destroy(markList[markList.Count - 1]);
                if (markList.Count == dataList.Count)
                {
                    markList.RemoveAt(markList.Count - 1);
                    dataList.RemoveAt(dataList.Count - 1);
                    clickMarkNameList.RemoveAt(clickMarkNameList.Count - 1);

                }
                else
                {

                    markList.RemoveAt(markList.Count - 1);
                    clickMarkNameList.RemoveAt(clickMarkNameList.Count - 1);
                }

            }


        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {


            if (markList.Count > 0)
            {
                if (markList.Count <= dataList.Count)
                {
                    UnityEditor.EditorUtility.DisplayDialog("上一个已经保存过了", "上一个已经保存过了", "确认", "取消");

                }
                else
                {

                    UnityEditor.EditorUtility.DisplayDialog("已保存上一个", "保存成功！", "确认", "取消");
                    GameObject lastObj = markList[markList.Count - 1];
                    string type = "";

                    if (lastObj.transform.name.Equals("qidian(Clone)"))
                    {
                        type = "1";
                    }
                    else
                    {

                        type = "2";
                    }
                    objName = clickMarkNameList[clickMarkNameList.Count - 1];
                    string data = "{\"camera_position_x\":" + transform.position.x + ",\"camera_position_y\":" + transform.position.y + ",\"camera_position_z\":" + transform.position.z + ",\"camera_rotation_x\":" + transform.rotation.eulerAngles.x + ",\"camera_rotation_y\":" + transform.rotation.eulerAngles.y + ",\"camera_rotation_z\":" + transform.rotation.eulerAngles.z + ",\"mark_position_x\":" + lastObj.transform.position.x + ",\"mark_position_y\":" + lastObj.transform.position.y + ",\"mark_position_z\":" + lastObj.transform.position.z + ",\"mark_rotation_x\":" + lastObj.transform.rotation.eulerAngles.x + ",\"mark_rotation_y\":" + lastObj.transform.rotation.eulerAngles.y + ",\"mark_rotation_z\":" + lastObj.transform.rotation.eulerAngles.z + ",\"type\":" + type + ",\"bone_value\":\""+ objName + "\"}";
                    Debug.Log(data);
                    dataList.Add(data);
                }


            }


        }


        if (Input.GetKeyDown(KeyCode.Return))
        {


            if (markList.Count > 0)
            {
                string exportFilePath = $"{showModel}.txt";

                UnityEditor.EditorUtility.DisplayDialog($"导出数据到项目：anatomy_tools/export/{exportFilePath}", "", "确认", "取消");
                string strRes = "[";
                foreach (string s in dataList)
                {

                    strRes += s + ",";


                }

                if (strRes.EndsWith(","))
                {
                    strRes = strRes.Substring(0, strRes.Length - 1);


                }
                strRes = strRes + "]";
                string[] line = { strRes };
                Debug.Log(strRes);
                System.IO.File.WriteAllLines($@"export/{exportFilePath}", line);

            }


        }

        if (Input.GetKey(KeyCode.N)) {


            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;//

            if (Physics.Raycast(ray, out hit))
            {
                    Transform trans = hit.transform;//得到碰撞点的坐标

                    string name = trans.name.Substring( trans.name.LastIndexOf("~")+1);
                    UnityEditor.EditorUtility.DisplayDialog("得到模型value:" + name, "", "确认", "取消");
                    objName = name;

            }

        }

        if (Input.GetKey(KeyCode.W))
        {
            if (markList.Count > 0)
            {
                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.x = vector.x + 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }


        }
        if (Input.GetKey(KeyCode.S))
        {
            if (markList.Count > 0)
            {


                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.x = vector.x - 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }
        }
        if (Input.GetKey(KeyCode.A))
        {

            if (markList.Count > 0)
            {

                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.y = vector.y + 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (markList.Count > 0)
            {

                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.y = vector.y - 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (markList.Count > 0)
            {

                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.z = vector.z + 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (markList.Count > 0)
            {

                Vector3 vector = markList[markList.Count - 1].transform.eulerAngles;

                vector.z = vector.z - 1;
                markList[markList.Count - 1].transform.rotation = Quaternion.Euler(vector);
            }
        }
    }

    //获取锚点方向
    private Quaternion GetAnchorQuaterion(Vector3 pointStart,Vector3 pointEnd) {
        Vector3 direction =  pointEnd - pointStart;
        direction.Normalize();
        return Quaternion.LookRotation(direction, Vector3.up);
    }

    public static float minX = 0;
    public static float minY = 0;
    public static float minZ = 0;

    public static float maxX = 0;
    public static float maxY = 0;
    public static float maxZ = 0;
    /// <summary>
    /// 获得中心点
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public Vector3 GetCenter()
    {


        maxX = -999;
        maxY = -999;
        minX = 999;
        minY = 999;
        minZ = 999;
        maxZ = -999;

        foreach (KeyValuePair<string, GameObject> items in AllObject)
        {
            GameObject obj = items.Value;
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
        RotationCenter = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, minZ + (maxZ - minZ) / 2);

        Debug.Log("0XXXXX" + maxX);
        Debug.Log("0XXXXX" + maxY);
        Debug.Log("0XXXXX" + maxZ);
        Debug.Log("0XXXXX" + minX);
        Debug.Log("0XXXXX" + minY);
        Debug.Log("0XXXXX" + minZ);


        Debug.Log("4XXXXX" + RotationCenter);
        Debug.Log("0XXXXX" + RotationCenter.x);
        Debug.Log("0XXXXX" + RotationCenter.y);
        Debug.Log("0XXXXX" + RotationCenter.z);
        return RotationCenter;
    }

    private string GetModelValue(string modelName)
    {
        if (modelName.Contains("~"))
        {
            string modelValue = modelName.Substring(modelName.LastIndexOf("~") + 1).Trim();
            return modelValue;

        }
        else
        {
            return modelName.Trim();
        }

    }
}
}
