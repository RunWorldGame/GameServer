using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectInitializer : MonoBehaviour
{
    
    public GameObject instantiateGameObjectMainThread(ObjectToSpawn objectToSpawn)
    {
        return Instantiate(getObjectToNumber(1), objectToSpawn.Position, objectToSpawn.Quaternion);
    }
    
       private GameObject getObjectToNumber(int number)
        {
                var t =  GameNetworkInitializer.Instance.ObjectsSpawnable[0];
                Debug.Log(t.tag);
                return t;
            if (number == 1)
            {

            }
            
            return GameNetworkInitializer.Instance.ObjectsSpawnable[0];
        }
}
