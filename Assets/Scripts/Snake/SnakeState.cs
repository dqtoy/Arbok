using UnityEngine;

public class SnakeState {
    public Vector3 headPosition;
    public int tick;
    public Direction direction;
    public Vector3[] linkPositions;
    public float elapsedTime;
    public bool isDead;
}
