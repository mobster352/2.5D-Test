using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Camera : NetworkBehaviour
{
    Camera main;
    Player player;
    Rigidbody body;
    Collider c;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        main = GetComponentInChildren<Camera>();
        player = GetComponent<Player>();
        // main = GetComponent<Camera>();
        // player = GetComponentInParent<Player>();
        c = GetComponentInChildren<Collider>();
        
        if(player == null)
            Debug.Log("Null player");

        body = player.GetComponentInChildren<Rigidbody>();

        // offset = main.transform.position - body.transform.position;
        offset = main.transform.position - c.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
            return;

        // main.transform.position =  body.transform.position + offset;
        main.transform.position =  c.transform.position + offset;
    }
}
