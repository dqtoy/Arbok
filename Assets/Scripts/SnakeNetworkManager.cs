using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeNetworkManager : NetworkManager {

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

		GlobalTick.I.InitTickForNewClient(conn);
	}
}
