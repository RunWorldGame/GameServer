using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Vector3> _playerSpawnPoints;
    private Queue<ObjectToSpawn> _bufferSpawnElements; 
    private Queue<ObjectToSpawn> _bufferPlayerToSpawns; 

    public static float speed = 1f;
    private Dictionary<EndPoint, User> _playerUserObjDic;
    private Dictionary<EndPoint, NetworkObjectPlayer> _playerNetworkObjectDic;

    private EndPoint _senderRemote; 
    
    private Socket _socket;
    private Socket _socket2;

    private byte[] _buffer;

    private bool waited = false;
    
    
    private Dictionary<byte, KeyCode> _buttonClickToByte;

    private GameObject _tempGameObject;
    
    public GameObject PlayerPrefab
    {
        get => _playerPrefab;
    }


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
        _playerNetworkObjectDic = new Dictionary<EndPoint, NetworkObjectPlayer>();
        _senderRemote = new IPEndPoint(IPAddress.Any, 11111);
        _playerUserObjDic = new Dictionary<EndPoint, User>();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _buffer = new byte[1024];
        
        _socket.Bind(_senderRemote);
        
        Task.Run(() => Listen());
    }

    private void Start()
    {
        _gameObjectInitializer = GetComponent<GameObjectInitializer>();
    }

    private void Update()
    {
        
        while (_bufferSpawnElements.Count > 0)
        {
            Debug.Log("should in 1");
            var gameObject = _gameObjectInitializer.instantiateGameObjectMainThread(_bufferSpawnElements.Dequeue());
            ObjectsToSync.Add(gameObject.GetComponent<NetworkObject>());
        } 
        while (_bufferPlayerToSpawns.Count > 0)
        { 
            Debug.Log("new player gets instantiated");
            var x = _bufferPlayerToSpawns.Dequeue();
            var gameObject = _gameObjectInitializer.instantiateGameObjectMainThread(x); 
            ObjectsToSync.Add(gameObject.GetComponent<NetworkObjectPlayer>());
            _playerNetworkObjectDic.Add(x.Endpoint, gameObject.GetComponent<NetworkObjectPlayer>());
        }

        sendPositionCharacters();
        if (_bufferSpawnElements.Count > 0)
        {
            if (!waited)
            {
                Debug.Log("send pos character");
            }

            waited = true;
        }

    }

    private void Listen()
    {
        Debug.Log("listening for connections");
        _ = _socket.ReceiveFrom(_buffer, ref _senderRemote);
            if (_playerUserObjDic.ContainsKey(_senderRemote))
            {
                //Debug.Log("user already exists");
                if (_playerUserObjDic.TryGetValue(_senderRemote, out User user))
                {
                    user.lastSeen = DateTime.Now;
                }
            }
            
        routeMessage(_buffer, _senderRemote); 
        Listen();
    }
    
    

    private void spawnNewPlayer(ObjectToSpawn objectToSpawn, string name)
    {
        Debug.Log("spawn new player");
        List<byte> list = SendNewPlayerInitializationRequest(objectToSpawn.Position, 1, name);
        
        sendNewMessage(list);
        _bufferPlayerToSpawns.Enqueue(objectToSpawn);
    }

    private List<byte> SendNewPlayerInitializationRequest(Vector3 position, ushort typeObject, string name)
    {
        var bytesName = Encoding.UTF8.GetBytes(name);

        List<byte> bytesMessage = new List<byte>() { 1 };
        bytesMessage.Add((byte) bytesName.Length);
        foreach (byte b in bytesName)
        {
           bytesMessage.Add(b); 
        }

        addVector3BytesToListByte(bytesMessage, position);
        
        bytesMessage.Add((byte) typeObject);
        
        return bytesMessage;
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

    private void sendNewMessage(List<byte> listBytes)
    {
        foreach (var (key, value) in _playerUserObjDic) 
        {
            _socket2.SendTo(listBytes.ToArray(), key); 
        }
    }

    private void sendPositionCharacters()
    {
        List<byte> message = new List<byte>();
        message.Add(0x6);
        foreach (var  value in ObjectsToSync)
            
        {
            
            addVector3BytesToListByte(message, value.transform.position);
            addVector3BytesToListByte(message, value.transform.rotation.eulerAngles);
            message.Add(0x1);
            message.Add(0x1);
        }
        sendNewMessage(message);
    }
    
    
    

    private void routeMessage(byte[] message, EndPoint endPoint)
    {
        int currentIndex = 0;
        byte messageType = message[0];
        if (messageType == 0x1)
        {
            Debug.Log("is new player request");
            var nameLength = message[1];
            Debug.Log("name lenghtj " + ((int)nameLength));
            currentIndex = (int)nameLength + 2;
            byte[] nameBytes = new byte[(int) nameLength]; 
            Array.Copy(message, 2,nameBytes,0, nameLength);
            
            string name = Encoding.UTF8.GetString(nameBytes);
            Debug.Log("name is " + name );
            Vector3 spawnPos = Vector3.zero;
            byte[] floatA = new byte[4]; 
            Array.Copy(message, currentIndex,floatA,0, 4);
            spawnPos.x = BitConverter.ToSingle(floatA);
            currentIndex += 4; 
            Array.Copy(message, currentIndex,floatA,0, 4);
            spawnPos.y = BitConverter.ToSingle(floatA);

            currentIndex += 4; 
            Array.Copy(message, currentIndex,floatA,0, 4);
            spawnPos.z = BitConverter.ToSingle(floatA);
            
            currentIndex += 4; 
            
            Debug.Log("vector 3 is " + spawnPos);
            
        }
        else if(messageType == 0x3)
        {
            Debug.Log("got new direction method");
            for (int i = 2; i < message[1] + 2; i++)
            {
                Debug.Log("key pressed "+ _buttonClickToByte[message[i]]);
            }

            var t = _playerUserObjDic[endPoint];
            Debug.Log("user " + t.Name + " send a message");
            _playerNetworkObjectDic[endPoint].WPushed = true;

        }
        else if (messageType == 0x5)
        {
             var nameLength = message[1];
             Debug.Log("name lenghtj " + ((int)nameLength));
             byte[] nameBytes = new byte[(int) nameLength]; 
             Array.Copy(message, 2,nameBytes,0, nameLength);
                        
             string name = Encoding.UTF8.GetString(nameBytes);
             Debug.Log("name is " + name );
             
             Vector3 spawnPoint = _playerSpawnPoints[0];
               // new Vector3()
               // Vector3 x = new Vector3(spawnPoint.position.x, spawnPoint.position.y, spawnPoint.position.z);
            Debug.Log("second"); 
            ObjectToSpawn objectToSpawn = new ObjectToSpawn(spawnPoint, 1, new Quaternion(0,0,0,0), _senderRemote); 
            Debug.Log("should so far");
                
            _playerUserObjDic.Add(_senderRemote, new User(name)); 
            spawnNewPlayer(objectToSpawn, name);
            
        }
        else if (messageType == 0x6)
        {
            Vector3 pos = Vector3.zero;
            Vector3 rot = Vector3.zero;
            byte currentAnimation;
            byte isShooting;
            Debug.Log("message type new Position");
            int currentIndexList = 1;
            foreach (var  value in ObjectsToSync)
            {
                pos = subtractVector3FromByteArray(message, currentIndexList);
                currentIndexList += 12;
                rot = subtractVector3FromByteArray(message, currentIndexList); 
                currentIndexList += 12;
                currentAnimation = message[currentIndexList++];
                isShooting = message[currentIndexList++];
                if (value.TryGetComponent(out NetworkObject networkObject))
                {
                    networkObject.NetworkPosition = pos;
                    networkObject.EulerAngles = rot;
                    networkObject.CurrentAnimation = currentAnimation;
                    networkObject.IsShooting = isShooting;
                }
                Debug.Log("vector is " + pos);
                Debug.Log("rotation is " + rot);
                Debug.Log("isCurrnetanimation " + ((int) currentAnimation) );
                Debug.Log("rotation is " + ((int) isShooting));
            }
        }
        else
        {
            Debug.Log("soemtzhing wrong");
        }
    }

    private Vector3 subtractVector3FromByteArray(byte[] arr, int index)
    {
        Vector3 vector3 = Vector3.zero;
        vector3.x = BitConverter.ToSingle(arr, index);
        index += 4;
        vector3.y = BitConverter.ToSingle(arr, index);
        index += 4;
        vector3.z = BitConverter.ToSingle(arr, index);
        index += 4;
        return vector3;
    }
    
}
