using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MarketController : MonoBehaviour {
    public static MarketController s;

    public GameObject shopScreen;


    public Transform cardsParent;
    public GameObject shopCard;

    public Transform cardRemoveCardParent;
    public CardInfoScriptable cardRemoveCard;
    
    public List<CardInfoScriptable> legalShopCards = new List<CardInfoScriptable>();
    public List<CardInfoScriptable> cardsToBeUnlocked = new List<CardInfoScriptable>();

    /*public TMP_Text removeCardCostText;
    public int removeCardCost;
    public int cardsRemovedThisNight = 0;*/
    
    public TMP_Text refreshCardCostText;
    public int refreshCardCost;
    public int refreshedThisNight = 0;

    public GameObject upgradeMarketButton;
    public TMP_Text upgradeCostText;
    public int upgradeCost;

    public int shopLevel = 0;

    public TMP_Text fundsText;

    public bool isFirstNight = true;
    private void Awake() {
        s = this;
        isFirstNight = true;
    }
    
    public void ShowMarket() {
        //cardsRemovedThisNight = 0;
        refreshedThisNight = 0;
        if (isFirstNight) {
            RefreshMarket(false);
            isFirstNight = false;
        } else {
            RefreshMarket(true);
        }
        Invoke(nameof(_ShowMarket), 1f);
    }

    void RefreshMarket(bool shuffle) {
        var baseCost = (5 + (shopLevel * shopLevel * 5));
        
        foreach (Transform child in cardsParent) {
            Destroy(child.gameObject);
        }
        
        Destroy(cardRemoveCardParent.GetChild(0));
        Instantiate(shopCard, cardRemoveCardParent).GetComponent<ShopCard>().SetUp(cardRemoveCard, Mathf.CeilToInt(baseCost*Random.Range(0.9f,1.1f)));


        if (shuffle) {
            Shuffle(legalShopCards);
        }

        for (int i = 0; i < 6; i++) {
            Instantiate(shopCard, cardsParent).GetComponent<ShopCard>().SetUp(legalShopCards[i], Mathf.CeilToInt(baseCost*Random.Range(0.9f,1.1f)));
        }

        upgradeCost = baseCost*2;
        upgradeCostText.text = $"${upgradeCost}";
        
        refreshCardCost = baseCost * (refreshedThisNight+1);
        refreshCardCostText.text = $"${refreshCardCost}";
        
        /*removeCardCost = baseCost * (cardsRemovedThisNight+1);
        removeCardCostText.text = $"${removeCardCost}";*/

        if (cardsToBeUnlocked.Count <= 0) {
            upgradeMarketButton.SetActive(false);
        }
    }

    void _ShowMarket() {
        shopScreen.SetActive(true);
    }

    public void BuyRefreshMarket() {
        if (GameMaster.s.money >= upgradeCost) {
            GameMaster.s.money -= upgradeCost;
        } else {
            MerchantBlop.s.NotEnoughMoney();
            return;
        }
        
        refreshedThisNight += 1;
        RefreshMarket(true);
    }

    public void UpgradeMarket() {
        if (GameMaster.s.money >= upgradeCost) {
            GameMaster.s.money -= upgradeCost;
        } else {
            MerchantBlop.s.NotEnoughMoney();
            return;
        }
        
        Shuffle(legalShopCards);

        var cardsToUnlock = 3;
        while (cardsToUnlock > 0) {
            if (cardsToBeUnlocked.Count > 0) {
                var toUnlock = cardsToBeUnlocked[0];
                cardsToBeUnlocked.RemoveAt(0);
                legalShopCards.Insert(0, toUnlock); //new cards will always be immediately visible
            }
            cardsToUnlock -= 1;
        }

        shopLevel += 1;
        
        RefreshMarket(false);
    }

    public void HideMarket() {
        shopScreen.SetActive(false);
        GameMaster.s.NextDay();
    }

    private void Update() {
        fundsText.text = $"${GameMaster.s.money}";
    }


    // Fisher-Yates shuffle algorithm
    public void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
