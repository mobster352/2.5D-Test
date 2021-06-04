using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Camera : MonoBehaviour
{
    Camera main;
    Player player;
    Rigidbody body;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        main = GetComponent<Camera>();
        player = GetComponentInParent<Player>();
        
        if(player == null)
            Debug.Log("Null player");

        body = player.GetComponentInChildren<Rigidbody>();

        offset = transform.position - body.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // if(!isLocalPlayer)
        //     return;

        transform.position =  body.transform.position + offset;
    }
}
