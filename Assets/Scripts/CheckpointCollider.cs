using UnityEngine;
using System.Collections;

public class CheckpointCollider : MonoBehaviour {

    public Laps laps;

    void Start() {

    }

    void OnTriggerEnter(Collider other) {
        //Is it the Player who enters the collider?
        if (other.CompareTag("Player")) {
            laps.passedCheckpoint(transform);
        }
    }

}