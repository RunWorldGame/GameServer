using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using DefaultNamespace.Scenes;
using JetBrains.Annotations;
using Unity.VisualScripting;
[RequireComponent(typeof(GameObjectInitializer))]
public class GameClient : MonoBehaviour
{
    public Queue<ObjectToSpawn> BufferSpawnElements;

    
    public List<NetworkObject> ObjectsToSync
    {
        get => GameNetworkInitializer.Instance.ObjectsToSync;
    }

    public Socket _clientSocket;
    private IPEndPoint _serverEndpoint;
    private byte[] _buffer;
    private EndPoint _senderRemote;
    private Vector3 curPos;


    private Dictionary<KeyCode, byte> _buttonClickToByte;

    [CanBeNull] public TransFormUpdater _transFormUpdater = null;


    private GameObjectInitializer _gameObjectInitializer;


    private bool isPlayerInitiatingReq = true;

    private void Awake()
    {
        _buttonClickToByte = new Dictionary<KeyCode, byte>();
        _buttonClickToByte.Add(KeyCode.Mouse1, 0x1);
        _buttonClickToByte.Add(KeyCode.Mouse2, 0x2);
        _buttonClickToByte.Add(KeyCode.W, 0x3);
        _buttonClickToByte.Add(KeyCode.A, 0x4);
        _buttonClickToByte.Add(KeyCode.S, 0x5);
        _buttonClickToByte.Add(KeyCode.D, 0x6);
        BufferSpawnElements = new Queue<ObjectToSpawn>();
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11111);
        
        _buffer = new byte[1024];
        
        _senderRemote = new IPEndPoint(IPAddress.Any, 0);
        
        
        
        var ping = getPing(_clientSocket, _serverEndpoint);
        Debug.Log("made ping");
       
    }

    private void Start()
    {
        _gameObjectInitializer = GetComponent<GameObjectInitializer>();
        Debug.Log("start called");
        Task.Run(() =>
        {
            Debug.Log("Task running");
            _transFormUpdater = new TransFormUpdater( this);
            _transFormUpdater.StartListeningForNewPositions();
            Debug.Log("not null anymore");
        });
        sendJoinRequest();
    }

    private void Update()
    {
        sendUserInput();
       //Debug.Log("new position"); 
       if (Input.GetKeyDown(KeyCode.Space))
       {
           //Debug.Log("space pressed");
           //_clientSocket.SendTo(new byte[] { 0 }, _serverEndpoint);
           //Debug.Log("send message");
       }
       while (BufferSpawnElements.Count > 0)
       { 
           Debug.Log("should in 1");
           var x = BufferSpawnElements.Dequeue();
           Debug.Log(x);
           var gameObj = _gameObjectInitializer.instantiateGameObjectMainThread(x);
           if (isPlayerInitiatingReq)
           {
               Debug.Log("setting cam,era");
                isPlayerInitiatingReq = false;
                CameraController.CameraControll.setObjectToFollow(gameObj.GetComponent<Transform>());
           }
           ObjectsToSync.Add(gameObj.GetComponent<NetworkObject>());
           Debug.Log("added a new gameobject");
        }
    }

    private void sendJoinRequest()
    {
        
        List<byte> message = new List<byte>();
        message.Add(5);
        string name = "Thomas"; 
        var bytesName = Encoding.UTF8.GetBytes(name);
        message.Add((byte) bytesName.Length);
        foreach (byte b in bytesName)
        {
            message.Add(b); 
        }
        sendNewMessage(message);
    }


    private void sendUserInput()
    {
        List<byte> message = new List<byte>();
        List<byte> tempMessage = new List<byte>();
        message.Add(0x3);
        bool sthClicked = false;
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            // Check if the key is pressed down
            if (Input.GetKeyDown(keyCode) && _buttonClickToByte.ContainsKey(keyCode))
            {
                sthClicked = true;
                tempMessage.Add(_buttonClickToByte[keyCode]);
                // Log the pressed key
                Debug.Log("Key pressed: " + keyCode);
            }
        }
        
        if (!sthClicked) return;
        message.Add((byte)tempMessage.Count);
        foreach (byte t in tempMessage)
        {
            message.Add(t);
        }
        Debug.Log("send message");
        sendNewMessage(message);
    }
    
    private void sendNewMessage(List<byte> listBytes)
    {
        byte[] delimeter = new byte[] { 0x12, 0x98, 0x90, 0x23,0x24,0x25 };
        foreach (byte b in delimeter)
        {
           listBytes.Add(b); 
        }
        //Debug.Log("sent message");
        _clientSocket.SendTo(listBytes.ToArray(), _serverEndpoint);
        
        
    }


    private void FixedUpdate()
    {
    }

    private void Listen()
    {

            //int received = _clientSocket.ReceiveFrom(_buffer, ref _senderRemote);
            
            //routeMessage();
           /* 
            //Debug.Log("got something");
            string data = Encoding.ASCII.GetString(_buffer, 0, received);
            //Debug.Log(data);
            var arr = data.Split(";");
            //Debug.Log(arr);
            float x = float.Parse(arr[0]);
            float y = float.Parse(arr[1]);
            float z = float.Parse(arr[2]);
            //Debug.Log("z");
            curPos = new Vector3(x, y, z);
            //Debug.Log("new postiion");
            
        */
    }
    
     
    
    long getPing(Socket socket, EndPoint endPoint)
    {
        var watch = Stopwatch.StartNew();
        var message = "hallo";
        byte[] data = Encoding.ASCII.GetBytes(message);
    
        socket.SendTo(data, endPoint);


        return 0;
    }
    void OnDestroy()
    {
        _clientSocket.Close();
        _clientSocket.Dispose();
    }
}

public enum ButtonEnum : byte
{
    Mouse1 = 1,
    Mouse2 = 2,
    Mouse3 = 3
    // Add more enum values as needed
}
