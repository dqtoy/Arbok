using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeNetworkManager : NetworkManager {
	public static SnakeNetworkManager I;

	public GameObject gameStateManagerPrefab;
	public GameObject[] spawnOnClientConnect;

	GameObject blockFloor;

	void Awake() {
		I = this;
	}

	// Called first but server isn't setup yet
	public override void OnStartServer() {
		Toolbox.Log("OnStartServer");
		base.OnStartServer();
	}

	public override void OnClientConnect(NetworkConnection conn) {
		Toolbox.Log("OnClientConnect");

		foreach (var go in spawnOnClientConnect) {
			Instantiate(go);
		}

		base.OnClientConnect(conn);

		if (NetworkServer.active) {
			NetworkServer.Spawn(Instantiate(gameStateManagerPrefab));
		}
	}

	public override void OnServerReady(NetworkConnection conn) {
		Toolbox.Log("OnServerReady");
		base.OnServerReady(conn);
	}

	// TODO Not getting called
	public override void OnClientDisconnect(NetworkConnection conn) {
		Toolbox.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);
	}
}
