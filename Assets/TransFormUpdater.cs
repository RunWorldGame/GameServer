using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace.Scenes
{
    public class TransFormUpdater
    {
        public Transform objectToTrack;
        private Socket _clientSocket;

        
        private byte[] _buffer;

        public Vector3 currentNetworkConnectionPosition;
        
        private EndPoint _senderRemote;

        private GameObjectInitializer _gameObjectInitializer;

        private GameClient _gameClient;

        private int counter = 0;
        
        public TransFormUpdater( GameClient gameClient)
        {
            _gameClient = gameClient;
            _senderRemote = new IPEndPoint(IPAddress.Any, 0);
            _clientSocket = gameClient._clientSocket;
            _buffer = new byte[1024];
        }
        

        public void StartListeningForNewPositions()
        {
            //_listenEvent.Invoke();
            Debug.Log("just listening");
            Listen();
        }
        
        public void Listen()
        {

            Debug.Log("invoked");
            while (true)
            {
                 _ = _clientSocket.ReceiveFrom(_buffer, ref _senderRemote);
                 routeMessage();
            }
        }
        private void routeMessage()
             {
                 int currentIndex = 0;
                 byte messageType = _buffer[0]; 
                 if (messageType == 0x1) 
                 { 
                     Debug.Log("is new player request"); 
                     var nameLength = _buffer[1]; 
                     Debug.Log("name lenghtj " + ((int)nameLength)); 
                     currentIndex = (int)nameLength + 2; 
                     byte[] nameBytes = new byte[(int) nameLength]; 
                     Array.Copy(_buffer, 2,nameBytes,0, nameLength);
                     string name = Encoding.UTF8.GetString(nameBytes);
                     Debug.Log("name is " + name );
                     Vector3 spawnPos = Vector3.zero;
                     byte[] floatA = new byte[4];
                     Array.Copy(_buffer, currentIndex,floatA,0, 4);
                     spawnPos.x = BitConverter.ToSingle(floatA);
                     currentIndex += 4;
                     Array.Copy(_buffer, currentIndex,floatA,0, 4);
                     spawnPos.y = BitConverter.ToSingle(floatA);
                     currentIndex += 4;
                     Array.Copy(_buffer, currentIndex,floatA,0, 4);
                     spawnPos.z = BitConverter.ToSingle(floatA);
                     
                     currentIndex += 4; 
                        
                     Debug.Log("vector 3 is " + spawnPos);
                     
                    ObjectToSpawn objectToSpawn = new ObjectToSpawn(spawnPos, 1, new Quaternion(0,0,0,0));
                    _gameClient.BufferSpawnElements.Enqueue(objectToSpawn);
                    
                    
                     
                 }
                 else if (messageType == 0x6)
                 {
                     
                     Vector3 pos = Vector3.zero;
                     Vector3 rot = Vector3.zero;
                     byte currentAnimation;
                     byte isShooting;
                     int currentIndexList = 1;
                     int m = 0;
                     Debug.Log(counter);
                     var d = _gameClient.ObjectsToSync;
                     foreach (var networkObject in d)
                     {
                         
                         pos = subtractVector3FromByteArray(_buffer, currentIndexList);
                         currentIndexList += 12;
                         rot = subtractVector3FromByteArray(_buffer, currentIndexList);
                         currentIndexList += 12;
                         currentAnimation = _buffer[currentIndexList++];
                         isShooting = _buffer[currentIndexList++];
                         
                         
                         networkObject.NetworkPosition = pos; 
                         networkObject.EulerAngles = rot; 
                         networkObject.CurrentAnimation = currentAnimation; 
                         networkObject.IsShooting = isShooting;
                         Debug.Log("" + counter + " new pos " + pos);
                     }

                     counter++;
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
                  
                  return vector3;
              }
           
    }
}