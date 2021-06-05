using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FollowCamera : MonoBehaviour
{
    Camera main;
    Player player;
    Rigidbody body;
    Collider c;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        main = GetComponent<Camera>();
        player = GetComponentInParent<Player>();
        c = GetComponentInChildren<Collider>();
        
        if(player == null)
            Debug.Log("Null player");

        body = player.GetComponentInChildren<Rigidbody>();

        offset = main.transform.position - body.transform.position;

        if(!player.GetComponent<NetworkIdentity>().isLocalPlayer){
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        main.transform.position =  body.transform.position + offset;
    }
}
