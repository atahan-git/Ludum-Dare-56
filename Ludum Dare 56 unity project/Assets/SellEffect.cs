using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SellEffect : MonoBehaviour {


    public GameObject moveUpThing;
    public TMP_Text sellText;
    public void SetUp(int sellPrice) {
        sellText.text = $"+${sellPrice}";
        aliveTime = -0.5f;
    }

    private float aliveTime;
    private void Update() {
        aliveTime += Time.deltaTime;
        if (aliveTime > 0f) {
            aliveTime *= 1.1f;
        }
        moveUpThing.transform.position = Vector3.MoveTowards(moveUpThing.transform.position, GameMaster.s.fundsGoLocation.position, aliveTime*Time.deltaTime);
    }
}
