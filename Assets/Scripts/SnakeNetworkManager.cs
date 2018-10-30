using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeNetworkManager : NetworkManager {

	public GameObject blockFloorPrefab;

	GameObject blockFloor;

	// public override void OnServerReady(NetworkConnection conn) {
	// 	Toolbox.Log("OnServerReady: " + conn.connectionId);
	// 	Snake.all.ForEach(x => x.DoFoo(conn));
	// }

	// public override void OnClientConnect(NetworkConnection conn) {
	// 	Toolbox.Log("OnClientConnect");
	// 	base.OnClientConnect(conn);
	// 	GlobalTick.I.Reset();
	// }

	public override void OnServerReady(NetworkConnection conn) {
		Toolbox.Log("OnServerReady");
		base.OnServerReady(conn);
	}

	public override void OnClientConnect(NetworkConnection conn) {
		Toolbox.Log("OnClientConnect");
		base.OnClientConnect(conn);
		Instantiate(blockFloorPrefab);
	}

	// TODO Not getting called
	public override void OnClientDisconnect(NetworkConnection conn) {
		Toolbox.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);
		Destroy(blockFloor);
	}
}
