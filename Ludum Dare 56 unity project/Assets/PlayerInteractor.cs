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

    public GameObject foodToken;
    public GameObject mergeToken;
    public GameObject sellToken;
    public GameObject butcherToken;
    public GameObject carrotToken;
    public GameObject blopChunksToken;
    public GameObject bloodMoneyToken;
    public GameObject berryToken;
    public GameObject marketingToken;
    public GameObject tripleBurgerToken;
    public GameObject steakToken;
    public GameObject potatoToken;
    public GameObject blopDrugToken;
    public GameObject hormonesToken;
    public GameObject mitosisToken;
    public GameObject transmuteToken;
    
    
    public GameObject redBlob;
    public GameObject blueBlop;
    public GameObject whiteBlop;
    public GameObject purpleBlop;
    public GameObject greenBlop;
    public GameObject catBlop;
    public GameObject manaBlop;

    public GameObject clickEffect;
    
    
    public LayerMask table;
    public LayerMask cards;

    public TMP_Text manaText;
    public int curMana = 0;
    
    public enum CardType {
        makeFoodBasic=0, mergeBlobsBasic=1, createRedBlobBasic=2, sellBlobBasic=3, drawCardBasic=4, removeNextCard=5, butcher=6,
        makeCarrot=7, bloodMoney=8, makeBerry=9, marketing=10, makeTripleBurger=11, makeSteak=12, makePotato=13,
        makeBlueBlop=14, foodForMasses=15, blopHungerCards=16, blopDrugs=17, foodDraw=18, makeWhiteBlop=19, makePurpleBlop=20,
        makeGreenBlop=21, illegalHormones=22, makeCatBlop=24, makeManaBlop=25, blopper=26, infrastructure=27,
        heft=28, mitosis=29, transmute=30, photos=23
    }
    
    public enum CardGraphicBase {
        food=0, utility=1, special=2, sell=3
    }

    public Transform cardsParent;
    public GameObject[] cardPrefabs;

    public CardInfoScriptable[] startingHand;
    public CardInfoScriptable[] testHand;
    public bool doTestHand = true;

    public int drawPerStep = 4;
    public int manaPerStep = 3;
    
    public List<CardInfoScriptable> deck = new List<CardInfoScriptable>();
    public List<CardInfoScriptable> discard = new List<CardInfoScriptable>();
    public List<CardScript> currentCards = new List<CardScript>();
    private Camera mainCam;
    private CardTargetShower _targetShower;

    public TMP_Text deckCountText;
    public TMP_Text discardCountText;

    public bool flashManaCost = false;
    private Transform _manaParent;
    
    private void Start() {
        deck.AddRange(startingHand);

        mainCam = Camera.main;
        _targetShower = GetComponentInChildren<CardTargetShower>();
        _manaParent = manaText.transform.parent;

        if (Application.isEditor && doTestHand) {
            for (int i = 0; i < testHand.Length; i++) {
                AddCard(testHand[i]);
            }
            curMana = manaPerStep;
        } else {
            NextStep();
        }
    }


    public void NextStep() {
        StartCoroutine(_NextStep());
    }

    IEnumerator _NextStep() {
        GameMaster.s.moveNextBlock += 1;
        GameMaster.s.cardActionBlock += 1;
        var count = currentCards.Count;
        for (int i = 0; i < count; i++) {
            var card = currentCards[0];
            currentCards.Remove(card);
            discard.Add(card.myInfo);
            StartCoroutine(_CardAnimGoToDiscard(card.transform));
            yield return new WaitForSeconds(0.1f);
        }

        var thisDraw = drawPerStep;
        var thisMana = manaPerStep;
        var allCreatures = GameMaster.s.GetComponentsInChildren<CreatureScript>();
        for (int i = 0; i < allCreatures.Length; i++) {
            if (allCreatures[i].myType == CreatureScript.BlopType.cat) {
                switch (allCreatures[i].size) {
                    case 1:
                        thisDraw += 1;
                        break;
                    case 2:
                        thisDraw += 3;
                        break;
                    case 3:
                        thisDraw += 8;
                        break;
                }
            }else if (allCreatures[i].myType == CreatureScript.BlopType.mana) {
                switch (allCreatures[i].size) {
                    case 1:
                        thisMana += 1;
                        break;
                    case 2:
                        thisMana += 3;
                        break;
                    case 3:
                        thisMana += 8;
                        break;
                }
            }
        }

        for (int i = 0; i < thisDraw; i++) {
            DrawCardFromDeck();
        }

        curMana = thisMana;

        GameMaster.s.moveNextBlock -= 1;
        GameMaster.s.cardActionBlock -= 1;
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
        var newCard = MakeCard(cardInfoScriptable, cardsParent);
        currentCards.Add(newCard);
    }

    public CardScript MakeCard(CardInfoScriptable cardInfoScriptable, Transform parent) {
        var prefab = cardPrefabs[(int)cardInfoScriptable.myBase];
        var newCard = Instantiate(prefab, parent).GetComponent<CardScript>();
        newCard.SetUp(cardInfoScriptable);
        return newCard;
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
        manaText.text = $"{curMana}";
        Vector3 mousePos = Input.mousePosition;
        Vector3 normalizedMousePos = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height);

        if (flashManaCost) {
            if (_manaParent.transform.localScale.x < 1.5f) {
                _manaParent.transform.localScale = Vector3.MoveTowards(_manaParent.transform.localScale, Vector3.one*1.5f,5*Time.deltaTime);
            } else {
                flashManaCost = false;
            }
        } else {
            if (_manaParent.transform.localScale.x > 1) {
                _manaParent.transform.localScale = Vector3.MoveTowards(_manaParent.transform.localScale, Vector3.one,5*Time.deltaTime);
            }
        }
        
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

            if (currentCard.myInfo.manaCost > curMana) {
                castingLegalPosition = false;
                flashManaCost = true;
                nearCards = false;
            } else {
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

    public bool removeNextCard = false;
    public GameObject removeCardEffect;
    IEnumerator _CastCardAtLocation(CardScript card, CardInfoScriptable info, Vector3 location) {
        GameMaster.s.moveNextBlock += 1;
        currentCards.Remove(card);
        var tipPos = GetCardTipPos(card.transform);
        curMana -= info.manaCost;

        if (removeNextCard || info.once) { //remove card
            Instantiate(removeCardEffect, card.transform.position, card.transform.rotation);
            Destroy(card.gameObject);
            removeNextCard = false;
        } else { // put it in discard
            StartCoroutine(_CardAnimGoToDiscard(card.transform));
            discard.Add(info);
        }
        GameMaster.s.cardsPlayed += 1;
        
        switch (info.myType) {
            case CardType.makeFoodBasic: {
                GameMaster.s.foodMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
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
            case CardType.removeNextCard: {
                removeNextCard = true;
                break;
            }
            case CardType.butcher: {
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(butcherToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makeCarrot: {
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(carrotToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.bloodMoney: {
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(bloodMoneyToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makeBerry:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(berryToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.marketing:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(marketingToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makeTripleBurger:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(tripleBurgerToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makeSteak:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(steakToken, tipPos, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makePotato:{
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(potatoToken, tipPos, Quaternion.identity), tipPos));
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(potatoToken, tipPos+Vector3.left*0.5f, Quaternion.identity), tipPos));
                break;
            }
            case CardType.makeBlueBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(blueBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.foodForMasses: {
                var allCreatures = GameMaster.s.GetComponentsInChildren<CreatureScript>();
                for (int i = 0; i < allCreatures.Length; i++) {
                    yield return StartCoroutine(YeetObjectToBoard(allCreatures[i].transform.position, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
                }
                break;
            }
            case CardType.blopHungerCards:{
                var allCreatures = GameMaster.s.GetComponentsInChildren<CreatureScript>();
                for (int i = 0; i < allCreatures.Length; i++) {
                    allCreatures[i].GoToNextStep();
                    DrawCardFromDeck();
                }
                break;
            }
            case CardType.blopDrugs:
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(blopDrugToken, tipPos, Quaternion.identity), tipPos));
                break;
            case CardType.foodDraw:
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
                DrawCardFromDeck();
                break;
            case CardType.makeWhiteBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(whiteBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.makePurpleBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(purpleBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.makeGreenBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(greenBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.illegalHormones:
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(hormonesToken, tipPos, Quaternion.identity), tipPos));
                break;
            case CardType.makeCatBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(catBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.makeManaBlop:
                GameMaster.s.blobsMade += 1;
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(manaBlop, tipPos, Quaternion.identity, GameMaster.s.creatureParent), tipPos));
                break;
            case CardType.blopper:
                for (int i = 0; i < deck.Count; i++) {
                    if (deck[i].myBase == CardGraphicBase.special) {
                        yield return StartCoroutine(YeetObjectToBoard(location + Vector3.up*0.25f, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
                    }
                }
                break;
            case CardType.infrastructure:
                for (int i = 0; i < deck.Count; i++) {
                    if (deck[i].myBase == CardGraphicBase.food) {
                        yield return StartCoroutine(YeetObjectToBoard(location + Vector3.up*0.25f, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
                    }
                }
                break;
            case CardType.heft:
                for (int i = 0; i < deck.Count; i++) {
                    if (i%5 == 0) {
                        yield return StartCoroutine(YeetObjectToBoard(location + Vector3.up*0.25f, Instantiate(foodToken, tipPos, Quaternion.identity), tipPos));
                    }
                }
                break;
            case CardType.mitosis:
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(mitosisToken, tipPos, Quaternion.identity), tipPos));
                break;
            case CardType.transmute:
                yield return StartCoroutine(YeetObjectToBoard(location, Instantiate(transmuteToken, tipPos, Quaternion.identity), tipPos));
                break;
            case CardType.photos:{
                var allCreatures = GameMaster.s.GetComponentsInChildren<CreatureScript>();
                for (int i = 0; i < allCreatures.Length; i++) {
                    var blop = allCreatures[i];
                    if (blop.GetVisualHungerLevel() >= 3) {
                        
                        GameMaster.s.money += blop.blopSellPrice;
                        Instantiate(blop.blopSoldEffect, blop.transform.position, Quaternion.identity).GetComponent<SellEffect>().SetUp(blop.blopSellPrice);
                    }
                }
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
