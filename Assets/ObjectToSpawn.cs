using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;
using UnityEngine;

public struct ObjectToSpawn
{ 
    public Vector3 Position; 
    public byte TypeObject; 
    public Quaternion Quaternion; 
    [CanBeNull] public TcpClient TcpClient;
    public ObjectToSpawn(Vector3 position, byte typeObject, Quaternion quaternion, TcpClient tcpClient) 
    { 
        Position = position; 
        TypeObject = typeObject; 
        Quaternion = quaternion; 
        TcpClient = tcpClient;
    }
    public ObjectToSpawn(Vector3 position, byte typeObject, Quaternion quaternion) 
    { 
        Position = position; 
        TypeObject = typeObject; 
        Quaternion = quaternion;
        TcpClient = null;
    }
}
