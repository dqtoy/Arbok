﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class AppleManager : NetworkBehaviour, ITickable {
	public static AppleManager I;

	public GameObject applePrefab;
	public int startingAppleCount;
	public int spawnAreaSize;
	public int deadZoneSize;

	IDictionary<DG_Position, AppleState> allApplesState = new Dictionary<DG_Position, AppleState>();

	public string SerializeAllApplesState() {
		Debug.Log(JsonConvert.SerializeObject(allApplesState));
		return JsonConvert.SerializeObject(allApplesState);
	}

	public void DeserializeAndLoadAllApplesState(string allApplesStateJson) {
		this.allApplesState = JsonConvert.DeserializeObject<Dictionary<DG_Position, AppleState>>(allApplesStateJson);

		this.allApplesState.Values.ToList().ForEach(SpawnApple);
	}

	void Awake() {
		I = this;

		GlobalTick.OnDoTick += DoTick;
		GlobalTick.OnRollbackTick += RollbackTick;
	}

	public void Reset() {
		Toolbox.Log("AppleManager Reset");
		DestroyAllApples();
		allApplesState.Clear();
	}

	public void DestroyAllApples() {
		allApplesState
			.Values
			.Select(SpawnedApple)
			.Where(NotNull)
			.ToList()
			.ForEach(Destroy);
	}

	GameObject SpawnedApple(AppleState x) => x.spawnedApple;

	bool NotNull(System.Object x) => x != null;

	void SpawnApple(AppleState appleState) {
		allApplesState[appleState.position].spawnedApple = Instantiate(
			applePrefab,
			new Vector3(appleState.position.x, 0, appleState.position.y),
			Quaternion.identity,
			transform
		);
	}

	void DeSpawnApple(AppleState appleState) {
		AppleState apple;

		if (TryGetAppleAtPosition(appleState.position, out apple) == false) {
			throw new Exception("Expected apple to be at position: spawnTick: " + appleState.spawnTick + " | " + JsonConvert.SerializeObject(appleState.position));
		}

		Destroy(apple.spawnedApple);

		allApplesState[appleState.position].spawnedApple = null;
	}

	public void DoTick(int tick) {
		// - is there an apple spawn at this tick?
		// - if yes, is there a snake head, tail, or wall on the apple position?
		// - if no, instantiate apple at position

		DG_Position x;

		TryGetApplePositionByTick(tick, out x);

		var appleState = TryGetAppleBySpawnTick(tick);

		if (appleState == null) return;

		if (IsAppleSpawnBlocked(appleState)) return;

		SpawnApple(appleState);
	}

	public AppleState TryGetAppleBySpawnTick(int tick) {
		DG_Position applePosition;

		if (TryGetApplePositionByTick(tick, out applePosition) == false) return null;

		if (allApplesState.ContainsKey(applePosition)) return allApplesState[applePosition];

		allApplesState[applePosition] = new AppleState() {
			position = applePosition,
				spawnTick = tick
		};

		return allApplesState[applePosition];
	}

	bool TryGetApplePositionByTick(int tick, out DG_Position x) {
		x = DG_Position.zero;

		if (tick % 2 == 0) return false;

		x.x = tick;
		x.y = tick;

		return true;
	}

	bool IsAppleSpawnBlocked(AppleState appleState) {
		return false;
	}

	public void RollbackTick(int tick) {
		// - is there an apple spawn at this tick?
		// - if yes, is there a snake head, tail, or wall on the apple position?
		// - if no, destroy apple at position

		var appleState = TryGetAppleBySpawnTick(tick);

		if (appleState == null) return;

		if (IsAppleSpawnBlocked(appleState)) return;

		DeSpawnApple(appleState);
	}

	public bool TryGetAppleAtPosition(DG_Position pos, out AppleState appleState) => allApplesState.TryGetValue(pos, out appleState);

	public void EatApple(int tick, AppleState appleState) {
		allApplesState[appleState.position].eatenTick = tick;
		DeSpawnApple(appleState);
	}

	public void ReverseEatApple(int tick, AppleState appleState) {
		SpawnApple(appleState);
		allApplesState[appleState.position].eatenTick = int.MaxValue;
	}
}
