using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Mirror;

public class TestEnemy : NetworkBehaviour
{
    [SyncVar]
    int hp = 5;
    Animator animator;
    AstarAI astarAI;
    Collider col;

    private void Start() {
        animator = GetComponent<Animator>();
        astarAI = GetComponent<AstarAI>();
        col = GetComponent<Collider>();
    }

    [Command(requiresAuthority = false)]
    void ChangeHealth(){
        hp -= 1;
        // Debug.Log("Enemy HP: "+hp);

        if(hp > 0){
            animator.SetTrigger("hit");
            RpcSetTriggerAnim("hit");
        }

        if(hp <= 0){
            Destroy(astarAI);
            Destroy(col);
            RpcChangeHealth();
            animator.SetTrigger("dead");
            RpcSetTriggerAnim("dead");
            StartCoroutine("WaitDead", 30);
        }
    }

    [ClientRpc]
    void RpcChangeHealth(){
        Destroy(astarAI);
        Destroy(col);
    }

    [ClientRpc]
    void RpcSetTriggerAnim(string name){
        animator.SetTrigger(name);
    }

    IEnumerator WaitDead(float time){
        yield return new WaitForSeconds(time);
        // Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
