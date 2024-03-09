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
}
