using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleManager : MonoBehaviour {
	public static AppleManager I;

	public List<Apple> all = new List<Apple>();
	public GameObject applePrefab;
	public int startingAppleCount;
	public int spawnAreaSize;

	void Awake() {
		I = this;
	}

	void Update() {

	}

	// Server only
	public void SpawnStartingApples() {
		Toolbox.Log("AppleManager SpawnStartingApples");
		DestroyAll();
		for (int i = 0; i < startingAppleCount; i++) {
			SpawnRandomApple();
		}
	}

	public void DestroyAll() {
		Toolbox.Log("AppleManager DestroyAll");
		all.ForEach(x => Destroy(x.gameObject));
		all.Clear();
	}

	public void EnableAll() {
		Toolbox.Log("AppleManager EnableAll");
		all.ForEach(x => x.gameObject.SetActive(true));
	}

	Apple SpawnRandomApple() {
		return SpawnApple(new AppleState() { isActive = true, position = GetRandomAppleSpawnPosition() });
	}

	Vector3 GetRandomAppleSpawnPosition() {
		return new Vector3(Random.Range(spawnAreaSize / -2, spawnAreaSize / 2), 0, Random.Range(spawnAreaSize / -2, spawnAreaSize / 2));
	}

	public Apple SpawnApple(AppleState state) {
		// Toolbox.Log("AppleManager SpawnApple");
		var newApple = Instantiate(applePrefab, state.position, Quaternion.identity, transform);
		newApple.SetActive(state.isActive);
		return newApple.GetComponent<Apple>();
	}
}
