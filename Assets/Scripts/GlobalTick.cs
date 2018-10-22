using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GlobalTick : NetworkBehaviour {

	public static GlobalTick I;

	public float ticksPerSecond = 1;

	public event Action OnDoTick;
	public event Action OnRollbackTick;

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
			RpcReceiveTickSync(currentTick);
		}
	}

	[ClientRpc]
	void RpcReceiveTickSync(int tick) {
		if (tick > currentTick) {
			RollForwardToTick(tick);
		} else if (tick < currentTick) {
			RollbackToTick(tick);
		}
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
	}

	public void RollForwardToTick(int tick) {
		Toolbox.Log("RollbackToTick " + tick);
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
		TargetInitTick(connection, currentTick);
	}

	[TargetRpc]
	public void TargetInitTick(NetworkConnection connection, int tick) {
		Debug.Log("RpcInitTick");
		Init(tick);
	}

	public void Init(int tick) {
		Debug.Log("Init");
		currentTick = tick;
		initialized = true;
		elapsedTime += NetworkManager.singleton.client.GetRTT() / 2;
	}
}
