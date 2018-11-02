using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

	public GameEvents<AppleCompoundEvent, AppleManager> appleEvents { get; private set; }

	void Awake() {
		I = this;

		appleEvents = new GameEvents<AppleCompoundEvent, AppleManager>();
		GlobalTick.OnDoTick += DoTick;
		GlobalTick.OnRollbackTick += RollbackTick;
	}

	[Server]
	public void ServerStart() {
		Toolbox.Log("AppleManager ServerStart");

		Reset();

		for (int i = 0; i < startingAppleCount; i++) {
			SpawnRandomApple();
		}
	}

	[Server]
	void SpawnRandomApple() => SpawnApple(new AppleState() { isActive = true, position = RandomSpawnPosition() });

	// TODO Do we need to not destroy it?
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

	public void Reset() {
		Toolbox.Log("AppleManager Reset");
		DestroyAllApples();
	}

	public void DestroyAllApples() {
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
		if (isServer) ServerTick();

		appleEvents.ExecuteEventsAtTickIfAny(GlobalTick.I.currentTick, this);
	}

	[Server]
	void ServerTick() {
		if (GlobalTick.I.currentTick % 10 == 0) {
			var newPos = RandomSpawnPosition();
			var tickToSpawn = GlobalTick.I.currentTick;
			appleEvents.AddOrReplaceAtTick(tickToSpawn, new AppleSpawnEvent(newPos));
			Toolbox.Log("AppleManager ServerTick % 10 " + tickToSpawn + JsonConvert.SerializeObject(newPos));
			RpcReceiveSpawn(tickToSpawn, newPos);
		}
	}

	[ClientRpc]
	void RpcReceiveSpawn(int tick, Vector3 pos) {
		Toolbox.Log("AppleManager RpcReceiveSpawn " + tick + JsonConvert.SerializeObject(pos));
		if (!isServer) {
			Toolbox.Log("AppleManager RpcReceiveSpawn !isServer");
			appleEvents.CorrectEventAtTick(new AppleSpawnEvent(pos), tick);
		}
	}

	public void RollbackTick() {
		appleEvents.ReverseEventsAtTickIfAny(GlobalTick.I.currentTick, this);
	}

	public bool IsAliveAppleAtPosition(Vector3 pos) {
		return AppleManager.I.all.Any(x => (x.gameObject.activeSelf && x.transform.position == pos));
	}

	public Apple GetAppleAtPosition(Vector3 pos) {
		return AppleManager.I.all.FirstOrDefault(x => x.transform.position == pos);
	}
}
