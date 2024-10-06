using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardInfoScriptable : ScriptableObject {
	public string title;
	public Sprite mainSprite;
	[Multiline]
	public string description;
	
	public int manaCost = 1;
	public PlayerInteractor.CardType myType;
	public PlayerInteractor.CardGraphicBase myBase;
	public bool once = false;
}
