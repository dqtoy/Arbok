using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeEvents {
    public SortedDictionary<int, SnakeCompoundEvent> dict = new SortedDictionary<int, SnakeCompoundEvent>();

    public void AddOrReplaceAtTick(int tick, SnakeEvent snakeEvent) {
        if (dict.ContainsKey(tick) == false) {
            dict[tick] = new SnakeCompoundEvent();
        }
        dict[tick].AddOrReplaceEvent(snakeEvent);
    }

    public bool HasEventAtTick(int tick) {
        return dict.ContainsKey(tick);
    }

    public void ExecuteEventsAtTickIfAny(int tick, Snake snake) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Execute(snake);
        }
    }

    public void ReverseEventsAtTickIfAny(int tick, Snake snake) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Reverse(snake);
        }
    }

    public override string ToString() {
        String result = "";
        foreach (var kvp1 in dict.Reverse()) {
            result += kvp1.Key + ": ";
            foreach (var kvp2 in kvp1.Value.events) {
                result += kvp2 + " ";
            }
            result += "\n";
        }
        return result;
    }

    public void PurgeTicksAfterTick(int tick) {
        var keysToRemove = new List<int>();

        foreach (var kvp1 in dict) {
            if (kvp1.Key > tick) {
                keysToRemove.Add(kvp1.Key);
            }
        }

        foreach (var key in keysToRemove) {
            dict.Remove(key);
        }
    }
}

public class SnakeCompoundEvent {
    static List<Type> priorityMap = new List<Type> {
        typeof(SnakeDieEvent),
        typeof(SnakeEatAppleEvent),
        typeof(SnakeChangeDirectionEvent),
        typeof(SnakeMoveEvent),
    };

    public SnakeEvent[] events = new SnakeEvent[priorityMap.Count()];

    public void AddOrReplaceEvent(SnakeEvent snakeEvent) {
        events[priorityMap.IndexOf(snakeEvent.GetType())] = snakeEvent;
    }

    public void Execute(Snake snake) {
        foreach (var snakeEvent in events) {
            snakeEvent?.Execute(snake);
        }
    }

    public void Reverse(Snake snake) {
        foreach (var snakeEvent in events.Reverse()) {
            snakeEvent?.Reverse(snake);
        }
    }
}

public interface SnakeEvent {
    void Execute(Snake snake);
    void Reverse(Snake snake);
}

class SnakeMoveEvent : SnakeEvent {
    Vector3 oldTailPosition;

    public void Execute(Snake snake) {
        Vector3 newPosition = snake.head.transform.position + snake.currentDirection.GetMoveVector();

        if (AboutToCollideWithSelf(snake, newPosition)) {
            snake.Die();
        }

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

public class SnakeChangeDirectionEvent : SnakeEvent {
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

public class SnakeEatAppleEvent : SnakeEvent {
    Apple apple;

    public SnakeEatAppleEvent(Apple apple) {
        this.apple = apple;
    }

    public void Execute(Snake snake) {
        apple.gameObject.SetActive(false);
        var newTail = GameObject.Instantiate(snake.snakeTailPrefab, snake.transform);
        snake.links.Add(newTail);
    }

    public void Reverse(Snake snake) {
        var firstTailLink = snake.links.Last();
        snake.links.Remove(firstTailLink);
        GameObject.Destroy(firstTailLink);
        apple.gameObject.SetActive(true);
    }

    public override string ToString() {
        return "E";
    }
}

public class SnakeDieEvent : SnakeEvent {
    public void Execute(Snake snake) {
        snake.isDead = true;
        snake.headVisual.SetActive(false);
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = false);
    }

    public void Reverse(Snake snake) {
        snake.links.ForEach(x => x.GetComponent<MeshRenderer>().enabled = true);
        snake.headVisual.SetActive(true);
        snake.isDead = false;
    }

    public override string ToString() {
        return "D";
    }
}
