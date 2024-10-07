using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CreatureScript : MonoBehaviour {
    private NavMeshAgent _agent;

    public GameObject mergeResult;
    public int size = 1;
    public int satiationLossPerStep = 1;
    public int blopSellPrice = 10;

    public GameObject blopSoldEffect;
    public GameObject spriteBase;
    public CreatureSpriteHolder spriteHolder;
    public SpriteRenderer baseSprite;
    public SpriteRenderer faceSprite;
    public GameObject deathEffect;
    public GameObject hungerEffect;

    public float myMultiplier = 1f;
    public TMP_Text multiplierText;
    
    public TMP_Text valueText;
    void Start() {
        _agent = GetComponent<NavMeshAgent>();
        state = State.chilling;
        chillTime = Random.Range(0, 1f);
        animTime = Random.Range(0, 10000f);
    }

    public enum State {
        chilling, walking, walkingToFood, eating, findingMergeTarget, walkingToMerge, merging
    }

    public State state;

    public Vector3 destination;
    public bool reachedDestination;
    public float chillTime;
    public float walkTime;
    public int satiationLevel = 3; // goes between 0-max satiation
    public int maxSatiation => 3 + satiationLossPerStep;
    public bool isAlive = true;
    public float animTime;
    public Vector3 lookTarget;
    public float angry = 0;

    [SerializeField]
    private float eatPercent = 0;
    [SerializeField]
    private Vector3 eatStartPos;
    [SerializeField]
    private FoodScript potentialYummy;
    [SerializeField]
    private GameObject potentialMerge;

    public bool isSleeping = false;
    public int eatBlock =0;

    public enum BlopType {
        red=0, blue=1, white=2, green=3, purple=4, cat=5, mana=6
    }

    public BlopType myType;
    
    public void SetSleepState(bool _isSleeping) {
        isSleeping = _isSleeping;
        _agent.enabled = !isSleeping;
    }
    void Update() {
        if (!isAlive) {
            return;
        }

        if (isSleeping) {
            return;
        }

        if (myType == BlopType.cat) {
            eatBlock = 10;
        }

        satiationLevel = Mathf.Clamp(satiationLevel, 0, 3 + satiationLossPerStep);
        
        multiplierText.text = $"x{myMultiplier:F2}";
        multiplierText.gameObject.SetActive(GameMaster.s.showValue && Math.Abs(myMultiplier - 1) > 0.05f);
        valueText.text = $"${blopSellPrice}";
        valueText.gameObject.SetActive(GameMaster.s.showValue);

        reachedDestination = ReachedDestination();

        var animTimeMultiplier = 0;
        switch (state) {
            case State.chilling: {
                animTimeMultiplier = 1;
                if (chillTime > 0) {
                    chillTime -= Time.deltaTime;
                } else {
                    WalkToLocation(PlaneScript.s.GetRandomPosOnPlane());
                }
                break;
            }
            case State.walking: {
                animTimeMultiplier = 2;
                
                walkTime += Time.deltaTime;
                if (reachedDestination || walkTime > 3) {
                    state = State.chilling;
                    chillTime = Random.Range(7, 10);
                }
                break;
            }
            case State.walkingToFood: {
                animTimeMultiplier = 2;
                
                if (potentialYummy != null && potentialYummy.gameObject != null) {
                    var foodAdjustedPos = potentialYummy.transform.position;
                    foodAdjustedPos.y = 0;
                    var selfAdjustedPos = transform.position;
                    selfAdjustedPos.y = 0;
                    if (Vector3.Distance(selfAdjustedPos, foodAdjustedPos) < 0.9f) {
                        if (potentialYummy.isTaken) {
                            potentialYummy = null;
                            state = State.chilling;
                            angry = Random.Range(1,2f);
                            _agent.SetDestination(transform.position);
                        } else {
                            potentialYummy.isTaken = true;
                            Destroy(potentialYummy.GetComponent<Rigidbody>());
                            state = State.eating;
                            GameMaster.s.moveNextBlock += 1;
                            eatBlock += 1;
                            _agent.SetDestination(transform.position);
                            eatPercent = -0.5f;
                            eatStartPos = potentialYummy.transform.position;
                        }
                    } else if (reachedDestination) {
                        potentialYummy = null;
                        state = State.chilling;
                        angry = Random.Range(1,2f);
                        _agent.SetDestination(transform.position);
                    }
                } else {
                    potentialYummy = null;
                    angry = Random.Range(1,2f);
                    state = State.chilling;
                    _agent.SetDestination(transform.position);
                }
                break;
            }
            case State.eating: {
                animTimeMultiplier = 0;
                
                eatPercent += Time.deltaTime;
                var realEatPercent = Mathf.Clamp01(eatPercent);
                var yummyTargetPos = Vector3.Lerp(eatStartPos, transform.position, realEatPercent);
                yummyTargetPos.y = Mathf.Sin(realEatPercent * Mathf.PI) + eatStartPos.y*(1-eatPercent);
                potentialYummy.transform.position = yummyTargetPos;
                if (realEatPercent>=1) {
                    FinishEating();
                }
                break;
            }
            case State.findingMergeTarget: {
                animTimeMultiplier = 6;
                
                if (chillTime > 0) {
                    chillTime -= Time.deltaTime;
                } else {
                    var allCreatures = GameMaster.s.GetComponentsInChildren<CreatureScript>();

                    CreatureScript closestCreature = null;
                    float closestDist = float.MaxValue;
                    for (int i = 0; i < allCreatures.Length; i++) {
                        var curCreature = allCreatures[i];
                        if (curCreature == this) {
                            continue;
                        }

                        var dist = Vector3.Distance(transform.position, curCreature.transform.position);
                        if (dist < closestDist) {
                            if (curCreature.size == size) {
                                closestDist = dist;
                                closestCreature = curCreature;
                            }
                        }
                    }

                    if (closestCreature != null) {
                        potentialMerge = closestCreature.gameObject;
                        WalkToMerge(potentialMerge.transform.position);
                    }
                }

                break;
            }
            case State.walkingToMerge: {
                animTimeMultiplier = 6;
                
                if (potentialMerge != null && potentialMerge.gameObject != null) {
                    var foodAdjustedPos = potentialMerge.transform.position;
                    foodAdjustedPos.y = 0;
                    var selfAdjustedPos = transform.position;
                    selfAdjustedPos.y = 0;
                    if (Vector3.Distance(selfAdjustedPos, foodAdjustedPos) < 0.9f) {
                        var creature = potentialMerge.GetComponent<CreatureScript>();
                        if (creature == null || creature.eatBlock > 0) {
                            potentialMerge = null;
                            state = State.findingMergeTarget;
                            angry = Random.Range(1,2f);
                            chillTime = angry;
                            _agent.SetDestination(transform.position);
                        } else {
                            if (myType == BlopType.blue) {
                                blopSellPrice += potentialMerge.GetComponent<CreatureScript>().blopSellPrice;
                            }
                            
                            Destroy(potentialMerge.GetComponent<Rigidbody>());
                            Destroy(potentialMerge.GetComponent<CreatureScript>());
                            Destroy(potentialMerge.GetComponent<NavMeshAgent>());
                            state = State.merging;
                            GameMaster.s.moveNextBlock += 1;
                            eatBlock += 1;
                            _agent.SetDestination(transform.position);
                            eatPercent = -0.5f;
                            eatStartPos = potentialMerge.transform.position;
                        }
                    }else if (reachedDestination) {
                        potentialMerge = null;
                        state = State.findingMergeTarget;
                        angry = Random.Range(1,2f);
                        chillTime = angry;
                        _agent.SetDestination(transform.position);
                    }
                } else {
                    potentialMerge = null;
                    angry = Random.Range(1,2f);
                    chillTime = angry;
                    state = State.findingMergeTarget;
                    _agent.SetDestination(transform.position);
                }
                break;
            }
            case State.merging: {
                animTimeMultiplier = 0;
                
                eatPercent += Time.deltaTime;
                var realEatPercent = Mathf.Clamp01(eatPercent);
                var yummyTargetPos = Vector3.Lerp(eatStartPos, transform.position, realEatPercent);
                yummyTargetPos.y = Mathf.Sin(realEatPercent * Mathf.PI) + eatStartPos.y*(1-eatPercent);
                potentialMerge.transform.position = yummyTargetPos;
                if (realEatPercent>=1) {
                    Instantiate(deathEffect, transform.position, transform.rotation);
                    var result = Instantiate(mergeResult, transform.position, transform.rotation, transform.parent);
                    result.GetComponent<CreatureScript>().satiationLevel = satiationLevel;
                    result.GetComponent<CreatureScript>().myMultiplier = myMultiplier;
                    if (myType == BlopType.blue) {
                        result.GetComponent<CreatureScript>().blopSellPrice = blopSellPrice*2;
                    }
                    GameMaster.s.blobsMade += 1;
                    
                    Destroy(gameObject);
                    Destroy(potentialMerge.gameObject);
                    potentialMerge = null;

                    GameMaster.s.moveNextBlock -= 1;
                    eatBlock -= 1;
                }

                break;
            }
            default: {
                state = State.chilling;
                break;
            }
        }

        Debug.DrawLine(destination, destination + Vector3.up, Color.green);
        Debug.DrawLine(transform.position, destination, Color.cyan);
        spriteBase.transform.rotation = Quaternion.Euler(30, 45, 0);


        animTime += Time.deltaTime * animTimeMultiplier * 2;
        baseSprite.sprite = spriteHolder.baseLayer[Mathf.FloorToInt(animTime) % spriteHolder.baseLayer.Length];
        
        if (state == State.eating)  {
            faceSprite.sprite = spriteHolder.eating;
        } else if (angry > 0) {
            faceSprite.sprite = spriteHolder.angry;
            angry -= Time.deltaTime;
        }else if (state == State.findingMergeTarget || state == State.walkingToMerge || state == State.merging) {
            faceSprite.sprite = spriteHolder.wantingToMerge;
        }else{
            faceSprite.sprite = spriteHolder.hungerSprites[GetVisualHungerLevel() ];
        }

        var lookZ = 0f;
        if (satiationLevel == 1) {
            lookZ = Mathf.Sin(Time.time*5) * 0.1f;
        }else if (satiationLevel == 0) {
            lookZ = Mathf.Sin(Time.time*10) * 0.2f;
        }
        //lookTarget = new Vector3(5,0,5);
        var lookDirection = lookTarget - transform.position;
        lookDirection.y = 0;
        lookDirection = Quaternion.Euler(0,-45,0)*lookDirection;

        if (lookDirection.magnitude > 1) {
            lookDirection = lookDirection.normalized;
        }
        var faceTargetPos = new Vector3(0.12f + lookDirection.x * 0.2f, 0 + lookDirection.z * 0.1f, 0);
        /*if (state == State.chilling) {
            faceTargetPos = new Vector3(0.12f, 0, 0);
        }*/
        faceSprite.transform.localPosition = Vector3.Lerp(faceSprite.transform.localPosition, faceTargetPos, 5*Time.deltaTime);
        faceSprite.transform.localRotation = Quaternion.Euler(0,0,lookZ);
    }

    public bool isDrugged = false;
    private void FinishEating() {
        var prevSatLevel = satiationLevel;
        switch (potentialYummy.myType) {
            case FoodScript.FoodType.food: {
                satiationLevel += 1;
                state = State.chilling;
                break;
            }
            case FoodScript.FoodType.merge: {
                state = State.findingMergeTarget;
                chillTime = 0.5f;
                break;
            }
            case FoodScript.FoodType.sell: {
                GameMaster.s.blobsSold += 1;
                var price = Mathf.CeilToInt(blopSellPrice * myMultiplier);
                GameMaster.s.money += price;
                Instantiate(blopSoldEffect, transform.position, Quaternion.identity).GetComponent<SellEffect>().SetUp(price);
                Destroy(gameObject);
                break;
            }
            case FoodScript.FoodType.butcher: {
                GameMaster.s.blobsDied += 1;
                for (int i = 0; i < 3; i++) {
                    Instantiate(PlayerInteractor.s.blopChunksToken, transform.position+Vector3.up*((i+1)*0.05f), Quaternion.identity);
                }
                Instantiate(deathEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
                break;
            }
            case FoodScript.FoodType.carrot:{
                satiationLevel += 1;
                state = State.chilling;
                myMultiplier *= 1.5f;
                break;
            }
            case FoodScript.FoodType.bloodMoney:
                GameMaster.s.blobsDied += 1;
                Instantiate(deathEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
                GameMaster.s.money *= 2;
                break;
            case FoodScript.FoodType.marketing:
                blopSellPrice += 10;
                state = State.chilling;
                break;
            case FoodScript.FoodType.berry:
                satiationLevel += 1;
                state = State.chilling;
                myMultiplier *= 2f;
                break;
            case FoodScript.FoodType.tripleBurger: {
                var result = Instantiate(mergeResult, transform.position, transform.rotation, transform.parent);
                result.GetComponent<CreatureScript>().satiationLevel = 1000;
                result.GetComponent<CreatureScript>().myMultiplier = myMultiplier;
                GameMaster.s.blobsMade += 1;
                Destroy(gameObject);
                break;
            }
            case FoodScript.FoodType.steak:
                satiationLevel += 2;
                state = State.chilling;
                myMultiplier *= 3f;
                break;
            case FoodScript.FoodType.potato:
                satiationLevel += 2;
                state = State.chilling;
                myMultiplier *= 2f;
                break;
            case FoodScript.FoodType.drugs:
                isDrugged = true;
                satiationLevel = 10000;
                state = State.chilling;
                break;
            case FoodScript.FoodType.hormones:
                if (Random.value < 0.5f) {
                    myMultiplier *= 10f;
                    state = State.chilling;
                } else {
                    Die();
                }
                break;
            case FoodScript.FoodType.mitosis: {
                satiationLevel = 0;
                var result = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
                result.GetComponent<CreatureScript>().potentialYummy = null;
                result.GetComponent<CreatureScript>().state = State.chilling;
                result.GetComponent<CreatureScript>().eatBlock -= 1;
                GameMaster.s.blobsMade += 1;
                state = State.chilling;
                break;
            }
            case FoodScript.FoodType.transmute: {
                var random = Random.Range(0, 7);
                GameObject result;
                switch (random) {
                    case 0:
                    default:
                        result = Instantiate(PlayerInteractor.s.redBlob, transform.position, transform.rotation, transform.parent);
                        break;
                    case 1:
                        result = Instantiate(PlayerInteractor.s.blueBlop, transform.position, transform.rotation, transform.parent);
                        break;
                    case 2:
                        result = Instantiate(PlayerInteractor.s.greenBlop, transform.position, transform.rotation, transform.parent);
                        break;
                    case 3:
                        result = Instantiate(PlayerInteractor.s.whiteBlop, transform.position, transform.rotation, transform.parent);
                        break;
                    case 4:
                        result = Instantiate(PlayerInteractor.s.catBlop, transform.position, transform.rotation, transform.parent);
                        break;
                    case 5:
                        result = Instantiate(PlayerInteractor.s.manaBlop, transform.position, transform.rotation, transform.parent);
                        break;
                    case 6:
                        result = Instantiate(PlayerInteractor.s.purpleBlop, transform.position, transform.rotation, transform.parent);
                        break;
                }
                
                result.GetComponent<CreatureScript>().satiationLevel = satiationLevel;
                result.GetComponent<CreatureScript>().myMultiplier = myMultiplier;
                Destroy(gameObject);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (myType == BlopType.purple) {
            if (satiationLevel > prevSatLevel) {
                if (GetVisualHungerLevel() > 3) {
                    var maxSat = 3 + satiationLossPerStep;
                    
                    satiationLevel = maxSat - 1;
                    
                    var result = Instantiate(gameObject, transform.position, transform.rotation, transform.parent);
                    result.GetComponent<CreatureScript>().potentialYummy = null;
                    result.GetComponent<CreatureScript>().state = State.chilling;
                    result.GetComponent<CreatureScript>().eatBlock -= 1;
                    GameMaster.s.blobsMade += 1;
                }
            }
        }
        
        Destroy(potentialYummy.gameObject);
        potentialYummy = null;
        GameMaster.s.moveNextBlock -= 1;
        eatBlock -= 1;
    }


    // always 0-4
    public int GetVisualHungerLevel() {
        return Mathf.Clamp(satiationLevel - satiationLossPerStep + 1, 0, 4);
    }

    public void GoToNextStep() {
        if (myType == BlopType.green) {
            satiationLevel = 10000;
            for (int i = 0; i < Mathf.Pow(size,2); i++) {
                Instantiate(PlayerInteractor.s.foodToken, transform.position, transform.rotation);
            }

        } else {
            satiationLevel -= satiationLossPerStep;

            if (satiationLevel < 0) {
                Die();
            } else {
                for (int i = 0; i < satiationLossPerStep; i++) {
                    Instantiate(hungerEffect, transform.position, transform.rotation);
                }
            }

            if (isDrugged) {
                satiationLevel = 0;
            }
        }
    }

    void Die() {
        isAlive = false;
        Instantiate(deathEffect, transform.position, transform.rotation);
        GameMaster.s.blobsDied += 1;
        Destroy(gameObject);
    }

    void _Walk(Vector3 location) {
        destination = location;
        _agent.SetDestination(destination);
        walkTime = 0;
        lookTarget = destination;
    }
    public void WalkToLocation(Vector3 location) {
        _Walk(location);
        state = State.walking;
    }
    void WalkToFood(Vector3 location) {
        _Walk(location);
        state = State.walkingToFood;
    }
    
    void WalkToMerge(Vector3 location) {
        _Walk(location);
        state = State.walkingToMerge;
    }
    
    public void SawFood(FoodScript foodScript) {
        if (!enabled) {
            return;
        }

        if (!(state == State.chilling || state == State.walking)) {
            return;
        }
        
        var canEat = true;
        switch (foodScript.myType) {
            case FoodScript.FoodType.carrot:
            case FoodScript.FoodType.berry:
            case FoodScript.FoodType.steak:
            case FoodScript.FoodType.potato:
            case FoodScript.FoodType.drugs:
            case FoodScript.FoodType.food: {
                if (satiationLevel >= maxSatiation) {
                    canEat = false;
                }
                break;
            }
            case FoodScript.FoodType.merge: {
                if (satiationLevel <= 0 || mergeResult == null) {
                    canEat = false;
                }
                break;
            }
            case FoodScript.FoodType.sell:
            case FoodScript.FoodType.butcher:
            case FoodScript.FoodType.bloodMoney:
            case FoodScript.FoodType.hormones:
            case FoodScript.FoodType.mitosis:
            case FoodScript.FoodType.transmute:
            case FoodScript.FoodType.marketing:
                // can always do these
                break;
            case FoodScript.FoodType.tripleBurger:
                if (mergeResult == null) {
                    canEat = false;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        
        if(canEat){
            if (potentialYummy == null) {
                potentialYummy = foodScript;
                WalkToFood(potentialYummy.transform.position);
            } 
        }
    }

    bool ReachedDestination() {
        // Check if we've reached the destination
        if (!_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f) {
                    return true;
                }
            }
        }

        return false;
    }
}
