using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public struct ObjectToSpawn
{ 
    public Vector3 Position; 
    public int TypeObject; 
    public Quaternion Quaternion; 
    public EndPoint Endpoint;
    public ObjectToSpawn(Vector3 position, int typeObject, Quaternion quaternion, EndPoint endpoint) 
    { 
        Position = position; 
        TypeObject = typeObject; 
        Quaternion = quaternion; 
        Endpoint = endpoint;
    }
}
