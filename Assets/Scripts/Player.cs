using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    float moveSpeed = 20f;
    float _rotationSpeed = 10f;

    Rigidbody body;

    private void Start() {
        body = GetComponentInChildren<Rigidbody>();
    }

    void Movement(){
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        
        Vector3 movePos = new Vector3(moveH, 0, moveV);
        movePos = movePos.normalized * moveSpeed * Time.deltaTime;
        body.MovePosition(body.transform.position + movePos);
        if(movePos.magnitude > 0){
            Quaternion forwardsRotation = Quaternion.LookRotation(movePos);
            body.MoveRotation(forwardsRotation);
        }
    }

    void Update()
    {
        if(!isLocalPlayer)
            return;
        
        Movement();
    }
}
