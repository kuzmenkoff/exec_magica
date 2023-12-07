using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public DecksManagerScr DecksManager;
    public List<Card> EnemyDeck, PlayerDeck;
    public int StarterCardsNum = 4;

    public Game(DecksManagerScr decksManager)
    {
        DecksManager = decksManager;
        
        EnemyDeck = new List<Card>(DecksManager.GetEnemyDeck().cards);
        PlayerDeck = new List<Card>(DecksManager.GetMyDeck().cards);
        List<Card> ShuffledDeck = ShuffleDeck(EnemyDeck);
        EnemyDeck = ShuffledDeck;
        ShuffledDeck = ShuffleDeck(PlayerDeck);
        PlayerDeck = ShuffledDeck;
    }

    public List<Card> ShuffleDeck(List<Card> Deck)
    {
        Card temp;
        System.Random random = new System.Random();
        // Fisher–Yates shuffle
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);

            temp = Deck[i];
            Deck[i] = Deck[randomIndex];
            Deck[randomIndex] = temp;
        }
        return Deck;
    }
}

public class GameManagerScr : MonoBehaviour
{
    public static GameManagerScr Instance;

    public Game CurrentGame;
    public Transform EnemyHand, PlayerHand,
                     EnemyField, PlayerField;
    public GameObject CardPref;
    DecksManagerScr decksManager;
    public int Turn = 1, TurnTime, OriginalTurnTime = 60;
    public bool TimerIsOn = true, PlayerIsFirst, PlayersTurn;
    public TextMeshProUGUI TurnTimeTxt, WhoseTurn;
    public Button EndTurnButton;
    public TextMeshProUGUI PlayerManaTxt, EnemyManaTxt;
    public List<GameObject> PlayerManaPoints, EnemyManaPoints;
    public Sprite ActiveManaPoint, InactiveManaPoint;

    public int PlayerHP, EnemyHP;
    public TextMeshProUGUI PlayerHPTxt, EnemyHPTxt;

    public GameObject ResultGO;
    public TextMeshProUGUI ResultTxt;

    public AttackedHero EnemyHero, PlayerHero;

    public int PlayerMana, EnemyMana, PlayerMaxMana = 1, EnemyMaxMana = 1, MAXMana = 10;

    public List<CardController> PlayerHandCards = new List<CardController>(),
                                PlayerFieldCards = new List<CardController>(),
                                EnemyHandCards = new List<CardController>(),
                                EnemyFieldCards = new List<CardController>();

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu_Scene");
    }

    public void RestartGame()
    {
        StopAllCoroutines();

        foreach (var card in PlayerHandCards)
            Destroy(card.gameObject);
        foreach (var card in PlayerFieldCards)
            Destroy(card.gameObject);
        foreach (var card in EnemyHandCards)
            Destroy(card.gameObject);
        foreach (var card in EnemyFieldCards)
            Destroy(card.gameObject);

        PlayerHandCards.Clear();
        PlayerFieldCards.Clear();
        EnemyHandCards.Clear();
        EnemyFieldCards.Clear();

        StartGame();
    }

    void StartGame()
    {
        decksManager = GetComponent<DecksManagerScr>();
        CurrentGame = new Game(decksManager);
        TurnTimeTxt.enabled = TimerIsOn;
        PlayerIsFirst = FlipCoin();
        PlayersTurn = PlayerIsFirst;
        PlayerMana = PlayerMaxMana;
        EnemyMana = EnemyMaxMana;

        PlayerHP = EnemyHP = 30;

        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand, false);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand, true);


        if (!PlayerIsFirst)
        {
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand, false);
            WhoseTurn.text = "Enemy turn";
            EndTurnButton.interactable = false;
        }
        else
        {
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand, true);
            WhoseTurn.text = "Your turn";
            EndTurnButton.interactable = true;
        }

        Turn = 0;
        ShowHP();
        ShowMana();

        ResultGO.SetActive(false);

        StartCoroutine(TurnFunc());
    }

    void GiveHandCards (List<Card> deck, Transform hand, bool player)
    {
        int i = 0;
        while (i++ < CurrentGame.StarterCardsNum)
        {
            GiveCardToHand(deck, hand, player);
        }
    }
    void GiveCardToHand(List<Card> deck, Transform hand, bool player)
    {
        if (player && PlayerHandCards.Count >= 8)
            return;
        else if (!player && EnemyHandCards.Count >= 8)
            return;
        if (deck.Count == 0)
            return;

        CreateCardPref(deck[0], hand);

        deck.RemoveAt(0);

    }

    void CreateCardPref(Card card, Transform hand)
    {
        GameObject cardGO = Instantiate(CardPref, hand, false);
        cardGO.SetActive(true);
        CardController cardC = cardGO.GetComponent<CardController>();

        cardC.Init(card, hand == PlayerHand);
        if (cardC.IsPlayerCard)
            PlayerHandCards.Add(cardC);
        else
            
            EnemyHandCards.Add(cardC);
    }

    IEnumerator TurnFunc()
    {
        foreach (var card in PlayerFieldCards)
            card.Info.PaintWhite();

        if (TimerIsOn)
        {
            TurnTime = OriginalTurnTime;
            TurnTimeTxt.text = TurnTime.ToString();
        }

        CheckCardForManaAvailability();

        if (PlayersTurn)
        {
            foreach (var card in PlayerFieldCards)
            {
                card.Card.CanAttack = true;
                card.Info.HighliteUsableCard();
                card.Ability.OnNewTurn();
            }

            while (TurnTime-- > 0)
            {
                TurnTimeTxt.text = TurnTime.ToString();
                yield return new WaitForSeconds(1);
            }
            ChangeTurn();
        }
        else
        {
            foreach (var card in EnemyFieldCards)
            {
                card.Card.CanAttack = true;
                card.Ability.OnNewTurn();
            }


            StartCoroutine(EnemyTurn(EnemyHandCards));
        }
    }

    IEnumerator EnemyTurn(List<CardController> cards)
    {
        yield return new WaitForSeconds(1);

        int randomCount = UnityEngine.Random.Range(0, cards.Count);
        for (int i = 0; i < randomCount; i++)
        {
            if (EnemyFieldCards.Count > 5 || EnemyMana == 0 || EnemyHandCards.Count == 0)
                break;

            List<CardController> cardsList = cards.FindAll(x => EnemyMana >= x.Card.ManaCost);

            if (cardsList.Count == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, cardsList.Count);

            cardsList[randomIndex].GetComponent<CardMovementScr>().MoveToField(EnemyField);

            yield return new WaitForSeconds(.51f);
            
            cardsList[randomIndex].transform.SetParent(EnemyField);

            cardsList[randomIndex].OnCast();
        }

        yield return new WaitForSeconds(1);

        while (EnemyFieldCards.Exists(x => x.Card.CanAttack))
        {
            var activeCard = EnemyFieldCards.FindAll(x => x.Card.CanAttack)[0];
            bool hasProvocation = PlayerFieldCards.Exists(x => x.Card.IsProvocation);
            if (hasProvocation ||
                UnityEngine.Random.Range(0, 2) == 0 && PlayerFieldCards.Count > 0)
            {
                CardController enemy;

                if(hasProvocation)
                    enemy = PlayerFieldCards.Find(x => x.Card.IsProvocation);
                else
                    enemy = PlayerFieldCards[UnityEngine.Random.Range(0, PlayerFieldCards.Count)];


                Debug.Log(activeCard.Card.Title + " (" + activeCard.Card.Attack + "; " + activeCard.Card.HP + ") ---> " +
                          enemy.Card.Title + " (" + enemy.Card.Attack + "; " + enemy.Card.HP + ")");

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(enemy.transform);
                yield return new WaitForSeconds(.75f);

                CardsFight(enemy, activeCard);
            }
            else
            {
                Debug.Log(activeCard.Card.Title + " (" + activeCard.Card.Attack + "; " + activeCard.Card.HP + ") ---> Hero");

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(PlayerHero.transform);
                yield return new WaitForSeconds(.75f);

                DamageHero(activeCard, false);
            }

            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(1);
        ChangeTurn();
    }

    public void RenewDeck(bool playerdeck)
    {
        if (playerdeck)
        {

            CurrentGame.PlayerDeck = new List<Card>(decksManager.GetMyDeck().cards);
            CurrentGame.PlayerDeck = CurrentGame.ShuffleDeck(CurrentGame.PlayerDeck);
        }
        else
        {
            CurrentGame.EnemyDeck = new List<Card>(decksManager.GetEnemyDeck().cards);
            CurrentGame.EnemyDeck = CurrentGame.ShuffleDeck(CurrentGame.EnemyDeck);
        }
    }

    public void ChangeTurn()
    {
        StopAllCoroutines();
        Turn++;
        PlayersTurn = !PlayersTurn;
        EndTurnButton.interactable = PlayersTurn;
        Debug.Log(CurrentGame.DecksManager.GetMyDeck().cards.Count);

        if (PlayersTurn)
        {
            if (CurrentGame.PlayerDeck.Count == 0)
                RenewDeck(true);
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand, true);
            WhoseTurn.text = "Your turn";
            if (PlayerMaxMana < MAXMana && Turn != 1)
                PlayerMaxMana++;
            PlayerMana = PlayerMaxMana;
            ShowMana();
        }
        else
        {
            if (CurrentGame.EnemyDeck.Count == 0)
                RenewDeck(false);
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand, false);
            WhoseTurn.text = "Enemy turn";
            if (EnemyMaxMana < MAXMana && Turn != 1)
                EnemyMaxMana++;
            EnemyMana = EnemyMaxMana;
            ShowMana();
        }
        StartCoroutine(TurnFunc());
    }

    public bool FlipCoin()
    {
        System.Random random = new System.Random();
        return random.Next(2) == 1;
    }

    public void CardsFight(CardController attacker, CardController defender)
    {
        defender.Card.GetDamage(attacker.Card.Attack);
        attacker.OnDamageDeal(defender);
        defender.OnTakeDamage(attacker);

        attacker.Card.GetDamage(defender.Card.Attack);
        attacker.OnTakeDamage();
        /*if (attacker.Card.Abilities.Contains(Card.AbilityType.SILENCE))
            defender.OnDamageDeal(attacker);
        else
            attacker.OnTakeDamage();*/
        
        attacker.CheckForAlive();
        defender.CheckForAlive();
    }

    public void ShowMana()
    {
        PlayerManaTxt.text = PlayerMana.ToString() + " / " + PlayerMaxMana.ToString();
        if (PlayerMana != 0) {
            for (int i = 0; i < PlayerMana; i++)
            {
                PlayerManaPoints[i].GetComponent<Image>().sprite = ActiveManaPoint;
            }
        }
        if (PlayerMana != MAXMana)
        {
            for (int i = PlayerMana; i < MAXMana; i++)
            {
                PlayerManaPoints[i].GetComponent<Image>().sprite = InactiveManaPoint;
            }
        }

        EnemyManaTxt.text = EnemyMana.ToString() + " / " + EnemyMaxMana.ToString();
        if (EnemyMana != 0)
        {
            for (int i = 0; i < EnemyMana; i++)
            {
                EnemyManaPoints[i].GetComponent<Image>().sprite = ActiveManaPoint;
            }
        }
        if (EnemyMana != MAXMana)
        {
            for (int i = EnemyMana; i < MAXMana; i++)
            {
                EnemyManaPoints[i].GetComponent<Image>().sprite = InactiveManaPoint;
            }
        }
    }

    void ShowHP()
    {
        EnemyHPTxt.text = EnemyHP.ToString();
        PlayerHPTxt.text = PlayerHP.ToString();
    }

    public void ReduceMana(bool playerMana, int manacost)
    {
        if (playerMana)
            PlayerMana = Mathf.Clamp(PlayerMana - manacost, 0, int.MaxValue);
        else
            EnemyMana = Mathf.Clamp(EnemyMana - manacost, 0, int.MaxValue);
        ShowMana();
    }    
    
    public void DamageHero(CardController card, bool isEnemyAttacked)
    {
        if (isEnemyAttacked)
            EnemyHP = Mathf.Clamp(EnemyHP - card.Card.Attack, 0, int.MaxValue);
        else
            PlayerHP = Mathf.Clamp(PlayerHP - card.Card.Attack, 0, int.MaxValue);
        ShowHP();
        card.OnDamageDeal();
        CheckForVictory();
    }

    void CheckForVictory()
    {
        if (EnemyHP == 0 || PlayerHP == 0)
        {
            ResultGO.SetActive(true);
            StopAllCoroutines();

            if (EnemyHP == 0)
                ResultTxt.text = "Hooraaaay! You won!";
            else
                ResultTxt.text = "Womp-womp... You lost.";
        }
    }

    public void CheckCardForManaAvailability()
    {
        foreach (var card in PlayerHandCards)
            card.Info.HighlightManaAvaliability(PlayerMana);

        
    }

    public void HightLightTargets(bool highlight)
    {
        List<CardController> targets = new List<CardController>();

        if (EnemyFieldCards.Exists(x => x.Card.IsProvocation))
            targets = EnemyFieldCards.FindAll(x => x.Card.IsProvocation);
        else
        {
            targets = EnemyFieldCards;
            EnemyHero.HighlightAsTarget(highlight);
        }
            

        foreach (var card in targets)
            card.Info.HighlightAsTarget(highlight);
    }
}
