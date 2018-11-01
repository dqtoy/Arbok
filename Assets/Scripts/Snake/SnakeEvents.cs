using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeCompoundEvent : CompoundGameEvent<Snake> {
    public override IList<Type> PriorityMap => priorityMap;
    static List<Type> priorityMap = new List<Type> {
        typeof(SnakeSpawnEvent),
        typeof(SnakeDieEvent),
        typeof(SnakeEatAppleEvent),
        typeof(SnakeChangeDirectionEvent),
        typeof(SnakeMoveEvent),
    };
}

class SnakeMoveEvent : GameEvent<Snake> {
    Vector3 oldTailPosition;

    public void Execute(Snake snake) {
        Vector3 newPosition = snake.head.transform.position + snake.currentDirection.GetMoveVector();

        if (snake.links.Count > 0) {
            var oldTail = snake.links.Last();
            snake.links.RemoveAt(snake.links.Count - 1);
            snake.links.Insert(0, oldTail);
            oldTailPosition = oldTail.transform.position;
            oldTail.transform.position = snake.head.transform.position;
            oldTail.transform.rotation = Quaternion.Euler(new Vector3(
                oldTail.transform.rotation.eulerAngles.x,
                snake.head.GetRotationOfVisual().eulerAngles.y,
                oldTail.transform.rotation.eulerAngles.z
            ));
        }

        snake.head.transform.position = newPosition;
    }

    private bool AboutToCollideWithSelf(Snake snake, Vector3 newPosition) {
        return snake.links.Any(x => x.transform.position == newPosition);
    }

    public void Reverse(Snake snake) {
        Vector3 oldPosition = snake.head.transform.position - snake.currentDirection.GetMoveVector();

        snake.head.transform.position = oldPosition;

        if (snake.links.Count > 0) {
            var newTail = snake.links[0];
            snake.links.RemoveAt(0);
            snake.links.Add(newTail);
            newTail.transform.position = oldTailPosition;
        }
    }

    public override string ToString() {
        return "M";
    }
}

public class SnakeChangeDirectionEvent : GameEvent<Snake> {
    Direction previousDirection = DummyDirection.I;
    public Direction newDirection { get; private set; }

    public SnakeChangeDirectionEvent(Direction newDirection) {
        this.newDirection = newDirection;
    }

    public void Execute(Snake snake) {
        previousDirection = snake.currentDirection;

        if (invalidTurn(snake.currentDirection, newDirection)) return;

        snake.currentDirection = newDirection;
        snake.head.SetRotationOfVisual(newDirection.GetHeadRotation());
    }

    public void Reverse(Snake snake) {
        snake.head.SetRotationOfVisual(previousDirection.GetHeadRotation());
        snake.currentDirection = previousDirection;
    }

    public override string ToString() {
        return "T-" + newDirection;
    }

    private static bool invalidTurn(Direction currentDir, Direction newDir) {
        if ((newDir == Down.I && currentDir == Up.I) ||
            (newDir == Left.I && currentDir == Right.I) ||
            (newDir == Up.I && currentDir == Down.I) ||
            (newDir == Right.I && currentDir == Left.I)) {

            return true;
        } else {
            return false;
        }
    }
}

public class SnakeEatAppleEvent : GameEvent<Snake> {
    Apple apple;

    public SnakeEatAppleEvent(Apple apple) {
        this.apple = apple;
    }

    public void Execute(Snake snake) {
        apple.gameObject.SetActive(false);
        var newTail = GameObject.Instantiate(snake.snakeTailPrefab, snake.transform);
        snake.links.Add(newTail.GetComponent<SnakeTail>());
    }

    public void Reverse(Snake snake) {
        Debug.Log("SnakeEatAppleEvent Reverse1 snake.links.Count: " + snake.links.Count);
        var firstTailLink = snake.links.Last();
        snake.links.Remove(firstTailLink);
        GameObject.Destroy(firstTailLink.gameObject);
        apple.gameObject.SetActive(true);
        Debug.Log("SnakeEatAppleEvent Reverse2 snake.links.Count: " + snake.links.Count);
    }

    public override string ToString() {
        return "E";
    }
}

public class SnakeSpawnEvent : GameEvent<Snake> {
    public void Execute(Snake snake) {
        snake.isDead = false;
        snake.headVisual.SetActive(true);
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = true);
    }

    public void Reverse(Snake snake) {
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = false);
        snake.headVisual.SetActive(false);
        snake.isDead = true;
    }

    public override string ToString() {
        return "S";
    }
}

public class SnakeDieEvent : GameEvent<Snake> {
    public void Execute(Snake snake) {
        DoExecute(snake);
    }
    public static void DoExecute(Snake snake) {
        snake.isDead = true;
        snake.headVisual.SetActive(false);
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = false);
    }

    public void Reverse(Snake snake) {
        DoReverse(snake);
    }

    public static void DoReverse(Snake snake) {
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = true);
        snake.headVisual.SetActive(true);
        snake.isDead = false;
    }

    public override string ToString() {
        return "D";
    }
}
