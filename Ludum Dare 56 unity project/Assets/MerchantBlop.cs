using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MerchantBlop : MonoBehaviour {
    public static MerchantBlop s;

    private void Awake() {
        s = this;
    }

    public CreatureSpriteHolder spriteHolder;
    public Image baseImg;
    public Image faceImg;
    public Image hat;

    public float happyTimer;
    public float angryTimer;
    public float getClickedTimer;
    public float notEnoughMoneyTimer;

    public GameObject notEnoughMoney;
    // card scale 130
    void Update() {
        var index = Mathf.FloorToInt(Time.time*2f) % spriteHolder.baseLayer.Length;
        var sprite = spriteHolder.baseLayer[index];
        baseImg.sprite = sprite;
        baseImg.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.rect.width * 5, sprite.rect.height * 5);
        baseImg.transform.localPosition = index == 0 ? Vector3.zero : new Vector3(0, 2.5f, 0);
        hat.transform.localPosition = index == 0 ? new Vector3(0, 27.5f, 0) : new Vector3(0, 32.5f, 0);

        if (angryTimer > 0) {
            angryTimer -= Time.deltaTime;
            faceImg.sprite = spriteHolder.angry;
        }else if (happyTimer > 0) {
            happyTimer -= Time.deltaTime;
            faceImg.sprite = spriteHolder.hungerSprites[3];
        }else if (getClickedTimer > 0) {
            getClickedTimer -= Time.deltaTime;
            faceImg.sprite = spriteHolder.eating;
        } else {
            faceImg.sprite = spriteHolder.hungerSprites[2];
        }

        sprite = faceImg.sprite;
        faceImg.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.rect.width * 5, sprite.rect.height * 5);
        
        notEnoughMoney.SetActive(notEnoughMoneyTimer>0f);
        notEnoughMoneyTimer -= Time.deltaTime;


        var myPos = transform.localPosition;
        var myPosNormalized = new Vector3((myPos.x+640) / 1280f, (myPos.y+360) / 720f, 0);
        //print($"{myPos} - {myPosNormalized}");
        
        Vector3 mousePos = Input.mousePosition;
        Vector3 normalizedMousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height);
        
        var lookDirection = normalizedMousePos - myPosNormalized;
        //print($"{normalizedMousePos} - {myPosNormalized} - {lookDirection}");
        //lookDirection = Quaternion.Euler(0,-45,0)*lookDirection;

        lookDirection *= 8;
        if (lookDirection.magnitude > 1) {
            lookDirection = lookDirection.normalized;
        }
        var faceTargetPos = new Vector3(0 + lookDirection.x * 18f, 0 + lookDirection.y * 12f, 0);
        //print(lookDirection);
        
        faceImg.transform.localPosition = Vector3.Lerp(faceImg.transform.localPosition, faceTargetPos, 5*Time.deltaTime);
    }


    public void BeHappy() {
        happyTimer = 1f;
    }

    public void NotEnoughMoney() {
        angryTimer = 1f;
        notEnoughMoneyTimer = angryTimer;
    }

    public void GetClicked() {
        getClickedTimer = 0.25f;
    }
}
