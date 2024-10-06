using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CreatureSpriteHolder : ScriptableObject {
	public Sprite[] baseLayer = new Sprite[2];
	public Sprite[] hungerSprites = new Sprite[5];
	public Sprite angry;
	public Sprite eating;
	public Sprite wantingToMerge;
	public Sprite sleeping;
}
