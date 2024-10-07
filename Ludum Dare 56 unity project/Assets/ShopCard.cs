using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : MonoBehaviour {

	public Transform cardParent;
	public TMP_Text costText;
	public GameObject myCard;

	public CardInfoScriptable _card;
	public int _cost;

	public Button myButton;
	
	public void SetUp(CardInfoScriptable card, int cost) {
		_card = card;
		_cost = cost;
		if (myCard != null) {
			Destroy(myCard);
		}
		
		myCard = PlayerInteractor.s.MakeCard(_card, cardParent).gameObject;
		myCard.transform.localPosition = Vector3.zero;
		myCard.transform.localRotation = Quaternion.Euler(-90,0,0);
		myCard.transform.localScale = Vector3.one * 120;

		costText.text = $"${_cost}";
	}

	public void BuyCard() {
		if (GameMaster.s.money >= _cost) {
			GameMaster.s.money -= _cost;
		} else {
			MerchantBlop.s.NotEnoughMoney();
			return;
		}
		MerchantBlop.s.BeHappy();

		myButton.interactable = false;
		myCard.SetActive(false);
		costText.gameObject.SetActive(false);
		
		PlayerInteractor.s.deck.Add(_card);
	}
}
