using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Mirror;

public class TestEnemy : NetworkBehaviour
{
    [SyncVar]
    int hp;
    Animator animator;
    AstarAI astarAI;
    Collider col;
    int hitLayer;

    private void Start() {
        animator = GetComponent<Animator>();
        astarAI = GetComponent<AstarAI>();
        col = GetComponent<Collider>();

        hp = Random.Range(3,6);
        hitLayer = animator.GetLayerIndex("Hit Layer");
    }

    [Command(requiresAuthority = false)]
    void ChangeHealth(){
        hp -= 1;
        // Debug.Log("Enemy HP: "+hp);

        if(hp > 0){
            animator.SetTrigger("hit");
            animator.SetLayerWeight(hitLayer, 1);

            RpcSetTriggerAnim("hit");
            RpcSetLayerWeight(hitLayer, 1);
        }

        if(hp <= 0){
            Destroy(astarAI);
            Destroy(col);
            RpcChangeHealth();

            animator.SetLayerWeight(hitLayer, 0);
            RpcSetLayerWeight(hitLayer, 0);

            // animator.SetTrigger("deadTrig");
            // RpcSetTriggerAnim("deadTrig");
            animator.SetBool("dead", true);
            RpcSetBoolAnim("dead", true);
            
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
    
    [ClientRpc]
    void RpcSetBoolAnim(string name, bool val){
        animator.SetBool(name, val);
    }

    [ClientRpc]
    void RpcSetLayerWeight(int layerIndex, float weight){
        animator.SetLayerWeight(layerIndex, weight);
    }

    IEnumerator WaitDead(float time){
        yield return new WaitForSeconds(time);
        // Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
