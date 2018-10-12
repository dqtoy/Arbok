using UnityEngine;

public abstract class ISnakeController : MonoBehaviour
{
    public abstract bool IsUpButtonPressed();
    public abstract bool IsRightButtonPressed();
    public abstract bool IsDownButtonPressed();
    public abstract bool IsLeftButtonPressed();
}
