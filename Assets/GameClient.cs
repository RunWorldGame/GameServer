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
    public Queue<ObjectToSpawn> BufferSpawnPlayers;

    
    public List<NetworkObject> ObjectsToSync
    {
        get => GameNetworkInitializer.Instance.ObjectsToSync;
    }

    public List<NetworkObject> ActivePlayer;

    public Socket _clientSocket;
    private IPEndPoint _serverEndpoint;
    private byte[] _buffer;
    private EndPoint _senderRemote;
    private Vector3 curPos;

    private byte selectedCharacter = 0x2;


    private Dictionary<KeyCode, byte> _buttonClickToByte;

    [CanBeNull] public TransFormUpdater _transFormUpdater = null;


    private GameObjectInitializer _gameObjectInitializer;

    private string name = "Thomas";


    private bool isPlayerInitiatingReq = true;

    private bool readyToPlay = false;
    [HideInInspector]
    public TcpClient TcpClient;
    private void Awake()
    {
        BufferSpawnPlayers = new Queue<ObjectToSpawn>();
        ActivePlayer = new List<NetworkObject>();
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
        string serverIpAddress = "127.0.0.1";
        int port = 5000; 
        _senderRemote = new IPEndPoint(IPAddress.Any, 0);
        TcpClient = new TcpClient(serverIpAddress, port);
        Debug.Log("Connected to server.");
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
        
        sendJoinRequest(TcpClient.GetStream());
        
    }

    private void Update()
    {
        /*
        sendUserInput();
*/
       while (BufferSpawnElements.Count > 0)
       { 
           var x = BufferSpawnElements.Dequeue();
           var gameObj = _gameObjectInitializer.instantiateGameObjectMainThread(x);
           ObjectsToSync.Add(gameObj.GetComponent<NetworkObject>());
        }

       if (BufferSpawnPlayers.Count > 0)
       {
           GameObject gameObj = null;
           while (BufferSpawnPlayers.Count > 0)
           {
                Debug.Log("instantiating new player");
                var x = BufferSpawnPlayers.Dequeue();
                gameObj = _gameObjectInitializer.instantiateGameObjectMainThread(x);
                ActivePlayer.Add(gameObj.GetComponent<NetworkObjectPlayer>());
           }

          if (isPlayerInitiatingReq)
          { 
              Debug.Log("setting cam,era"); 
              isPlayerInitiatingReq = false; 
              CameraController.CameraControll.setObjectToFollow(gameObj.GetComponent<Transform>());
           }
       }

      
    }

    private void sendJoinRequest(NetworkStream networkStream)
    {
        List<byte> message = new List<byte>();
        message.Add(0x5);
        var bytesName = Encoding.UTF8.GetBytes(name);
        message.Add((byte) bytesName.Length);
        foreach (byte b in bytesName)
        {
            message.Add(b); 
        }
        message.Add(selectedCharacter);
        networkStream.Write(message.ToArray());
    }
      private void sendNewMessage(List<byte> listBytes)
      {
          _clientSocket.SendTo(listBytes.ToArray(), _serverEndpoint);
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
    
    long getPing(TcpClient tcpClient)
    {
        var watch = Stopwatch.StartNew();
        var message = "hallo";
        byte[] data = Encoding.ASCII.GetBytes(message);
        List<byte> messageBytes = new List<byte>();
        messageBytes.Add(0x99);
        foreach (byte b in data)
        {
           messageBytes.Add(b); 
        }
        byte[] buffer = new byte[1024];
        tcpClient.GetStream().Write(messageBytes.ToArray());
        tcpClient.GetStream().Read(buffer);
        Debug.Log("ending");
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
