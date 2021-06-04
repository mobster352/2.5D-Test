using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    float moveSpeed = 5;
    void Movement(){
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        Vector3 movePos = new Vector3(moveH, 0, moveV);
        transform.position = transform.position + movePos * moveSpeed * Time.deltaTime;
    }

    void Update()
    {
        if(!isLocalPlayer)
            return;
        
        Movement();
    }
}
