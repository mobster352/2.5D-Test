using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Door : MonoBehaviour
{
    public bool canOpen;
    public bool doorOpening;
    private Animator myDoor;

    public Door(){
        canOpen = false;
        doorOpening = false;
    }

    public Door(bool canOpen, bool doorOpening){
        this.canOpen = canOpen;
        this.doorOpening = doorOpening;
    }

    private void Start() {
        canOpen = false;
        doorOpening = false;
        myDoor = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        canOpen = true;
        Player player = other.gameObject.GetComponentInParent<Player>();
        if(player){
            Debug.Log("Player near door: "+player.gameObject.name);
            player.CanOpenDoor(canOpen, this);
        }
    }

    private void OnTriggerExit(Collider other) {
        canOpen = false;
        Player player = other.gameObject.GetComponentInParent<Player>();
        if(player)
            player.CanOpenDoor(canOpen, this);
    }

    public void OpenDoor(){
        if(doorOpening)
            return;

        doorOpening = true;

        Debug.Log("Door opening");

        // transform.Translate(new Vector3(2.6f, 0, 0));
        // Vector3.MoveTowards(transform.position, transform.position+new Vector3(2.6f, 0, 0), 1 * Time.deltaTime);
        myDoor.Play("DoorOpen", 0, 0.0f);

        StartCoroutine("WaitToCloseDoor", 3f);
    }

    IEnumerator WaitToCloseDoor(float waitTime){
        yield return new WaitForSeconds(waitTime);
        CloseDoor();
    }

    void CloseDoor(){
        Debug.Log("Door closing");

        // transform.Translate(new Vector3(-2.6f, 0, 0));
        // Vector3.MoveTowards(transform.position, transform.position+new Vector3(-2.6f, 0, 0), 1 * Time.deltaTime);
        GetComponent<Animator>().Play("DoorClose", 0, 0.0f);

        doorOpening = false;
    }
}
