using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    float moveSpeed;
    [SerializeField]
    LayerMask layerMask;
    float _rotationSpeed;

    Rigidbody body;
    bool isAiming;
    Camera main;
    Vector3 worldPos;
    Vector3 startPos;
    Vector3 forwardDir;
    Transform bulletPrefab;
    Laser laser;

    private void Start() {
        body = GetComponentInChildren<Rigidbody>();
        isAiming = false;
        moveSpeed = 20f;
        _rotationSpeed = 10f;
        main = GetComponentInChildren<Camera>();
        bulletPrefab = GameAssets.i.bulletPrefab;
        laser = GetComponentInChildren<Laser>(true);
    }

    void Movement(){
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        
        Vector3 movePos = new Vector3(moveH, 0, moveV);
        movePos = movePos.normalized * moveSpeed * Time.deltaTime;
        // Debug.Log("MovePos: "+movePos);
        body.MovePosition(body.transform.position + movePos);

        if(movePos.magnitude > 0 && !isAiming){
            Quaternion forwardsRotation = Quaternion.LookRotation(movePos);
            body.MoveRotation(forwardsRotation);
            // Debug.Log("forwardsRotation: "+forwardsRotation.eulerAngles);
        }
    }

    void Aim(){
        isAiming = Input.GetButton("Aim");
        if(isAiming){
            startPos = body.transform.position;
            forwardDir = body.transform.TransformDirection(Vector3.forward);
            // Vector3 endPos = body.transform.position + forwardDir * 10;
            // Debug.DrawLine(startPos, endPos,Color.red);

            Ray ray = main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 hitPoint;
            if(Physics.Raycast(ray, out hit, 100, layerMask)){
                // Debug.Log(("Hit"));
                hitPoint = hit.point;
                hitPoint.y = 0;
                worldPos = new Vector3(hitPoint.x, body.transform.position.y, hitPoint.z);
                Debug.DrawLine(startPos, worldPos);
                laser.gameObject.SetActive(true);
                body.transform.LookAt(worldPos);
            }
        }
        else{
            laser.gameObject.SetActive(false);
        }
    }

    void Fire(){
        bool isFiring = Input.GetButtonDown("Fire1");
        if(isFiring && isAiming){
            Debug.Log("Fire!");
            Transform bullet = Instantiate(bulletPrefab, startPos, Quaternion.LookRotation(forwardDir));
            Physics.IgnoreCollision(bullet.GetComponent<Collider>(), body.GetComponent<Collider>(), true);
        }
    }

    void Update()
    {
        if(!isLocalPlayer)
            return;
        
        Movement();
        
        Fire();
    }

    private void FixedUpdate() {
        if(!isLocalPlayer)
            return;

        Aim();
    }
}
