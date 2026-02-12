using System.Runtime.Serialization;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

//[DataContract]
public class PointInfo
{

    public string did { get; set; }
    //[DataMember(Name = "camera_position_x")]
    public GameObject panelObject { get; set; }
    public Button hideObject { get; set; }
    public Button infoObject { get; set; }
    public Text textObject { get; set; }
    public bool isHide { get; set; }

    [DataMember(Name = "content")]
    public string content { get; set; }
    [DataMember(Name = "isLeft")]
    public bool isLeft { get; set; }
    [DataMember(Name = "camera_position_x")]
    public string PositionX { get; set; }
    [DataMember(Name = "camera_position_y")]
    public string PositionY { get; set; }
    [DataMember(Name = "camera_position_z")]
    public string PositionZ { get; set; }
}

