using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeTailContainer : MonoBehaviour
{

    public GameObject snakeTailPrefab;

    public List<SnakeTail> tails { get; private set; }

    private void Awake() {
        tails = new List<SnakeTail>();
    }

    public void GrowForLocalPlayer() {
        var goNewTail = Instantiate(snakeTailPrefab, new Vector3(0, -10, 0), Quaternion.identity, transform);
        SnakeTail newTail = goNewTail.GetComponent<SnakeTail>();
        tails.Add(newTail);
    }

    public void GrowForRemotePlayer(Vector3 newTailPosition)
    {
        var goNewTail = Instantiate(snakeTailPrefab, newTailPosition, Quaternion.identity, transform);
        SnakeTail newTail = goNewTail.GetComponent<SnakeTail>();
        tails.Insert(0, newTail);
    }

    public void Move(Vector3 newPosition) {
        if (tails.Count > 0) {
            var oldTail = tails[tails.Count - 1];
            tails.RemoveAt(tails.Count - 1);
            tails.Insert(0, oldTail);
            oldTail.transform.position = newPosition;
        }
    }

    public void AlignWithHeadPosition(Vector3 headPosition, Direction direction) {
        if(tails.Count == 0) {
            return;
        }

        var firstTailActualPosition = tails[0].transform.position;
        var firstTailExpectedPosition = headPosition - direction.GetMoveVector();
        var tailOffset = firstTailExpectedPosition - firstTailActualPosition;

        foreach(SnakeTail tail in tails) {
            tail.SetPos(tail.transform.position + tailOffset);
        }
    }
}
