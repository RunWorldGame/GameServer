using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class GameNetworkInitializer : MonoBehaviour
{
   public bool IsServer;

   [SerializeField] private GameObject serverInstance;
   [SerializeField] private GameObject clientInstance;


   public List<NetworkObject> ObjectsToSync;

   public static GameNetworkInitializer Instance;

   public List<GameObject> ObjectsSpawnable;
   public byte[] delimeterFrame = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 ,0x99,0x12};
   public byte[] anotherMessageComintFrame = new byte[] { 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x12, 0x34 };
   private void Awake()
   {
      Instance = this;
      
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
      foreach (byte b in delimeterFrame)
      {
        message.Add(b); 
      }

      int size = message.Count - offset;
      message.InsertRange(offset + 1,BitConverter.GetBytes(size + 4));
   }
}
