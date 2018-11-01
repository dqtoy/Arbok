using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class AppleManager : NetworkBehaviour, ITickable {
	public static AppleManager I;

	public List<Apple> all = new List<Apple>();
	public GameObject applePrefab;
	public int startingAppleCount;
	public int spawnAreaSize;
	public int deadZoneSize;

	void Awake() {
		I = this;
	}

	[Server]
	public void SpawnStartingApples() {
		Toolbox.Log("AppleManager SpawnStartingApples");
		Reset();
		for (int i = 0; i < startingAppleCount; i++) {
			SpawnRandomApple();
		}
	}

	[Server]
	void SpawnRandomApple() => SpawnApple(new AppleState() { isActive = true, position = RandomSpawnPosition() });

	public void DeSpawnApple(Vector3 applePos) {
		var apple = GetAppleAtPosition(applePos);
		if (apple.gameObject.activeSelf == false) {
			throw new Exception("Expected apple to be alive");
		}
		apple.gameObject.SetActive(false);
	}

	[Server]
	Vector3 RandomSpawnPosition() => new Vector3(RandomFloat(), 0, RandomFloat());

	float RandomFloat() => Random.Range(deadZoneSize / 2, spawnAreaSize / 2) * RandomNegative();

	[Server]
	int RandomNegative() => Random.value > 0.5 ? 1 : -1;

	[Server]
	public void Reset() {
		Toolbox.Log("AppleManager Reset");
		all.ForEach(x => Destroy(x.gameObject));
		all.Clear();
	}

	public void SpawnApple(Vector3 position) {
		SpawnApple(new AppleState() { isActive = true, position = position });
	}

	public void SpawnApple(AppleState state) {
		var apple = GetAppleAtPosition(state.position);

		if (apple) {
			apple.gameObject.SetActive(true);
		} else {
			var newApple = Instantiate(applePrefab, state.position, Quaternion.identity, transform);
			newApple.SetActive(state.isActive);
		}
	}

	public void DoTick() {
		throw new System.NotImplementedException();
	}

	public void RollbackTick() {
		throw new System.NotImplementedException();
	}

	public bool IsAliveAppleAtPosition(Vector3 pos) {
		return AppleManager.I.all.Any(x => (x.gameObject.activeSelf && x.transform.position == pos));
	}

	public Apple GetAppleAtPosition(Vector3 pos) {
		return AppleManager.I.all.FirstOrDefault(x => x.transform.position == pos);
	}
}
