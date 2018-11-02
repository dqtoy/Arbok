using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AppleCompoundEvent : CompoundGameEvent<AppleManager> {
    public override IList<Type> PriorityMap => priorityMap;

    static List<Type> priorityMap = new List<Type> {
        typeof(AppleSpawnEvent),
    };
}

public class AppleSpawnEvent : GameEvent<AppleManager> {
    Vector3 spawnPosition;

    public AppleSpawnEvent(Vector3 spawnPosition) {
        this.spawnPosition = spawnPosition;
    }

    public void Execute(AppleManager appleManager) {
        // Toolbox.Log("AppleSpawnEvent Execute");
        appleManager.SpawnApple(spawnPosition);
    }

    public void Reverse(AppleManager appleManager) {
        // Toolbox.Log("AppleSpawnEvent Reverse");
        appleManager.DeSpawnApple(spawnPosition);
    }

    public override string ToString() {
        return "S";
    }
}
