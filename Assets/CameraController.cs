using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController CameraControll { get; set; }
    // Start is called before the first frame update
    [SerializeField] private CinemachineVirtualCamera _cinemachineBrain;
    private void Awake()
    {
        CameraControll = this;
    }

    public void setObjectToFollow(Transform gameObject)
    {
        _cinemachineBrain.Follow = gameObject;
    }
    
}
