using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public byte CurrentAnimation { get; set; }
    
    public byte IsShooting { get; set; }

    public Vector3 NetworkPosition { get; set; }

    public Vector3 EulerAngles { get; set; }
    
    

}
