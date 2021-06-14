using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Mirror;

public class MyNetworkRoomManager: NetworkRoomManager 
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

    public override void OnRoomClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnRoomClientConnect");
    }

    public override void OnRoomClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnRoomClientDisconnect");

        int index = -1;
        players.TryGetValue(conn.connectionId, out index);
        if(index != -1){
            players.Remove(conn.connectionId);
            if(index == 0)
                indexP1 = 0;
            else
                indexP2 = 0;
        }
        // Debug.Log($"indexP1: {indexP1} / indexP2: {indexP2}");
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        Debug.Log("OnRoomServerCreateGamePlayer");

        if(index > 1)
            index = 0;

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
            // return null;
        }

        if(!players.ContainsKey(conn.connectionId))
            players.Add(conn.connectionId, index);

        Debug.Log("index: "+index);

        Transform startPos = GameObject.FindObjectsOfType<NetworkStartPosition>()[index].transform;
        GameObject thePlayerPrefab = playerPrefabs[index];

        GameObject player = startPos != null
            ? Instantiate(thePlayerPrefab, startPos.position, startPos.rotation)
            : Instantiate(thePlayerPrefab);

        if(index==0)
            ServerAssets.i.player1 = player;
        else
            ServerAssets.i.player2 = player;

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{thePlayerPrefab.name} [connId={conn.connectionId}]";
        // NetworkServer.AddPlayerForConnection(conn, player);

        index++;

        return player;
    }

    public override void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        Debug.Log("OnRoomServerAddPlayer");

        
    }

    public override void OnRoomServerDisconnect(NetworkConnection conn)
    {
        base.OnRoomServerDisconnect(conn);
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        Debug.Log("OnServerDisconnect");
    }
}