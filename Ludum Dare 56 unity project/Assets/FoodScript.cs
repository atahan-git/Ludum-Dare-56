using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodScript : MonoBehaviour
{
    public GameObject spriteBase;

    public bool isTaken = false;
    
    public enum FoodType {
        food=0, merge=1,sell=2
    }

    public FoodType myType;
    void Update()
    {
        spriteBase.transform.rotation = Quaternion.Euler(30, 45, 0);
    }

    private void OnTriggerStay(Collider other) {
        if (enabled) {
            var creature = other.GetComponentInParent<CreatureScript>();
            if (creature != null) {
                creature.SawFood(this);
            }
        }
    }
}
