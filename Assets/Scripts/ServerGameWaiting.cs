using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerGameWaiting : ServerGameState {

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() { }

	public override ServerGameState GetNextState() {
		if (SnakeServer.I.HaveEnoughPlayersToStartGame()) {
			return GetComponent<ServerGameRunning>();
		} else {
			return null;
		}
	}
}
