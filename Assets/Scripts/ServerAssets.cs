using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerAssets : MonoBehaviour
{
    public static ServerAssets i;
    public GameObject player1;
    public GameObject player2;

    public ServerAssets(){
        i = this;
    }
}
