using System;
using System.Collections.Generic;
using System.Linq;

public class GameEvents<T, U> where T : CompoundGameEvent<U>, new() {
    public SortedDictionary<int, T> dict = new SortedDictionary<int, T>();

    public void AddOrReplaceAtTick(int tick, GameEvent<U> gameEvent) {
        if (dict.ContainsKey(tick) == false) {
            dict[tick] = new T();
        }
        dict[tick].AddOrReplaceEvent(gameEvent);
    }

    public bool HasEventAtTick(int tick) {
        return dict.ContainsKey(tick);
    }

    public void ExecuteEventsAtTickIfAny(int tick, U actor) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Execute(actor);
        }
    }

    public void ReverseEventsAtTickIfAny(int tick, U actor) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Reverse(actor);
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

public abstract class CompoundGameEvent<T> {
    public abstract IList<Type> PriorityMap { get; }

    public GameEvent<T>[] events;

    public CompoundGameEvent() {
        this.events = new GameEvent<T>[PriorityMap.Count()];
    }

    public void AddOrReplaceEvent(GameEvent<T> gameEvent) {
        events[PriorityMap.IndexOf(gameEvent.GetType())] = gameEvent;
    }

    public void Execute(T actor) {
        events.Where(x => x != null).ToList().ForEach(gameEvent => {
            // Toolbox.Log("Executing GameEvent: " + gameEvent.GetType().Name);
            gameEvent.Execute(actor);
        });
    }

    public void Reverse(T actor) {
        events.Reverse().Where(x => x != null).ToList().ForEach(gameEvent => {
            // Toolbox.Log("Reversing GameEvent: " + gameEvent.GetType().Name);
            gameEvent?.Reverse(actor);
        });
    }
}

public interface GameEvent<T> {
    void Execute(T actor);
    void Reverse(T actor);
}
