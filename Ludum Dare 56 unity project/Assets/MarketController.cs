using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketController : MonoBehaviour {
    public static MarketController s;

    public GameObject shopScreen;
    private void Awake() {
        s = this;
    }
    
    public void ShowMarket() {
        Invoke(nameof(_ShowMarket), 1f);
    }

    void _ShowMarket() {
        shopScreen.SetActive(true);
        
    }

    public void HideMarket() {
        shopScreen.SetActive(false);
        GameMaster.s.NextDay();
    }
}
