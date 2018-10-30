using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlobalTick : NetworkBehaviour {

	public static GlobalTick I;

	public float ticksPerSecond = 1;

	public static event Action OnDoTick;
	public static event Action OnRollbackTick;
	public static event Action<int> OnInitialized;

	float elapsedTime = 0;

	int _currentTick;
	public int currentTick {
		get { return _currentTick; }
		private set { _currentTick = value; DebugScreen.I.globalTickText.text = "Global Tick: " + value.ToString(); }
	}

	public bool manualTickDebugMode = false;
	bool initialized = false;

	void Awake() {
		Debug.Log("GlobalTick Awake");
		I = this;
	}

	void Start() {
		Debug.Log("GlobalTick Start");
		if (isServer) {
			StartCoroutine(SyncTickLoop());
		}
	}

	[Server]
	IEnumerator SyncTickLoop() {
		while (true) {
			yield return new WaitForSeconds(1);
			RpcReceiveTickSync(currentTick, elapsedTime);
		}
	}

	[ClientRpc]
	void RpcReceiveTickSync(int serverTick, float serverCurrentTickElapsedTime) {
		if (serverTick > currentTick) {
			RollForwardToTick(serverTick);
		} else if (serverTick < currentTick) {
			RollbackToTick(serverTick);
			// elapsedTime -= (currentTick - serverTick) / ticksPerSecond;
		}
		// this.elapsedTime = serverCurrentTickElapsedTime + (NetworkManager.singleton.client.GetRTT() / 2f / 1000f);
		// this.elapsedTime = 0;
	}

	void Update() {
		DebugScreen.I.globalTickNetID.text = netId.Value.ToString();
		if (!initialized) return;

		elapsedTime += Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.P)) {
			manualTickDebugMode = !manualTickDebugMode;
		}

		if (manualTickDebugMode) {
			if (Input.GetKeyDown(KeyCode.N)) {
				DoTick();
			}

			if (Input.GetKeyDown(KeyCode.B)) {
				RollbackTick();
			}
		} else {
			if (elapsedTime > (1 / ticksPerSecond)) {
				elapsedTime -= 1 / ticksPerSecond;
				DoTick();
			}
		}
	}

	void DoTick() {
		currentTick++;

		OnDoTick?.Invoke();

		if (currentTick / 10 % 2 == 0) {
			DebugScreen.I.SetPlaneRed();
		} else {
			DebugScreen.I.SetPlaneBlue();
		}
	}

	public void RollForwardToTick(int tick) {
		Toolbox.Log("RollForwardToTick " + tick);
		while (currentTick < tick) {
			DoTick();
		}
	}

	public int RollbackToTick(int tickToRollbackTo) {
		Toolbox.Log("RollbackToTick " + tickToRollbackTo);
		var rolledBackCount = 0;

		while (this.currentTick != tickToRollbackTo) {
			RollbackTick();
			rolledBackCount++;
		}

		return rolledBackCount;
	}

	void RollbackTick() {
		Toolbox.Log("RollbackTick");
		OnRollbackTick?.Invoke();
		currentTick--;
	}

	public void Reset() {
		Debug.Log("Reset");
		currentTick = 0;
		elapsedTime = 0;
	}

	[Server]
	public void InitTickForNewClient(NetworkConnection connection) {
		Debug.Log("InitTickForNewClient");
		TargetInitTick(connection, currentTick, elapsedTime);
	}

	[TargetRpc]
	public void TargetInitTick(NetworkConnection connection, int tick, float serverCurrentTickElapsedTime) {
		Debug.Log("RpcInitTick");
		Init(tick, serverCurrentTickElapsedTime);
	}

	public void Init(int tick, float serverCurrentTickElapsedTime) {
		Debug.Log("Init");
		currentTick = tick;
		initialized = true;
		elapsedTime = 0;
		// elapsedTime = serverCurrentTickElapsedTime + (NetworkManager.singleton.client.GetRTT() / 2f / 1000f);
		OnInitialized?.Invoke(currentTick);
	}
}
