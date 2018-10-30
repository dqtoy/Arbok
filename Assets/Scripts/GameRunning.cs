using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRunning : GameState {
	public override GameState GetNextState() {
		if (GameStateManager.I.GetAlivePlayerCount() <= 1) {
			return GetComponent<GameOver>();
		} else {
			return null;
		}
	}
}
