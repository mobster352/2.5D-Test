using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed;
    // Start is called before the first frame update
    void Start()
    {
        speed = 50;
        StartCoroutine(BulletTimer(3, transform));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void OnCollisionEnter(Collision col){
        if(col.gameObject.layer != LayerMask.NameToLayer("Player"))
            Destroy(this.gameObject);
    }

    IEnumerator BulletTimer(float time, Transform bullet){
        yield return new WaitForSeconds(time);
        Destroy(bullet.gameObject);
    }
}
