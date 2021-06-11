using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    /// <summary>Player Prefabs that can be spawned over the network need to be registered here.</summary>
    public GameObject[] playerPrefabs;
    /// <summary>Player Spawns</summary>
    public Transform[] playerSpawns;
    

    public override void OnStartServer()
    {
        // base.OnStartServer();
        Debug.Log("Server started");
    }

    public override void OnStopServer()
    {
        // base.OnStopServer();
        Debug.Log("Server stopped");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Connected to server");
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        // base.OnClientDisconnect(conn);
        Debug.Log("Disconnected from server");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // base.OnServerAddPlayer(conn);
        int index = 0;

        //conn.connectionId == 0 - host
        if(conn.connectionId != 0){
            index = 1;
        }

        Debug.Log("Player Index: "+index);
        Transform startPos = playerSpawns[index];
        GameObject thePlayerPrefab = playerPrefabs[index];
            GameObject player = startPos != null
                ? Instantiate(thePlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(thePlayerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{thePlayerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
    }
}