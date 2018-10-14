using UnityEngine;

public interface ISnakeController {
    Direction GetDirection();
    Vector3 GetNextHeadPosition();
}
