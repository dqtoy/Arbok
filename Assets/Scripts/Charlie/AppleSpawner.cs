using UnityEngine;
using UnityEngine.Networking;

public class AppleSpawner : NetworkBehaviour{

    public GameObject applePrefab;
    public int numberOfApples;

    public override void OnStartServer() {
        for (int i = 0; i < numberOfApples; i++) {
            var spawnPosition = new Vector3(
                Mathf.Floor(Random.Range(-8.0f, 8.0f)),
                0.0f,
                Mathf.Floor(Random.Range(-8.0f, 8.0f)));

            var apple = (GameObject)Instantiate(applePrefab, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(apple);
        }
    }
}