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
    int indexP1 = 0;
    int indexP2 = 0;
    int index = 0;
    Dictionary<int, int> players = new Dictionary<int, int>();

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
        // int index = 0;

        //conn.connectionId == 0 - host
        // if(conn.connectionId != 0){
        //     index = 1;
        // }
        // int index = conn.connectionId;

        if(indexP1 == 0){
            index = 0;
            indexP1 = 1;
        }            
        else if(indexP2 == 0){
            index = 1;
            indexP2 = 1;
        }
        else {
            Debug.Log("Max # of players already in game");
            return;
        }

        Debug.Log($"indexP1: {indexP1} / indexP2: {indexP2}");
        
        Debug.Log("Player Index: "+index);
        players.Add(conn.connectionId, index);
        Transform startPos = playerSpawns[index];
        GameObject thePlayerPrefab = playerPrefabs[index];
            GameObject player = startPos != null
                ? Instantiate(thePlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(thePlayerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{thePlayerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

        index++;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        int index = -1;
        players.TryGetValue(conn.connectionId, out index);
        if(index != -1){
            players.Remove(conn.connectionId);
            if(index == 0)
                indexP1 = 0;
            else
                indexP2 = 0;
        }
        Debug.Log($"indexP1: {indexP1} / indexP2: {indexP2}");
    }
}