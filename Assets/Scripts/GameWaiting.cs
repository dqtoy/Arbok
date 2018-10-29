using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameWaiting : GameState {

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() { }

	public override GameState GetNextState() {
		if (GameStateManager.I.HaveEnoughPlayersToStartGame()) {
			return GetComponent<GameRunning>();
		} else {
			return null;
		}
	}
}
