using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    Collider collider;

    private void Start() {
        collider = GetComponent<Collider>();   
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Entered ammo");
        other.GetComponentInParent<Player>().AmmoPickupInRange(gameObject);
    }

    private void OnTriggerExit(Collider other) {
        other.GetComponentInParent<Player>().AmmoPickupOutOfRange();
    }
}
