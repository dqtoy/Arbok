using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeState {
    public Vector3 headPosition;
    public int tick;
    public Direction direction;
    public IEnumerable<Vector3> linkPositions;
    public float elapsedTime;
    public bool isDead;
    public NetworkInstanceId netId;
}
