using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameStateManager : NetworkBehaviour {
	public static GameStateManager I;

	public int minimumPlayerCount = 2;

	[SyncVar]
	int playerCount = 0;
	[SyncVar]
	int alivePayerCount = 0;
	[SyncVar]
	string stateString;

	GameState state;

	void Awake() {
		I = this;
	}

	void Start() {
		if (!isServer) return;

		Toolbox.Log("SnakeServer Start");
		state = GetComponent<GameWaiting>();
		state.Enter();
		stateString = state.GetType().Name;
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
			stateString = state.GetType().Name;
			state.Enter();
		}
	}

	void UpdateClient() {
		GameUI.I.SetMainGameText(stateString);
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

	public int GetAlivePlayerCount() {
		return alivePayerCount;
	}

	void OnDestroy() {
		Destroy(BlockFloor.I.gameObject);
		AppleManager.I.DestroyAll();
	}
}
