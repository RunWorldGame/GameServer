using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeController : NetworkMono
{
    // Start is called before the first frame update
    [SerializeField] private float time;
    private float timerAdd;
    private bool isTop;
    private Vector3 currentTrarget;
    private Vector3 top;
    private Vector3 down;
    
    public float speed = 1f;
    public override void Start()
    {
         base.Start();
         if (!isServerObj)
         {
             return;
         }
       
         timerAdd = 0f;
        isTop = false;
        currentTrarget = top;
        top = new Vector3(0, 1, 0);
        down = new Vector3(0, -1, 0);
    }
    
    public override void Update()
    {
       base.Update();
       if (!isServerObj)
       {
           return;
       }
       if (networkObjectPlayer.WPushed)
        {
            Debug.Log("updated speed");
            networkObjectPlayer.WPushed = false;
            speed *= 5;
        }
        
        timerAdd += Time.deltaTime;
        if (timerAdd > time)
        {
            timerAdd = 0;
            isTop = !isTop;
            currentTrarget = isTop ? top : down;
        }

        transform.position += currentTrarget * Time.deltaTime * speed;
        networkObjectPlayer.NetworkPosition = transform.position;
        networkObjectPlayer.EulerAngles = transform.rotation.eulerAngles;
    }


    
}
