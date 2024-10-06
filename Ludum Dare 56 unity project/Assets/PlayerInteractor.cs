using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PlayerInteractor : MonoBehaviour {
    public static PlayerInteractor s;

    private void Awake() {
        s = this;
    }

    public GameObject food;
    public GameObject mergeToken;
    public GameObject sellToken;
    public GameObject redBlob;
    
    
    public GameObject clickEffect;
    
    
    public LayerMask table;
    public LayerMask cards;
    
    public enum CardType {
        makeFoodBasic=0, mergeBlobsBasic=1, createRedBlobBasic=2, sellBlobBasic=3, drawCardBasic=4
    }
    
    public enum CardGraphicBase {
        food=0, utility=1, special=2
    }

    public Transform cardsParent;
    public GameObject[] cardPrefabs;

    public CardInfoScriptable[] startingHand;

    public int startDraw = 5;
    public int perStepDraw = 2;
    
    public List<CardInfoScriptable> deck = new List<CardInfoScriptable>();
    public List<CardInfoScriptable> discard = new List<CardInfoScriptable>();
    public List<CardScript> currentCards = new List<CardScript>();
    private Camera mainCam;
    private CardTargetShower _targetShower;

    public TMP_Text deckCountText;
    public TMP_Text discardCountText;
    
    private void Start() {
        deck.AddRange(startingHand);
        for (int i = 0; i < startDraw; i++) {
            DrawCardFromDeck();
        }

        mainCam = Camera.main;
        _targetShower = GetComponentInChildren<CardTargetShower>();
    }

    public int drawRemaining = 0;
    public void DrawCardFromDeck() {
        if (deck.Count > 0) {
            var toDraw = deck[0];
            deck.RemoveAt(0);
            AddCard(toDraw);
        } else {
            if (discard.Count > drawRemaining) {
                drawRemaining += 1;
            }

            if (discard.Count > 0) {
                if (!isReshuffling) {
                    isReshuffling = true;
                    StartCoroutine(_ReShuffle());
                }
            }
        }
    }

    private bool isReshuffling = false;
    IEnumerator _ReShuffle() {
        GameMaster.s.moveNextBlock += 1;
        GameMaster.s.cardActionBlock += 1;
        isReshuffling = true;

        var totalShuffleTime = 1f;
        var waitPerCard = totalShuffleTime / discard.Count;
        var curWaitTime = 0f;
        Shuffle(discard);
        while (discard.Count > 0) {
            var card = discard[0];
            discard.RemoveAt(0);
            deck.Add(card);

            curWaitTime += waitPerCard;
            if (curWaitTime > 0.1f) {
                yield return new WaitForSeconds(curWaitTime);
                curWaitTime = 0;
            }
        }


        yield return null;

        isReshuffling = false;
        for (int i = 0; i < drawRemaining; i++) {
            DrawCardFromDeck();
        }

        drawRemaining = 0;
        GameMaster.s.moveNextBlock -= 1;
        GameMaster.s.cardActionBlock -= 1;
    }


    public void AddCard(CardInfoScriptable cardInfoScriptable) {
        GameMaster.s.moveNextBlock += 1;
        addCardList.Add(cardInfoScriptable);
    }

    void _AddCard(CardInfoScriptable cardInfoScriptable) {
        GameMaster.s.moveNextBlock -= 1;
        var prefab = cardPrefabs[(int)cardInfoScriptable.myBase];
        var newCard = Instantiate(prefab, cardsParent).GetComponent<CardScript>();
        newCard.SetUp(cardInfoScriptable);
        currentCards.Add(newCard);
    }

    public List<CardInfoScriptable> addCardList = new List<CardInfoScriptable>();

    public int curIndex = -1;
    public bool isDragging;
    public bool nearCards;
    public float addCardTimer = 0;
    public Vector3 lastCastPos;
    public bool castingLegalPosition;
    
    void Update() {
        deckCountText.text = $"{deck.Count}";
        discardCountText.text = $"{discard.Count}";
        Vector3 mousePos = Input.mousePosition;
        Vector3 normalizedMousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height);
        
        if (addCardTimer <= 0) {
            if (!GameMaster.s.CardActionsBlocked() && addCardList.Count > 0) {
                var toAdd = addCardList[0];
                addCardList.RemoveAt(0);
                _AddCard(toAdd);
                addCardTimer = 0.2f;
            }
        } else {
            addCardTimer -= Time.deltaTime;
        }
        
        if (isDragging) {
            isDragging = Input.GetMouseButton(0);
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            RaycastHit hit;

            var currentCard = currentCards[curIndex];
            //cardTip=currentCard.transform.TransformPoint(cardTip);
                
            if (normalizedMousePos.magnitude < 0.33f || GameMaster.s.CardActionsBlocked()) {
                _targetShower.Stop();
                castingLegalPosition = false;
            } else {
                if (Physics.Raycast(ray, out hit, 100, table)) {
                    lastCastPos = hit.point;
                    _targetShower.ShowTarget(currentCard.transform, hit.point);
                    castingLegalPosition = true;
                }
            }
            


            if (!isDragging) {
                if (!Input.GetMouseButton(1)) {
                    if (castingLegalPosition) {
                        _targetShower.StopImmediately();
                        CastCardAtLocation(currentCard, currentCard.myInfo, lastCastPos);
                        nearCards = false;
                    }
                }
            }

        } else {
            castingLegalPosition = false;
            _targetShower.Stop();
            var mouseAngleAdjusted = 1-Mathf.Clamp01((GetMouseAngle() - 10) / 65);
            curIndex = Mathf.Clamp(Mathf.RoundToInt(mouseAngleAdjusted * currentCards.Count), 0, currentCards.Count-1);


            if (Input.GetMouseButtonDown(0)) {
                Ray ray = mainCam.ScreenPointToRay(mousePos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, table)) {
                    Instantiate(clickEffect, hit.point, Quaternion.identity);

                    var creatures = Physics.OverlapSphere(hit.point, 2f);
                    for (int i = 0; i < creatures.Length; i++) {
                        var creature = creatures[i].GetComponentInParent<CreatureScript>();
                        if (creature != null) {
                            if (creature.state == CreatureScript.State.chilling || creature.state == CreatureScript.State.walking) {
                                creature.WalkToLocation(hit.point);
                            }
                        }
                    }
                }
            }
        }
        
        
        if (!isDragging) { //near cards
            if (nearCards) {
                if (normalizedMousePos.magnitude > 0.58f) {
                    nearCards = false;
                }
            } else {
                if (normalizedMousePos.magnitude < 0.33f) {
                    nearCards = true;
                }
            }

            if (nearCards) {
                if (curIndex >= 0) {
                    isDragging = Input.GetMouseButton(0);
                }
            }
        }

        var cardParentTargetScale = nearCards ? 1f : 0.7f;
        
        cardsParent.transform.localScale = Vector3.Lerp(cardsParent.transform.localScale, Vector3.one*cardParentTargetScale, 10*Time.deltaTime);

        var cardCount = currentCards.Count;
        Vector3 cardBasePos = new Vector3(0.29f, 0.136f,0.01f);
        Quaternion cardBaseRot = Quaternion.Euler(-117.81f, -94.01f, 76.907f);
        
        for (int i = 0; i <cardCount; i++) {
            var targetPos = cardBasePos;
            var targetScale = Vector3.one;
            var targetRotation = cardBaseRot;
            if (nearCards && i == curIndex) {
                targetScale *= 1.25f;
                targetPos += new Vector3(0.5f, 1.25f, 0);
            }

            if (cardCount > 1) {
                var axis = Vector3.forward;
                var effectiveCardCount = cardCount;
                if (cardCount <= 2) {
                    effectiveCardCount += 1;
                }
                var posIndex = (effectiveCardCount - i)-1;
                var angle = (posIndex / (effectiveCardCount - 1f)) * 55f - 25f;
                targetPos = RotatePositionAroundPoint(targetPos, axis, angle);
                targetRotation = RotateQuaternionAroundPoint(targetRotation, axis, angle);
            }
            
            
            currentCards[i].transform.localScale = Vector3.Lerp(currentCards[i].transform.localScale, targetScale, 10 * Time.deltaTime);
            currentCards[i].transform.localPosition = Vector3.Lerp(currentCards[i].transform.localPosition, targetPos, 10 * Time.deltaTime);
            currentCards[i].transform.localRotation = Quaternion.Slerp(currentCards[i].transform.localRotation, targetRotation, 60*Time.deltaTime);
        }
    }
    
    Vector3 RotatePositionAroundPoint(Vector3 position, Vector3 axis, float angle)
    {
        return Quaternion.AngleAxis(angle, axis) * position;
    }
    
    Quaternion RotateQuaternionAroundPoint(Quaternion rotation, Vector3 axis, float angle) {
        return Quaternion.AngleAxis(angle, axis) * rotation;
    }


    void CastCardAtLocation(CardScript card, CardInfoScriptable info, Vector3 location) {
        StartCoroutine(_CastCardAtLocation(card, info, location));
    }
    
    IEnumerator _CastCardAtLocation(CardScript card, CardInfoScriptable info, Vector3 location) {
        GameMaster.s.moveNextBlock += 1;
        currentCards.Remove(card);
        var tipPos = GetCardTipPos(card.transform);
        StartCoroutine(_CardAnimGoToDiscard(card.transform));
        
        discard.Add(info);
        GameMaster.s.cardsPlayed += 1;
        
        switch (info.myType) {
            case CardType.makeFoodBasic: {
                GameMaster.s.foodMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(food, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.mergeBlobsBasic: {
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(mergeToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.createRedBlobBasic: {
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(redBlob, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            }
            case CardType.sellBlobBasic:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(sellToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.drawCardBasic:{
                DrawCardFromDeck();
                DrawCardFromDeck();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        yield return null;
        GameMaster.s.moveNextBlock -= 1;
    }

    private IEnumerator YeetObjectToBoard(Vector3 location, GameObject yeetObj, Vector3 tipPos) {
        var foodScript =yeetObj.GetComponent<FoodScript>();
        var agent = yeetObj.GetComponent<NavMeshAgent>();
        var creature = yeetObj.GetComponent<CreatureScript>();
        if (foodScript) {
            yeetObj.GetComponent<Rigidbody>().isKinematic = true;
            foodScript.enabled = false;
        }
        if (agent) {
            agent.enabled = false;
        }
        if (creature) {
            creature.enabled = false;
        }
        
        var spawnPercent = 0f;

        while (spawnPercent < 1) {
            spawnPercent += Time.deltaTime;
            var realEatPercent = Mathf.Clamp01(spawnPercent);
            var yummyTargetPos = Vector3.Lerp(tipPos, location, realEatPercent);
            yummyTargetPos.y += Mathf.Sin(realEatPercent * Mathf.PI);
            yeetObj.transform.position = yummyTargetPos;
            yield return null;
        }
        
        if (foodScript) {
            yeetObj.GetComponent<Rigidbody>().isKinematic = false;
            foodScript.enabled = true;
        }
        if (agent) {
            agent.enabled = true;
        }
        if (creature) {
            creature.enabled = true;
        }
    }

    IEnumerator _CardAnimGoToDiscard(Transform card) {
        Vector3 startPos = card.position;
        Quaternion startRot = card.rotation;
        card.localPosition = new Vector3(6f, 0.4f, 0);
        card.localRotation = Quaternion.identity; 
        Vector3 targetPos = card.position;
        Quaternion targetRot = card.rotation;
        card.position = startPos;
        card.rotation = startRot;
        card.SetParent(null);
        
        while (Vector3.Distance(card.position, targetPos) > 0.5f) {
            card.position = Vector3.Lerp(card.position, targetPos,10f*Time.deltaTime);
            card.localScale = Vector3.Lerp(card.localScale, Vector3.zero, 5f*Time.deltaTime);
            //card.rotation = Quaternion.Slerp(card.rotation, targetRot,20*Time.deltaTime );
            yield return null;
        }
        
        Destroy(card.gameObject);
    }


    float GetMouseAngle() {
        Vector2 circleCenter = new Vector2(0, 0);
        
        Vector2 mousePosition = Input.mousePosition;
        Vector2 direction = mousePosition - circleCenter;
        float angleRadians = Mathf.Atan2(direction.y, direction.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        return angleDegrees;
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

    public static Vector3 GetCardTipPos(Transform cardTrans) {
        return cardTrans.position + cardTrans.forward * 2 + cardTrans.right * 0.75f;
    }
}
