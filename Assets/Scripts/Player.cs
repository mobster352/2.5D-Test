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
    Weapon weapon;
    Animator animator;

    // [SerializeField]
    // Value to rotate the body around
    float rotateBody;
    bool canOpen;
    Door currentDoor;

    private void Start() {
        body = GetComponentInChildren<Rigidbody>();
        isAiming = false;
        moveSpeed = 20f;
        _rotationSpeed = 10f;
        main = GetComponentInChildren<Camera>();
        bulletPrefab = GameAssets.i.bulletPrefab;
        laser = GetComponentInChildren<Laser>(true);
        weapon = GetComponentInChildren<Weapon>();
        rotateBody = 35f;
        animator = GetComponentInChildren<Animator>();
        canOpen = false;
    }

    void Movement(){
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        
        Vector3 movePos = new Vector3(moveH, 0, moveV);
        movePos = movePos.normalized * moveSpeed * Time.deltaTime;
        // Debug.Log("MovePos: "+movePos);
        body.MovePosition(body.transform.position + movePos);

        if(movePos.magnitude > 0){
            Quaternion forwardsRotation = Quaternion.LookRotation(movePos);
            forwardsRotation.eulerAngles += new Vector3(0,rotateBody,0);
            body.MoveRotation(forwardsRotation);
            // Debug.Log("forwardsRotation: "+forwardsRotation.eulerAngles);

            // Debug.Log("Weapon Forward: "+weapon.transform.forward);
            if(weapon.transform.forward.z != 0){
                animator.SetBool("isWalkingForward", true);
            }
        }
        else
            animator.SetBool("isWalkingForward", false);
    }

    void Aim(){
        isAiming = Input.GetButton("Aim");
        if(isAiming){
            animator.SetBool("isWalkingForward", false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            // startPos = body.transform.position;
            startPos = weapon.transform.position;
            // forwardDir = body.transform.TransformDirection(Vector3.forward);
            // forwardDir = weapon.transform.TransformDirection(Vector3.forward);
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
                body.transform.Rotate(new Vector3(0,rotateBody,0));
            }
        }
        else{
            if(laser.gameObject.activeSelf){
                laser.gameObject.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                BroadcastMessage("isFiring", false);
            }
        }
    }

    void Fire(){
        bool isFiring = Input.GetButtonDown("Fire1");
        if(isFiring && isAiming){
            // Debug.Log("Fire!");
            // Transform bullet = Instantiate(bulletPrefab, startPos, Quaternion.LookRotation(forwardDir));
            // Physics.IgnoreCollision(bullet.GetComponent<Collider>(), body.GetComponent<Collider>(), true);
            BroadcastMessage("isFiring", true);
        }
    }

    void Update()
    {
        if(!isLocalPlayer)
            return;
        
        if(!isAiming)
            Movement();
        
        Fire();

        if(canOpen && Input.GetButtonDown("Use")){
            currentDoor.OpenDoor();
        }
    }

    private void FixedUpdate() {
        if(!isLocalPlayer)
            return;

        Aim();
    }

    [Command]
	void CmdSpawnBullets(){
		// Debug.Log("Here");
		// Recoil
		if (weapon.recoil)
			weapon.Recoil();
		
		// Muzzle flash effects
		if (weapon.makeMuzzleEffects)
		{
			GameObject muzfx = weapon.muzzleEffects[Random.Range(0, weapon.muzzleEffects.Length)];
			if (muzfx != null){
				GameObject muzzle = Instantiate(muzfx, weapon.muzzleEffectsPosition.position, weapon.muzzleEffectsPosition.rotation);
				NetworkServer.Spawn(muzzle);
			}
		}

		// Instantiate shell props
		if (weapon.spitShells)
		{
			GameObject shellGO = Instantiate(weapon.shell, weapon.shellSpitPosition.position, weapon.shellSpitPosition.rotation) as GameObject;
			shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(weapon.shellSpitForce + Random.Range(0, weapon.shellForceRandom), 0, 0), ForceMode.Impulse);
			shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(weapon.shellSpitTorqueX + Random.Range(-weapon.shellTorqueRandom, weapon.shellTorqueRandom), weapon.shellSpitTorqueY + Random.Range(-weapon.shellTorqueRandom, weapon.shellTorqueRandom), 0), ForceMode.Impulse);
			NetworkServer.Spawn(shellGO);
		}

		// Play the gunshot sound
		weapon.GetComponent<AudioSource>().PlayOneShot(weapon.fireSound);
	}

    public void CanOpenDoor(bool canOpen, Door door){
        this.canOpen = canOpen;
        currentDoor = door;
    }

}
