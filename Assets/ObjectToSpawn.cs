using System.Collections;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;

public struct ObjectToSpawn
{ 
    public Vector3 Position; 
    public int TypeObject; 
    public Quaternion Quaternion; 
    [CanBeNull] public EndPoint Endpoint;
    public ObjectToSpawn(Vector3 position, int typeObject, Quaternion quaternion, EndPoint endpoint) 
    { 
        Position = position; 
        TypeObject = typeObject; 
        Quaternion = quaternion; 
        Endpoint = endpoint;
    }
    public ObjectToSpawn(Vector3 position, int typeObject, Quaternion quaternion) 
    { 
        Position = position; 
        TypeObject = typeObject; 
        Quaternion = quaternion;
        Endpoint = null;
    }
}
