using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {
    public LineRenderer lr;
	// Use this for initialization

    private void Awake() {
        gameObject.SetActive(false);   
    }
	void Start () {
        lr = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        lr.SetPosition(0, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.collider)
            {
                Vector3 hitPoint = hit.point;
                // hitPoint.y = lr.GetPosition(0).y;
                lr.SetPosition(1, hitPoint);
            }
        }
        else lr.SetPosition(1, transform.forward*5000);
	}
}
