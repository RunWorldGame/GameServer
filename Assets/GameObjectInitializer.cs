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
            if (number == 1)
            {
                
                return GetComponent<GameServer>().PlayerPrefab;
            }
            
            return GetComponent<GameServer>().PlayerPrefab;
        }
}
