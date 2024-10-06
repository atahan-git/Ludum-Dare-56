using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardScript : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text manaText;
    public CardInfoScriptable myInfo;
    
    public void SetUp(CardInfoScriptable scriptable) {
        myInfo = scriptable;
        spriteRenderer.sprite = myInfo.mainSprite;
        titleText.text = myInfo.title;
        descriptionText.text = myInfo.description;
        manaText.text = $"{myInfo.manaCost}";

        transform.localPosition = new Vector3(10, 1, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
    }
}
