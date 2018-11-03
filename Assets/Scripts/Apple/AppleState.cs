using Newtonsoft.Json;
using UnityEngine;

public class AppleState {
    public int spawnTick;
    public int eatenTick = int.MaxValue;
    public DG_Position position;
    [JsonIgnore]
    public GameObject spawnedApple;
}
