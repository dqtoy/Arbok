using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameWaiting : GameState {
	public GameObject globalTickPrefab;
	public GameObject[] networkSpawnOnEnter;
	public GameObject[] spawnOnEnter;

	[Server]
	public override GameState GetNextState() {
		if (GameStateManager.I.HaveEnoughPlayersToStartGame()) {
			return GetComponent<GameRunning>();
		} else {
			return null;
		}
	}

	[Server]
	public override void Enter() {
		foreach (var go in networkSpawnOnEnter) {
			NetworkServer.Spawn(Instantiate(go));
		}

		foreach (var go in spawnOnEnter) {
			Instantiate(go);
		}

		var globalTick = Instantiate(globalTickPrefab);
		globalTick.GetComponent<GlobalTick>().ServerStart();
		NetworkServer.Spawn(globalTick);
	}
}
