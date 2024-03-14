using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Serialization;

public class GameNetworkInitializer : MonoBehaviour
{
   public bool IsServer;

   [SerializeField] private GameObject serverInstance;
   [SerializeField] private GameObject clientInstance;

   [HideInInspector] public Dictionary<KeyCode, byte> ButtonClickToByte;

   public List<NetworkObject> ObjectsToSync;

   public static GameNetworkInitializer Instance;

   public List<GameObject> ObjectsSpawnable;
   public byte[] DelimeterFrame;
   public byte[] anotherMessageComintFrame;
   public int LengthFrames
   {
      get => anotherMessageComintFrame.Length;
   }
   private void Awake()
   {
      Instance = this;
      DelimeterFrame = new byte[] { 9, 2, 1, 2, 1, 2, 1, 2, 1, 9 };
      ButtonClickToByte = new Dictionary<KeyCode, byte>(); 
      ButtonClickToByte.Add(KeyCode.Mouse1, 0x1);
      ButtonClickToByte.Add(KeyCode.Mouse2, 0x2); 
      ButtonClickToByte.Add(KeyCode.W, 0x3); 
      ButtonClickToByte.Add(KeyCode.A, 0x4); 
      ButtonClickToByte.Add(KeyCode.S, 0x5); 
      ButtonClickToByte.Add(KeyCode.D, 0x6); 
      ButtonClickToByte.Add(KeyCode.E, 0x7);  
      anotherMessageComintFrame = new byte[] { 5, 2, 1, 2, 1, 2, 1, 2, 1, 5 };
      if (IsServer)
      {
         serverInstance.SetActive(true);
      }
      else
      {
         clientInstance.SetActive(true);
      }
   }

   public void AddDelimeterFrame(List<byte> message, int offset)
   {
      Debug.Log("offset was "+ offset);
      Debug.Log("sending message " + message[offset]);
      foreach (byte b in DelimeterFrame)
      {
        message.Add(b); 
      }

      int size = message.Count - offset;
      message.InsertRange(offset + 1,BitConverter.GetBytes(size + 4));
   }
}
