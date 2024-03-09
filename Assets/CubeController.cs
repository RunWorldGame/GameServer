using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float time;
    private float timerAdd;
    private bool isTop;
    private Vector3 currentTrarget;
    private Vector3 top;
    private Vector3 down;

    private NetworkObjectPlayer _networkObjectPlayer;

    public float speed = 1f;
    void Start()
    {
        _networkObjectPlayer = GetComponent<NetworkObjectPlayer>();
        timerAdd = 0f;
        isTop = false;
        currentTrarget = top;
        top = new Vector3(0, 1, 0);
        down = new Vector3(0, -1, 0);
    }

    private void LateUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_networkObjectPlayer.WPushed)
        {
            Debug.Log("updated speed");
            _networkObjectPlayer.WPushed = false;
            speed *= 5;
        }
        timerAdd += Time.deltaTime;
        if (timerAdd > time)
        {
            timerAdd = 0;
            isTop = !isTop;
            currentTrarget = isTop ? top : down;
        }

        transform.position += currentTrarget * Time.deltaTime * GameServer.speed * speed;

    }
}
