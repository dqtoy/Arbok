using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFloor : MonoBehaviour {
	public static BlockFloor I;
	public GameObject floorBlockPrefab;
	public GameObject killBlockPrefab;
	public int size = 100;
	Vector2 nextDrop;
	int dropDirectionIndex = 0;
	Vector2[] dropDirections = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
	public float minRandomTorque = 1;
	public float maxRandomTorque = 10;
	public float blockPushAwayForce = 1;
	Vector3 startScale;
	public Material normalMaterial;
	public Material darkMaterial;

	void Awake() {
		I = this;
		startScale = transform.localScale;
	}

	// Use this for initialization
	void Start() {
		Debug.Log("BlockFloor Start");
		SpawnFloor();
		GlobalTick.OnInitialized += (tick) => {
			Debug.Log("BlockFloor GlobalTick.I.OnInitialized");
			StartDropping();
			for (int i = 0; i < tick; i++) {
				DoTick();
			}
		};
	}

	// Update is called once per frame
	void Update() {

	}

	void SpawnFloor() {
		nextDrop = new Vector2(0, 0);
		transform.localScale = Vector3.one;

		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				if (x == 0) {
					SpawnKillBlock(new Vector3(-1, 0, y));
				}
				if (x == size - 1) {
					SpawnKillBlock(new Vector3(size, 0, y));
				}
				if (y == 0) {
					SpawnKillBlock(new Vector3(x, 0, -1));
				}
				if (y == size - 1) {
					SpawnKillBlock(new Vector3(x, 0, size));
				}
				var z = Instantiate(floorBlockPrefab, new Vector3(x, -1, y), Quaternion.identity, transform);
				z.name = "(" + x.ToString() + "," + y.ToString() + ")";
				if (x % 2 == 0 && y % 2 == 1) {
					z.GetComponent<MeshRenderer>().material = darkMaterial;
				} else {
					z.GetComponent<MeshRenderer>().material = normalMaterial;
				}
			}
		}

		transform.position = new Vector3(-size / 2 * startScale.x, 0, -size / 2 * startScale.z);

		transform.localScale = startScale;
	}

	public void StartDropping() {
		GlobalTick.OnDoTick += DoTick;
	}

	public void StopDropping() {
		GlobalTick.OnDoTick -= DoTick;
	}

	void DoTick() {
		var x = GetNextBlockToDrop();
		if (x == null) {
			nextDrop -= dropDirections[dropDirectionIndex];
			NextDirection();
			nextDrop += dropDirections[dropDirectionIndex];
			x = GetNextBlockToDrop();
		}
		if (x == null) {
			StopDropping();
			return;
		}
		DropBlock(x);
		nextDrop += dropDirections[dropDirectionIndex];
	}

	GameObject GetNextBlockToDrop() {
		return GameObject.Find("(" + nextDrop.x.ToString() + "," + nextDrop.y.ToString() + ")");
	}

	void DropBlock(GameObject block) {
		SpawnKillBlock(block.transform.position);
		var r = block.AddComponent<Rigidbody>();
		r.AddTorque(GetRandomTorque());
		r.AddForce((r.transform.position - Vector3.zero).normalized * blockPushAwayForce, ForceMode.VelocityChange);
		r.useGravity = false;
		block.name += " Dropping";
	}

	void SpawnKillBlock(Vector3 position) {
		Instantiate(killBlockPrefab, new Vector3(position.x, 0, position.z), Quaternion.identity, transform);
	}

	Vector3 GetRandomTorque() {
		return new Vector3(
			Random.Range(minRandomTorque, maxRandomTorque),
			Random.Range(minRandomTorque, maxRandomTorque),
			Random.Range(minRandomTorque, maxRandomTorque)
		);
	}

	void NextDirection() {
		dropDirectionIndex++;
		if (dropDirectionIndex == dropDirections.Length) {
			dropDirectionIndex = 0;
		}
	}
}
