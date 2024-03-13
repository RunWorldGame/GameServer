using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectInitializer : MonoBehaviour
{
    
    public GameObject instantiateGameObjectMainThread(ObjectToSpawn objectToSpawn)
    {
        return Instantiate(getObjectToNumber(objectToSpawn.TypeObject), objectToSpawn.Position, objectToSpawn.Quaternion);
    }
    
       private GameObject getObjectToNumber(byte typeObj)
        {
            if (typeObj == 0x2)
            {
                Debug.Log("is second");
                return GameNetworkInitializer.Instance.ObjectsSpawnable[1];
            }
                var t =  GameNetworkInitializer.Instance.ObjectsSpawnable[0];
                Debug.Log(t.tag);
                return t;
        }
}
