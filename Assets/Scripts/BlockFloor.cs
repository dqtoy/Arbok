using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFloor : MonoBehaviour {
	public static BlockFloor I;

	public GameObject floorBlockPrefab;
	public GameObject killBlockPrefab;
	public int size = 100;
	public float minRandomTorque = 1;
	public float maxRandomTorque = 10;
	public float blockPushAwayForce = 1;
	public Material normalMaterial;
	public Material darkMaterial;

	readonly Vector2[] dropDirections = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

	Vector3 startScale = Vector3.one;
	Vector2 nextDrop;
	int dropDirectionIndex = 0;

	List<GameObject> killBlocks = new List<GameObject>();
	List<Vector2> nextDrops = new List<Vector2>();
	List<int> dropDirectionIndexes = new List<int>();
	List<GameObject> dropBlocks = new List<GameObject>();

	void Awake() {
		I = this;
		transform.localScale = Vector3.one;
		startScale = transform.localScale;
	}

	// Use this for initialization
	void Start() {
		Debug.Log("BlockFloor Start");
		SpawnFloor();
		GlobalTick.OnInitialized += (tick) => {
			Debug.Log("BlockFloor GlobalTick.I.OnInitialized");

			killBlocks.Clear();
			nextDrops.Clear();
			dropDirectionIndexes.Clear();
			dropBlocks.Clear();

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
		GlobalTick.OnRollbackTick += RollbackTick;
	}

	public void StopDropping() {
		GlobalTick.OnDoTick -= DoTick;
		GlobalTick.OnRollbackTick -= RollbackTick;
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

		nextDrop += dropDirections[dropDirectionIndex];
		DropBlock(x);
	}

	void RollbackTick() {
		Debug.Log("RollbackTick");
		GameObject killBlock = killBlocks[killBlocks.Count - 1];
		killBlocks.RemoveAt(killBlocks.Count - 1);

		nextDrops.RemoveAt(nextDrops.Count - 1);
		nextDrop = nextDrops[nextDrops.Count - 1];

		dropDirectionIndexes.RemoveAt(dropDirectionIndexes.Count - 1);
		dropDirectionIndex = dropDirectionIndexes[dropDirectionIndexes.Count - 1];

		GameObject dropBlock = dropBlocks[dropBlocks.Count - 1];
		dropBlock.name = dropBlock.name.Substring(0, dropBlock.name.Length - 9);
		dropBlock.transform.position = new Vector3(killBlock.transform.position.x, -1, killBlock.transform.position.z);
		dropBlock.transform.rotation = Quaternion.identity;
		dropBlocks.RemoveAt(dropBlocks.Count - 1);

		Destroy(killBlock);
	}

	GameObject GetNextBlockToDrop() {
		return GameObject.Find("(" + nextDrop.x.ToString() + "," + nextDrop.y.ToString() + ")");
	}

	void DropBlock(GameObject block) {
		GameObject killBlock = SpawnKillBlock(block.transform.position);
		block.name += " Dropping";

		killBlocks.Add(killBlock);
		nextDrops.Add(nextDrop);
		dropDirectionIndexes.Add(dropDirectionIndex);
		dropBlocks.Add(block);
	}

	GameObject SpawnKillBlock(Vector3 position) {
		GameObject killBlock = Instantiate(killBlockPrefab, new Vector3(position.x, 0, position.z), Quaternion.identity, transform);
		return killBlock;
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
