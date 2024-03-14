using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace.Scenes
{
    public class TransFormUpdater
    {
        private Socket _clientSocket;
        
        private byte[] _buffer;
        
        private EndPoint _senderRemote;

        private GameObjectInitializer _gameObjectInitializer;

        private GameClient _gameClient;
        private int lengthFrame;

        public TransFormUpdater( GameClient gameClient)
        {
            _gameClient = gameClient;
            _senderRemote = new IPEndPoint(IPAddress.Any, 0);
            _clientSocket = gameClient._clientSocket;
            _buffer = new byte[1024];
            lengthFrame = GameNetworkInitializer.Instance.LengthFrames;
        }


        public void StartListeningForNewPositions()
        {
            Listen();
        }
        
        public void Listen()
        {
            NetworkStream stream = _gameClient.TcpClient.GetStream();
         
            int bytesRead;
            int justOnes = 0;
         
            try
            {
                // Loop to receive all the data sent by the client.
                while ((bytesRead = stream.Read(_buffer, 0, _buffer.Length)) != 0)
                {
                    // Convert the data received into a string.
                    //var strGet = Encoding.UTF8.GetString(buffer);
                    //Debug.Log(strGet);
                   
                    
                    List<byte> dataToSendBack = routeMessage(bytesRead);
                    if (justOnes < 3000)
                    {
                        stream.Write(dataToSendBack.ToArray());
                    }
                    justOnes++;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.ToString());
            }
            finally
            {
                // Shutdown and close the connection.
                _gameClient.TcpClient.Close();
            }
        }


       

        private List<byte> routeMessage(int lengthWholeMessage)
        {          Debug.Log(_gameClient.anyButtonPressed);
            List<byte> messageToSend = new List<byte>();

            int currentIndex = 0;
            Debug.Log("new messag received");
            int totalIndex = 0;
            while (totalIndex < lengthWholeMessage)
            {
                byte messageType = _buffer[currentIndex++];
                Debug.Log("got message with type " + (byte)messageType);
                int lengthMessage = BitConverter.ToInt32(_buffer, currentIndex); 
                totalIndex += lengthMessage; 
                currentIndex += 4;
                if (messageType == 0x6)
                {
                    Debug.Log("got message 0x6");
                    var pos = Vector3.zero;
                    var rot = Vector3.zero;
                    byte currentAnimation;
                    byte isShooting;
                    var c = _gameClient.ActivePlayer;
                    foreach (var networkObject in c)
                    {
                        pos = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        rot = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        currentAnimation = _buffer[currentIndex++];
                        isShooting = _buffer[currentIndex++];

                        networkObject.NetworkPosition = pos;
                        networkObject.EulerAngles = rot;
                        networkObject.CurrentAnimation = currentAnimation;
                        networkObject.IsShooting = isShooting;
                    }

                    var d = _gameClient.ObjectsToSync;
                    foreach (var networkObject in d)
                    {
                        pos = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        rot = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        currentAnimation = _buffer[currentIndex++];
                        isShooting = _buffer[currentIndex++];

                        networkObject.NetworkPosition = pos;
                        networkObject.EulerAngles = rot;
                        networkObject.CurrentAnimation = currentAnimation;
                        networkObject.IsShooting = isShooting;
                    }

                    if (_gameClient.ButtonsToSync)
                    {
                        Debug.Log("pressed a key");
                        addButtonInputsMessage(messageToSend,_gameClient.messagePressedButtons);
                    }

                }
                else if (messageType == 0x8)
                {
                    Debug.Log("got messageype 0x8");
                    var countObjects = BitConverter.ToSingle(_buffer, currentIndex);
                    currentIndex += 4;
                    var pos = Vector3.zero;
                    var rot = Vector3.zero;
                    for (var i = _gameClient.ObjectsToSync.Count; i < countObjects; i++)
                    {
                        Debug.Log("added new object");
                        pos = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        rot = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        var typeObject = _buffer[currentIndex++];
                        var objectToSpawn = new ObjectToSpawn(pos, typeObject, Quaternion.Euler(0, 0, 0));
                        _gameClient.BufferSpawnElements.Enqueue(objectToSpawn);
                    }
                    currentIndex += lengthFrame;
                }
                else if (messageType == 0x9)
                {
                    Debug.Log("got message 0x9");
                    var pos = Vector3.zero;
                    var rot = Vector3.zero;
                    var newCountActivePlayers = BitConverter.ToInt32(_buffer, currentIndex);
                    currentIndex += 4;
                    for (var i = _gameClient.ActivePlayer.Count; i < newCountActivePlayers; i++)
                    {
                        pos = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        Debug.Log("position to spawn at " + pos);
                        rot = subtractVector3FromByteArray(_buffer, currentIndex);
                        currentIndex += 12;
                        var typeObject = _buffer[currentIndex++];
                        Debug.Log("player with tyoe " + typeObject);
                        var objectToSpawn = new ObjectToSpawn(pos, typeObject, Quaternion.Euler(0, 0, 0));
                        _gameClient.BufferSpawnPlayers.Enqueue(objectToSpawn);
                    }
                    currentIndex += lengthFrame;
                }
                else if (messageType == 0x99)
                {
                    messageToSend.Add(0x99);
                    Debug.Log("got ping response");
                    currentIndex += lengthFrame;
                }
                else
                {
                    messageToSend.Add(0x50);
                    var t = Encoding.UTF8.GetBytes("Hallo welt");
                    foreach (var b in t) messageToSend.Add(b);
                    Debug.Log("soemtzhing wrong");
                    currentIndex += lengthFrame;
                }
                
                currentIndex = totalIndex;
            }

            if (messageToSend.Count == 0)
            {
                messageToSend.Add(0x6);
            }
            GameNetworkInitializer.Instance.AddDelimeterFrame(messageToSend,0);
            return messageToSend;

        }

        private void addButtonInputsMessage(List<byte> message, List<byte> buttons)
        {
            
            message.Add(0x3);
            message.Add((byte)message.Count);
            foreach (byte t in buttons)
            {
                  message.Add(t);
                  
            }
            buttons.Clear();
        }
              private Vector3 subtractVector3FromByteArray(byte[] arr, int index)
              { 
                  Vector3 vector3 = Vector3.zero; 
                  vector3.x = BitConverter.ToSingle(arr, index); 
                  index += 4; 
                  vector3.y = BitConverter.ToSingle(arr, index); 
                  index += 4; 
                  vector3.z = BitConverter.ToSingle(arr, index); 
                  
                  return vector3;
              }
    }
}