using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
	Apple SpawnRandomApple() => SpawnApple(new AppleState() { isActive = true, position = RandomSpawnPosition() });

	Vector3 RandomSpawnPosition() => new Vector3(RandomFloat(), 0, RandomFloat());

	float RandomFloat() => Random.Range(deadZoneSize / 2, spawnAreaSize / 2) * RandomNegative();

	int RandomNegative() => Random.value > 0.5 ? 1 : -1;

	public Apple SpawnApple(AppleState state) {
		// Toolbox.Log("AppleManager SpawnApple");
		var newApple = Instantiate(applePrefab, state.position, Quaternion.identity, transform);
		newApple.SetActive(state.isActive);
		return newApple.GetComponent<Apple>();
	}

	public void Reset() {
		Toolbox.Log("AppleManager DestroyAll");
		all.ForEach(x => Destroy(x.gameObject));
		all.Clear();
	}

	public void EnableAll() {
		Toolbox.Log("AppleManager EnableAll");
		all.ForEach(x => x.gameObject.SetActive(true));
	}

	public void DoTick() {
		throw new System.NotImplementedException();
	}

	public void RollbackTick() {
		throw new System.NotImplementedException();
	}
}
