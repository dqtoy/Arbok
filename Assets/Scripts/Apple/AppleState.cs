using Newtonsoft.Json;
using UnityEngine;

public class AppleState {
    public int spawnTick;
    public int eatenTick = int.MaxValue;
    public Vector2 position;
    [JsonIgnore]
    public GameObject spawnedApple;
}
