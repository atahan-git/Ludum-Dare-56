using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderCam : MonoBehaviour {

    public Camera target;
    void Start() {
        GetComponent<Canvas>().worldCamera = target;
        GetComponent<Canvas>().planeDistance = 6.5f;
    }

}
