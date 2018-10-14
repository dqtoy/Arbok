using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeSpawnMessage : MessageBase {

}

public class SnakeNetworkManager : NetworkManager {

	// Use this for initialization
	void Start() { }

	// Update is called once per frame
	void Update() {

	}

	const short MsgTypeSnakeSpawn = 1000;

	#region Client
	#endregion

	//Detect when a client connects to the Server
	public override void OnClientConnect(NetworkConnection connection) {
		base.OnClientConnect(connection);
		//Change the text to show the connection on the client side
		Debug.Log(connection.connectionId + " Connected!");
		var snake = Instantiate(playerPrefab);
		snake.GetComponent<Snake>().isLocalPlayer = true;
		client.Send(MsgTypeSnakeSpawn, new SnakeSpawnMessage());
	}

	//Detect when a client connects to the Server
	public override void OnClientDisconnect(NetworkConnection connection) {
		base.OnClientDisconnect(connection);
		//Change the text to show the connection loss on the client side
		Debug.Log("Connection" + connection.connectionId + " Lost!");
	}

	public override void OnStartClient(NetworkClient client) {
		Debug.Log("OnStartClient");
		client.RegisterHandler(MsgTypeSnakeSpawn, OnServerSnakeSpawn);
	}

	public override void OnStartServer() {
		Debug.Log("OnStartServer");
		NetworkServer.RegisterHandler(MsgTypeSnakeSpawn, OnServerSnakeSpawn);
	}

	public void OnServerSnakeSpawn(NetworkMessage networkMessage) {
		Debug.Log("OnServerSnakeSpawn: networkMessage " + JsonUtility.ToJson(networkMessage));
		Debug.Log("OnServerSnakeSpawn: networkMessage.conn " + JsonUtility.ToJson(networkMessage.conn));
		Debug.Log("OnServerSnakeSpawn: networkMessage.conn.connectionId " + networkMessage.conn.connectionId);
		Debug.Log("OnClientSnakeSpawn: " + networkMessage);
		var isMessageFromHost = networkMessage.conn.connectionId == 0;
		Debug.Log("OnServerSnakeSpawn: NetworkServer.localConnections " + String.Join(", ", NetworkServer.localConnections.Select(JsonUtility.ToJson).ToArray()));
		Debug.Log("OnServerSnakeSpawn: NetworkServer.connections " + String.Join(", ", NetworkServer.connections.Select(JsonUtility.ToJson).ToArray()));

		var snakeSpawnMessage = networkMessage.ReadMessage<SnakeSpawnMessage>();
		var connectionIdsWithHost = NetworkServer.connections.Select(x => x.connectionId).ToList();
		connectionIdsWithHost.Add(0);
		Debug.Log("OnServerSnakeSpawn: connectionIdsWithHost.Count " + connectionIdsWithHost.Count);
		Debug.Log("OnServerSnakeSpawn: connectionIdsWithHost " + String.Join(", ", connectionIdsWithHost.Select(x => x.ToString()).ToArray()));
		var filteredConnectionIds = connectionIdsWithHost.Where(x => x != networkMessage.conn.connectionId).ToList();
		Debug.Log("OnServerSnakeSpawn: filteredConnectionIds " + String.Join(", ", filteredConnectionIds.Select(x => x.ToString()).ToArray()));
		filteredConnectionIds.ForEach(x => NetworkServer.SendToClient(x, MsgTypeSnakeSpawn, snakeSpawnMessage));
	}

	public void OnClientSnakeSpawn(NetworkMessage networkMessage) {
		Debug.Log("OnClientSnakeSpawn: " + networkMessage);
		var y = networkMessage.ReadMessage<SnakeSpawnMessage>();
		Instantiate(playerPrefab);
	}
}
