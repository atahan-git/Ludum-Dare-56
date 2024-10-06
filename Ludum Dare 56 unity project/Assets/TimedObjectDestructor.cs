using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedObjectDestructor : MonoBehaviour {
    public float timeout = 2f;
    void Start()
    {
        Destroy(gameObject, timeout);
    }
}
