using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class GameServer : MonoBehaviour
{
    public List<NetworkObject> ObjectsToSync
    {
        get => GameNetworkInitializer.Instance.ObjectsToSync;
    }
    [SerializeField] private List<Vector3> _playerSpawnPoints;
    private Queue<ObjectToSpawn> _bufferSpawnElements; 
    private Queue<ObjectToSpawn> _bufferPlayerToSpawns; 

    private Dictionary<TcpClient, User> _playerUserObjDic;
    private Dictionary<TcpClient, NetworkObjectPlayer> _playerNetworkObjectDic;

    private Dictionary<TcpClient, NetworkStream> _playerStreamDic;
    
    private EndPoint _senderRemote; 
    
    private Socket _socket;
    private Socket _socket2;

    private byte[] _buffer;

    private Dictionary<byte, KeyCode> _buttonClickToByte;

    private GameObject _tempGameObject;

    private int counterObjectsToSyncAtStart;

    private bool gotNewUser = false;

    private bool addNewUserMessage = false;
    

    private GameObjectInitializer _gameObjectInitializer;
    private void Awake()
    {
        _bufferPlayerToSpawns = new Queue<ObjectToSpawn>();
        _buttonClickToByte = new Dictionary<byte, KeyCode>();
        _buttonClickToByte.Add( 0x1, KeyCode.Mouse1); 
        _buttonClickToByte.Add( 0x2, KeyCode.Mouse2); 
        _buttonClickToByte.Add( 0x3, KeyCode.W); 
        _buttonClickToByte.Add( 0x4, KeyCode.A); 
        _buttonClickToByte.Add( 0x5, KeyCode.S); 
        _buttonClickToByte.Add( 0x6, KeyCode.D);
        
        _bufferSpawnElements = new Queue<ObjectToSpawn>();
        _playerNetworkObjectDic = new Dictionary<TcpClient, NetworkObjectPlayer>();
        _senderRemote = new IPEndPoint(IPAddress.Any, 11111);
        _playerUserObjDic = new Dictionary<TcpClient, User>();
        _playerStreamDic = new Dictionary<TcpClient, NetworkStream>();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _buffer = new byte[1024];
        
        _socket.Bind(_senderRemote);
        
        Task.Run(() => Listen());
    }

    private void Start()
    {
        counterObjectsToSyncAtStart = ObjectsToSync.Count;
        _gameObjectInitializer = GetComponent<GameObjectInitializer>();
    }

    private void Update()
    {
        while (_bufferSpawnElements.Count > 0)
        {
            var gameObject = _gameObjectInitializer.instantiateGameObjectMainThread(_bufferSpawnElements.Dequeue());
            ObjectsToSync.Add(gameObject.GetComponent<NetworkObject>());
        } 
        while (_bufferPlayerToSpawns.Count > 0)
        {
            gotNewUser = true;
            Debug.Log("new player gets instantiated");
            var x = _bufferPlayerToSpawns.Dequeue();
            var gameObject = _gameObjectInitializer.instantiateGameObjectMainThread(x); 
            _playerNetworkObjectDic.Add(x.TcpClient, gameObject.GetComponent<NetworkObjectPlayer>());
        }

        
       // sendPositionCharacters();
    }

    private void Listen()
    {
        Debug.Log("listening for connections");
        int port = 5000;
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

        TcpListener server = new TcpListener(ipAddress, port);
        server.Start();
        Console.WriteLine("server lsiterning on port 5000");
        while (true)
        {
            Console.WriteLine("waiting for connection");

            TcpClient tcpClient = server.AcceptTcpClient();
            Thread clientthread = new Thread(HandleClient);
            clientthread.Start(tcpClient);
        }
        return;
    }
    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
         
        // Get a stream object for reading and writing data.
        NetworkStream stream = client.GetStream();
         
        _playerStreamDic.Add(client, stream);
        byte[] buffer = new byte[1024];
        int bytesRead;
         
        try
        {
            // Loop to receive all the data sent by the client.
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                List<byte> dataToSendBack = routeMessage(buffer, client);
                Debug.Log("sending bytes ");
                foreach (byte b in dataToSendBack)
                {
                   Debug.Log((int)b); 
                }
                stream.Write(dataToSendBack.ToArray());
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.ToString());
        }
        finally
        {
            // Shutdown and close the connection.
            client.Close();
        }
    }

    private void addAdditionalObjectsToSync(List<byte> message)
    {
        message.Add(0x8);
        var u = BitConverter.GetBytes(counterObjectsToSyncAtStart);
        message.Add(u[0]);
        message.Add(u[1]);
        message.Add(u[2]);
        message.Add(u[3]);
        for (int i = counterObjectsToSyncAtStart; i < ObjectsToSync.Count; i++)
        {
            var z = ObjectsToSync[i].GetComponent<Transform>();
            addVector3BytesToListByte(message, z.position);
            addVector3BytesToListByte(message, z.rotation.eulerAngles);
            message.Add(ObjectsToSync[i].TypeObject);
        }
    }

    private void getAllActivePlayer(List<byte> message)
    {
        message.Add(0x9);
        var u = BitConverter.GetBytes(_playerNetworkObjectDic.Count);
        message.Add(u[0]);
        message.Add(u[1]);
        message.Add(u[2]);
        message.Add(u[3]);
        foreach (var (key, value) in _playerNetworkObjectDic)
        {
            addVector3BytesToListByte(message, value.NetworkPosition);
            addVector3BytesToListByte(message, value.EulerAngles);
            message.Add(value.TypeObject);
        }
    }
    
    private void addVector3BytesToListByte(List<byte> bytesMessage, Vector3 vector3)
    {
        byte[] floatArr = new byte[4];
        floatArr = BitConverter.GetBytes(vector3.x);
        foreach (byte b in floatArr)
        {
           bytesMessage.Add(b); 
        }
        floatArr = BitConverter.GetBytes(vector3.y);
        foreach (byte b in floatArr)
        {
           bytesMessage.Add(b); 
        }
        floatArr = BitConverter.GetBytes(vector3.z);
        foreach (byte b in floatArr)
        {
           bytesMessage.Add(b); 
        }
    }


    private void sendPositionCharacters()
    {
        List<byte> message = new List<byte>();
        message.Add(0x6);
            
        foreach (var  value in _playerNetworkObjectDic)
        {
            addVector3BytesToListByte(message, value.Value.transform.position);
            addVector3BytesToListByte(message, value.Value.transform.rotation.eulerAngles);
            message.Add(0x1);
            message.Add(0x1);
        }
        foreach (var  value in ObjectsToSync)
        {
            addVector3BytesToListByte(message, value.transform.position);
            addVector3BytesToListByte(message, value.transform.rotation.eulerAngles);
            message.Add(0x1);
            message.Add(0x1);
        }
        //sendNewMessage(message);
    }

    private int AddAnotherMessageComingFrameAndLength(List<byte> message)
    {
        foreach (byte b in GameNetworkInitializer.Instance.anotherMessageComintFrame)
        {
           message.Add(b); 
        }

        int t = message.Count + 1 + 4;
        message.InsertRange(1,BitConverter.GetBytes(message.Count + 4) );
        return t;
    }
    
    private List<byte> routeMessage(byte[] message, TcpClient tcpClient)
    {
        List<byte> messageToSend = new List<byte>();
        int offset = 0;
        if (gotNewUser)
        {
            Debug.Log("got new user called");
            gotNewUser = false;
            getAllActivePlayer(messageToSend);
            offset = AddAnotherMessageComingFrameAndLength(messageToSend);
            
        }
        int currentIndex = 0;
        byte messageType = message[currentIndex];
        currentIndex++;
        if(messageType == 0x3)
        {
            Debug.Log("got new direction method");
            for (int i = 2; i < message[1] + 2; i++)
            {
                Debug.Log("key pressed "+ _buttonClickToByte[message[i]]);
            }

            var t = _playerUserObjDic[tcpClient];
            Debug.Log("user " + t.Name + " send a message");
            _playerNetworkObjectDic[tcpClient].WPushed = true;

        }
        else if (messageType == 0x5)
        {
             var nameLength = message[currentIndex];
             currentIndex++;
             Debug.Log("name lenghtj " + ((int)nameLength));
             byte[] nameBytes = new byte[(int) nameLength]; 
             Array.Copy(message, 2,nameBytes,0, nameLength);
                        
             string name = Encoding.UTF8.GetString(nameBytes);
             Debug.Log("name is " + name );
             
             Vector3 spawnPoint = _playerSpawnPoints[0];
            Debug.Log("second");
            currentIndex += nameLength;
            byte typeObject = message[currentIndex];
            ObjectToSpawn objectToSpawn = new ObjectToSpawn(spawnPoint, typeObject, new Quaternion(0,0,0,0), tcpClient); 
            Debug.Log("should so far");
            _playerUserObjDic.Add(tcpClient, new User(name)); 
            
            _bufferPlayerToSpawns.Enqueue(objectToSpawn);
             addAdditionalObjectsToSync(messageToSend);
        }
        else if (messageType == 0x99)
        {
            Debug.Log("got ping");
            messageToSend.Add(0x99);
        }
        else if(messageType == 0x50)
        {
            Debug.Log("send a messgae that client did not understand");
            
        }
        else
        {
            
        }
        GameNetworkInitializer.Instance.AddDelimeterFrame(messageToSend, offset);
        return messageToSend;
    }
}
