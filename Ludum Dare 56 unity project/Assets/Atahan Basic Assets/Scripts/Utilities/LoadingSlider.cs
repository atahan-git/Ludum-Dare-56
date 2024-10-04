using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSlider : MonoBehaviour {
    private Slider _slider;

    private void Start() {
        _slider = GetComponent<Slider>();
    }

    void Update() {
        _slider.value = SceneLoader.loadingProgress;
    }
}
