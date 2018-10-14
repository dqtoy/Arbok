using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeNetworkManager : NetworkManager {

	// public override void OnServerReady(NetworkConnection conn) {
	// 	Debug.Log("OnServerReady: " + conn.connectionId);
	// 	Snake.all.ForEach(x => x.DoFoo(conn));
	// }
}
