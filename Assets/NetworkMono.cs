using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMono : MonoBehaviour
{
    protected NetworkObjectPlayer networkObjectPlayer;

    protected NetworkObject networkObject;
    
    protected bool isServerObj = true;


    protected void Awake()
    {
        if (TryGetComponent(out NetworkObjectPlayer networkObjectPlayer2))
        {
            networkObjectPlayer = networkObjectPlayer2;
        }
        else
        {
            networkObject = GetComponent<NetworkObject>();
        }
            
    }

    protected void UpdateIfNotNetworkObject()
    { 
        transform.position = networkObjectPlayer.NetworkPosition; 
        transform.rotation = Quaternion.Euler(networkObjectPlayer.EulerAngles.x, networkObjectPlayer.EulerAngles.y, networkObjectPlayer.EulerAngles.z);
    }
    
    public virtual void Start()
    {
         networkObjectPlayer = GetComponent<NetworkObjectPlayer>();
         if (!GameNetworkInitializer.Instance.IsServer) 
         { 
             isServerObj = false;
             return;
         }
    }

    public virtual void Update()
    {
         if (!isServerObj)
         { 
             UpdateIfNotNetworkObject(); 
             return;
         }
         
    }
}
