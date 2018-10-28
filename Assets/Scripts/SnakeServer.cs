using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeServer : NetworkBehaviour {
	public static SnakeServer I;

	public int minimumPlayerCount = 2;

	[SyncVar]
	int playerCount = 0;
	[SyncVar]
	int alivePayerCount = 0;

	ServerGameState state;

	void Awake() {
		I = this;
	}

	void Start() {
		if (!isServer) return;

		Toolbox.Log("SnakeServer Start");
		state = GetComponent<ServerGameWaiting>();
	}

	void Update() {
		if (isServer) UpdateServer();
		if (isClient) UpdateClient();
	}

	void UpdateServer() {
		playerCount = NetworkServer.connections.Count;
		alivePayerCount = Snake.GetAlivePlayerCount();

		var nextState = state.GetNextState();

		if (nextState) {
			Toolbox.Log("SnakeServer switching state: " + nextState.GetType().Name);
			state.Exit();
			state = nextState;
			state.Enter();
		}
	}

	void UpdateClient() {
		GameUI.I.SetMainGameText(playerCount + " Player" + (playerCount == 1 ? "" : "s"));
		GameUI.I.SetAlivePlayersText(alivePayerCount);
	}

	public override void OnStartServer() {
		Toolbox.Log("SnakeServer OnStartServer");
	}

	public bool HaveEnoughPlayersToStartGame() {
		return NeededPlayerCount() == 0;
	}

	public int NeededPlayerCount() {
		if (playerCount >= minimumPlayerCount) {
			return 0;
		} else {
			return minimumPlayerCount - playerCount;
		}
	}
}
