using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using CodeMonkey.Utils;
using Mirror;

public class AstarAI : NetworkBehaviour
{
    public Transform targetPosition;
    private CharacterController controller;
    private Rigidbody rigidbody;
    // private Rigidbody rigidbody;

    public Path path;
    public float speed = 2f;
    public float nextWaypointDistance = 2f;
    [SyncVar]
    private int currentWaypoint = 0;
    public bool reachedEndOfPath;

    private Seeker seeker;
    [SyncVar]
    private Vector3 startingPosition;
    [SyncVar]
    private Vector3 roamPosition;
    public bool waitingForNextMove = false;
    [SyncVar]
    private bool pathToPlayer = false;
    [SyncVar]
    private bool pathToStart = false;
    // private Enemy enemy;
    private Animator animator;
    public float reachedRoamDistance;
    public float reachedPlayerDistance;
    public float stopChaseDistance;
    public float attackRange;


    private enum State {
        Roaming,
        ChaseTarget,
        GoingBackToStart,
        AttackPlayer
    }

    private State state;

    [SyncVar]
    Vector3 playerPos;
    float nextAttackTime;
    [SerializeField] private float rotationSpeed;

    private void Awake(){
        state = State.Roaming;
        // enemy = GetComponent<Enemy>();
    }

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();

        controller = GetComponentInChildren<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        // rigidbody = GetComponent<Rigidbody>();

        startingPosition = transform.position;

        roamPosition = GetRoamingPosition();
        seeker.StartPath(transform.position, roamPosition, OnPathComplete);

        // seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
        animator = GetComponentInChildren<Animator>();
    }

    private Vector3 GetRoamingPosition(){
        Vector3 randomPos = startingPosition + UtilsClass.GetRandomDirXZ() * Random.Range(2f, 3f);
        var info = AstarPath.active.GetNearest(randomPos, NNConstraint.Default);
        return info.position;
    }

    private void FindTarget(){
        float targetRange = 4f;
        Vector3 newPlayerPos = GetClosestPlayer();
        if(Vector3.Distance(transform.position, newPlayerPos) < targetRange){
            //player within target range
            state = State.ChaseTarget;
        }
    }

    Vector3 GetClosestPlayer(){
        if(ServerAssets.i.player1 == null)
            return Vector3.zero;
        if(ServerAssets.i.player2 == null)
            return ServerAssets.i.player1.GetComponentInChildren<Rigidbody>().transform.position;

        Vector3 p1Pos = ServerAssets.i.player1.GetComponentInChildren<Rigidbody>().transform.position;
        Vector3 p2Pos = ServerAssets.i.player2.GetComponentInChildren<Rigidbody>().transform.position;
        if(Vector3.Distance(transform.position, p1Pos) < Vector3.Distance(transform.position, p2Pos)){
            return p1Pos;
        }
        else
            return p2Pos;
    }

    public void OnPathComplete(Path p){
        // Debug.Log("Path Accepted. Error: "+p.error);

        if(!p.error){
            path = p;
            currentWaypoint = 0;
        }
    }

    public void HasReachDestination(){
        // float reachedPositionDistance = 2f;
        // Debug.Log("Roam pos: "+roamPosition);
        if(Vector3.Distance(transform.position, roamPosition) < reachedRoamDistance){
            //reached roam position
            // Debug.Log("Reached Roam");
            animator.SetBool("walking", false);
            StartCoroutine(WaitForNextMove(Random.Range(1f,4f)));
            waitingForNextMove = true;
            // return true;
        }
        // return false;
    }

    public void HasReachPlayer(Vector3 playerPos){
        // float reachedPositionDistance = 2f;
        // Debug.Log("Transform.position: "+transform.position+" / Magnitude: "+transform.position.magnitude);
        // Debug.Log("playerPos: "+playerPos+" / Magnitude: "+playerPos.magnitude);
        // Debug.Log("Distance: "+Vector3.Distance(transform.position, playerPos)+ " / reachedPlayerDistance: "+reachedPlayerDistance);
        if(Vector3.Distance(transform.position, playerPos) < reachedPlayerDistance){
            //reached roam position
            // Debug.Log("Reached Roam");
            animator.SetBool("walking", false);
            StartCoroutine(WaitForNextMove(0));
            // return true;
        }
        // return false;
    }

    public void HasReachStartPosition(){
        float reachedPositionDistance = 2f;
        if(Vector3.Distance(transform.position, startingPosition) < reachedPositionDistance){
            state = State.Roaming;
            pathToStart = false;
            waitingForNextMove = false;
            animator.SetBool("walking", false);
            StartCoroutine(WaitForNextMove(0));
            // return true;
        }
        // return false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Debug.Log("State: "+state);

        if(isServer){
            Vector3 newPlayerPos = GetClosestPlayer();
            switch(state){
                // default:
                case State.Roaming: 
                // Debug.Log("Here");
                    if(waitingForNextMove)
                        return;

                    moveTo(roamPosition);
                    HasReachDestination();

                    FindTarget();
                    break;

                case State.ChaseTarget:
                // Debug.Log("Here2");

                    float playerTooFarFromOrigin = 2f;
                    if(!pathToPlayer || Vector3.Distance(newPlayerPos, playerPos) > playerTooFarFromOrigin){
                        pathToPlayer = true;
                        playerPos = newPlayerPos;
                        seeker.StartPath(transform.position, playerPos, OnPathComplete);
                    }
                    else{
                        moveTo(playerPos);
                        HasReachPlayer(playerPos);
                    }

                    // float stopChaseDistance = 20f;
                    // if(Vector3.Distance(transform.position, Player.current.getPosition()) > stopChaseDistance){
                    if(Vector3.Distance(transform.position, startingPosition) > stopChaseDistance){
                        //Too far, stop chasing
                        state = State.GoingBackToStart;
                        UpdateState(state);
                        pathToPlayer = false;
                    }

                    // float attackRange = 3f;
                    if(Vector3.Distance(transform.position, newPlayerPos) < attackRange){
                        //Too far, stop chasing
                        state = State.AttackPlayer;
                        UpdateState(state);
                        pathToPlayer = false;
                    }

                    break;
                
                case State.GoingBackToStart:
                    if(!pathToStart){
                        seeker.StartPath(transform.position, startingPosition, OnPathComplete);
                        pathToStart = true;
                    }
                    else{
                        moveTo(startingPosition);
                        HasReachStartPosition();
                    }
                    break;

                case State.AttackPlayer:
                    AttackPlayerState();

                    float playerTooFar = 3f;
                    if(Vector3.Distance(transform.position, newPlayerPos) > playerTooFar){
                        //Too far, stop chasing
                        state = State.ChaseTarget;
                        UpdateState(state);
                    }
                    break;
            }
        }

        if(isClient){
            switch(state){
                case State.AttackPlayer:
                    AttackPlayerState();
                    break;
            }
        }
    }

    void AttackPlayerState(){
        if(Time.time > nextAttackTime){
            //stop moving
            animator.SetBool("walking", false);
            animator.SetTrigger("attack");
            //can attack every 4 seconds
            float attackRate = 4f;
            nextAttackTime = Time.time + attackRate;
            // enemy.attackPlayer();
        }
    }

    [ClientRpc]
    void UpdateState(State state){
        this.state = state;
        // Debug.Log("New state: "+state);
    }

    public void moveTo(Vector3 roamPosition){
        if(path==null){
            return;
        }

        animator.SetBool("walking", true);

        reachedEndOfPath = false;
        float distanceToWaypoint;
        // float timeCount = 0.0f;
        while(true){
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);

            if(distanceToWaypoint < nextWaypointDistance){
                if(currentWaypoint + 1 < path.vectorPath.Count){
                    currentWaypoint++;
                }
                else{
                    reachedEndOfPath = true;
                    break;
                }
            }
            else{
                break;
            }
        }

        var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint/nextWaypointDistance) : 1f;

        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector3 velocity = dir * speed * speedFactor;
        // Debug.Log("Moving now");
        if(controller != null)
            controller.SimpleMove(velocity);
        if(rigidbody != null)
            rigidbody.MovePosition(transform.position + velocity * Time.deltaTime);
    }

    IEnumerator WaitForNextMove(float time){
        yield return new WaitForSeconds(time);
        if(state == State.Roaming){
            // Debug.Log("Roaming");
            roamPosition = GetRoamingPosition();
            seeker.StartPath(transform.position, roamPosition, OnPathComplete);
            waitingForNextMove = false;
            path = null;
        }
        if(state == State.ChaseTarget){
            // Debug.Log("Chasing");
            // seeker.StartPath(transform.position, Player.current.getPosition(), OnPathCompleteToPlayer);
            pathToPlayer = false;
            path = null;
        }
    }

    void LateUpdate(){
        if(!isServer)
            return;

        Vector3 newPlayerPos = GetClosestPlayer();
        var rotation = new Quaternion();
        switch(state){
            case State.Roaming:
                if(path==null)
                    return;
                rotation = Quaternion.LookRotation(path.vectorPath[currentWaypoint] - transform.position);
                rotation.x = 0;
                rotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
                break;
            case State.ChaseTarget:
                if(path==null)
                    return;
                rotation = Quaternion.LookRotation(newPlayerPos - transform.position);
                rotation.x = 0;
                rotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
                break;
            case State.AttackPlayer:
                rotation = Quaternion.LookRotation(newPlayerPos - transform.position);
                rotation.x = 0;
                rotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
                break;
            case State.GoingBackToStart:
                if(path==null)
                    return;
                rotation = Quaternion.LookRotation(path.vectorPath[currentWaypoint] - transform.position);
                rotation.x = 0;
                rotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
                break;
        }
    }

    public Vector3 getStartingPosition(){
        return startingPosition;
    }
}
