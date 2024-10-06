using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    public static GameMaster s;

    private void Awake() {
        s = this;
    }

    public int moveNextBlock = 0;
    public int cardActionBlock = 0;
    public Button moveNextButton;

    public TMP_Text creatureCounts;
    public Transform creatureParent;

    public Transform fundsGoLocation;


    public GameObject youWon;
    public GameObject instructions;

    public TMP_Text statsText;
    public Light mainLight;

    public int money = 10;
    public int timeOfDay = 8; // goes from 8 to 20
    public int timeIncrement = 3; // make sure divides 12

    public int currentDay = 1;
    public bool isWon = false;
    public bool isNight = false;
    
    public int blobsMade = 0;
    public int blobsSold = 0;
    public int blobsDied = 0;
    public int cardsPlayed = 0;
    public int decksShuffled = 0;
    public int foodMade = 0;

    public TMP_Text youWonStatsText;

    public bool showValue = false;

    private void Start() {
        money = 10;
        showValue = true;
    }

    private void Update() {
        if (!isWon && money > 5000000) {
            isWon = true;
            youWon.SetActive(true);
            
            youWonStatsText.text = $"It took you {currentDay} days\n" +
                                   $"You made {foodMade} food\n" +
                                   $"You created {blobsMade} blobs\n" +
                                   $"You sold {blobsSold} blobs\n" +
                                   $"In your care {blobsDied} blobs died\n" +
                                   $"You played {cardsPlayed} cards\n" +
                                   $"You shuffled your deck {decksShuffled} times";
        }


        if (isNight) {
            mainLight.colorTemperature = Mathf.Lerp(mainLight.colorTemperature, 20000f, 2f*Time.deltaTime);
        } else {
            mainLight.colorTemperature = Mathf.Lerp(mainLight.colorTemperature, 5000f, 2f*Time.deltaTime);
        }
        
        moveNextButton.interactable = !NextTurnBlocked();

        var hungerCounts = new int[] { 0, 0, 0, 0, 0 };
        
        var allCreatures = GetComponentsInChildren<CreatureScript>();

        for (int i = 0; i < allCreatures.Length; i++) {
            if (allCreatures[i].isAlive) {
                hungerCounts[allCreatures[i].GetVisualHungerLevel()] += 1;
            }
        }

        creatureCounts.text = $"super happy:{hungerCounts[4]}\n" +
                              $"happy:{hungerCounts[3]}\n" +
                              $"normal:{hungerCounts[2]}\n" +
                              $"sad:{hungerCounts[1]}\n" +
                              $"<color=red>starving:{hungerCounts[0]}";
        
        statsText.text = $"Funds:${money}\n" +
                              $"Goal:$5 million!\n" +
                              $"Time:{timeOfDay}:00\n" +
                              $"Market at 20:00\n" +
                              $"Day:{currentDay}";
    }

    public void GoToNextStep() {
        if (NextTurnBlocked()) {
            return;
        }

        timeOfDay += timeIncrement;
        
        var allCreatures = GetComponentsInChildren<CreatureScript>();

        for (int i = 0; i < allCreatures.Length; i++) {
            allCreatures[i].GoToNextStep();
        }

        var playerInteractor = GetComponent<PlayerInteractor>();
        playerInteractor.NextStep();

        if (timeOfDay >= 20) {
            OpenMarket();
        }
    }

    public void OpenMarket() {
        moveNextBlock += 1;
        isNight = true;
        MarketController.s.ShowMarket();
        
        var allCreatures = GetComponentsInChildren<CreatureScript>();
        for (int i = 0; i < allCreatures.Length; i++) {
            allCreatures[i].SetSleepState(true);
        }
    }

    public void NextDay() {
        moveNextBlock -= 1;
        currentDay += 1;
        isNight = false;
        timeOfDay = 8;
        
        var allCreatures = GetComponentsInChildren<CreatureScript>();
        for (int i = 0; i < allCreatures.Length; i++) {
            allCreatures[i].SetSleepState(false);
            allCreatures[i].myMultiplier = 1;
        }
    }

    public bool NextTurnBlocked() {
        return moveNextBlock > 0;
    }
    
    public bool CardActionsBlocked() {
        return cardActionBlock > 0;
    }

    public void ShowTutorial() {
        instructions.SetActive(true);
    }

    public void HideTutorial() {
        instructions.SetActive(false);
    }

    public void Restart() {
        
    }

    public void HideYouWon() {
        youWon.SetActive(false);
    }

    public Image showValueButton;
    public void ShowValue() {
        showValue = !showValue;
        if (showValue) {
            showValueButton.color = Color.green;
            
        } else {
            showValueButton.color = Color.white;
        }
    }
}
