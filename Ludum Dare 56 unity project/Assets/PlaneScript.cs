using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaneScript : MonoBehaviour {
    public static PlaneScript s;

    private void Awake() {
        s = this;
    }

    public Vector3 GetRandomPosOnPlane() {
        var min = 0.1f;
        var max = 9.2f;
        var pos = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));
        if (pos.x + pos.z < 2.4f) {
            if (Random.value < 0.5f) {
                pos.x = 2.4f;
            } else {
                pos.z = 2.4f;
            }
        }

        if ((10 - pos.x) + (10 - pos.z) < 2.7f) {
            if (Random.value < 0.5f) {
                pos.x = 7.3f;
            } else {
                pos.z = 7.3f;
            }
        }
        return pos;
    }
}
