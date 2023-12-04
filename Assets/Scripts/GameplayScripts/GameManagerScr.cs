using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    DecksManagerScr DecksManager;
    public List<Card> EnemyDeck, PlayerDeck;
    public int StarterCardsNum = 4;

    public Game(DecksManagerScr decksManager)
    {
        DecksManager = decksManager;
        
        EnemyDeck = DecksManager.EnemyDeck.cards;
        PlayerDeck = DecksManager.MyDeck.cards;
        List<Card> ShuffledDeck = ShuffleDeck(EnemyDeck);
        EnemyDeck = ShuffledDeck;
        ShuffledDeck = ShuffleDeck(PlayerDeck);
        PlayerDeck = ShuffledDeck;
    }

    List<Card> ShuffleDeck(List<Card> Deck)
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

    public List<CardInfoScript> PlayerHandCards = new List<CardInfoScript>(),
                                PlayerFieldCards = new List<CardInfoScript>(),
                                EnemyHandCards = new List<CardInfoScript>(),
                                EnemyFieldCards = new List<CardInfoScript>();

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

        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand);
        if (!PlayerIsFirst)
        {
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand);
            WhoseTurn.text = "Enemy turn";
            EndTurnButton.interactable = false;
        }
        else
        {
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand);
            WhoseTurn.text = "Your turn";
            EndTurnButton.interactable = true;
        }

        Turn = 0;
        ShowHP();
        ShowMana();

        ResultGO.SetActive(false);

        StartCoroutine(TurnFunc());
    }

    void GiveHandCards (List<Card> deck, Transform hand)
    {
        int i = 0;
        while (i++ < CurrentGame.StarterCardsNum)
        {
            GiveCardToHand(deck, hand);
        }
    }
    void GiveCardToHand(List<Card> deck, Transform hand)
    {
        if (deck.Count == 0)
            return;

        Card card = deck[0];
        
        GameObject cardGO = Instantiate(CardPref, hand, false);
        cardGO.SetActive(true);

        if (hand == EnemyHand)
        {
            cardGO.GetComponent<CardInfoScript>().HideCardInfo(card);
            EnemyHandCards.Add(cardGO.GetComponent<CardInfoScript>());
        }
        else
        {
            cardGO.GetComponent<CardInfoScript>().ShowCardInfo(card, true);
            PlayerHandCards.Add(cardGO.GetComponent<CardInfoScript>());
            cardGO.GetComponent<AttackedCard>().enabled = false;
        }

        deck.RemoveAt(0);

    }

    IEnumerator TurnFunc()
    {
        foreach (var card in PlayerFieldCards)
            card.PaintWhite();

        if (TimerIsOn)
        {
            TurnTime = OriginalTurnTime;
            TurnTimeTxt.text = TurnTime.ToString();
        }

        CheckCardForAvailability();

        if (PlayersTurn)
        {
            foreach (var card in PlayerFieldCards)
            {
                card.SelfCard.ChangeUsageState(true);
                card.HighliteUsableCard();
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
                card.SelfCard.ChangeUsageState(true);


            StartCoroutine(EnemyTurn(EnemyHandCards));
        }
    }

    IEnumerator EnemyTurn(List<CardInfoScript> cards)
    {
        yield return new WaitForSeconds(1);

        int randomCount = UnityEngine.Random.Range(0, cards.Count);
        for (int i = 0; i < randomCount; i++)
        {
            if (EnemyFieldCards.Count > 5 || EnemyMana == 0 || EnemyHandCards.Count == 0)
                break;

            List<CardInfoScript> cardsList = cards.FindAll(x => EnemyMana >= x.SelfCard.ManaCost);

            if (cardsList.Count == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, cardsList.Count);

            cardsList[randomIndex].GetComponent<CardMovementScr>().MoveToField(EnemyField);

            ReduceMana(false, cardsList[0].SelfCard.ManaCost);

            yield return new WaitForSeconds(.51f);
            
            cardsList[randomIndex].ShowCardInfo(cardsList[randomIndex].SelfCard, false);
            cardsList[randomIndex].transform.SetParent(EnemyField);


            EnemyFieldCards.Add(cardsList[randomIndex]);
            EnemyHandCards.Remove(cardsList[randomIndex]);
        }

        yield return new WaitForSeconds(1);

        foreach (var activeCard in EnemyFieldCards.FindAll (x => x.SelfCard.CanBeUsed))
        {
            if (UnityEngine.Random.Range(0, 2) == 0 && PlayerFieldCards.Count > 0)
            {



                var enemy = PlayerFieldCards[UnityEngine.Random.Range(0, PlayerFieldCards.Count)];

                Debug.Log(activeCard.SelfCard.Title + " (" + activeCard.SelfCard.Attack + "; " + activeCard.SelfCard.HP + ") ---> " +
                          enemy.SelfCard.Title + " (" + enemy.SelfCard.Attack + "; " + enemy.SelfCard.HP + ")");

                activeCard.SelfCard.ChangeUsageState(false);

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(enemy.transform);
                yield return new WaitForSeconds(.75f);

                CardsFight(enemy, activeCard);
            }
            else
            {
                Debug.Log(activeCard.SelfCard.Title + " (" + activeCard.SelfCard.Attack + "; " + activeCard.SelfCard.HP + ") ---> Hero");

                activeCard.SelfCard.ChangeUsageState(false);

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(PlayerHero.transform);
                yield return new WaitForSeconds(.75f);

                DamageHero(activeCard, false);
            }

            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(1);
        ChangeTurn();
    }

    public void ChangeTurn()
    {
        StopAllCoroutines();
        Turn++;
        PlayersTurn = !PlayersTurn;
        EndTurnButton.interactable = PlayersTurn;

        if (PlayersTurn)
        {
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand);
            WhoseTurn.text = "Your turn";
            if (PlayerMaxMana < MAXMana && Turn != 1)
                PlayerMaxMana++;
            PlayerMana = PlayerMaxMana;
            ShowMana();
        }
        else
        {
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand);
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

    public void CardsFight(CardInfoScript playerCard, CardInfoScript enemyCard)
    {
        playerCard.SelfCard.GetDamage(enemyCard.SelfCard.Attack);
        enemyCard.SelfCard.GetDamage(playerCard.SelfCard.Attack);

        if (!playerCard.SelfCard.IsAlive())
            DestroyCard(playerCard);
        else
            playerCard.RefreshData();

        if (!enemyCard.SelfCard.IsAlive())
            DestroyCard(enemyCard);
        else
            enemyCard.RefreshData();
    }

    void DestroyCard(CardInfoScript card)
    {
        card.GetComponent<CardMovementScr>().OnEndDrag(null);

        if (EnemyFieldCards.Exists(x => x == card))
            EnemyFieldCards.Remove(card);

        if (PlayerFieldCards.Exists(x => x == card))
            PlayerFieldCards.Remove(card);

        Destroy(card.gameObject);
    }

    void ShowMana()
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
    
    public void DamageHero(CardInfoScript card, bool isEnemyAttacked)
    {
        if (isEnemyAttacked)
            EnemyHP = Mathf.Clamp(EnemyHP - card.SelfCard.Attack, 0, int.MaxValue);
        else
            PlayerHP = Mathf.Clamp(PlayerHP - card.SelfCard.Attack, 0, int.MaxValue);
        ShowHP();
        card.PaintWhite();
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

    public void CheckCardForAvailability()
    {
        foreach (var card in PlayerHandCards)
            card.CheckForAvailability(PlayerMana);

        
    }

    public void HightLightTargets(bool highlight)
    {
        foreach (var card in EnemyFieldCards)
            card.HighliightAsTarget(highlight);

        EnemyHero.HighlightAsTarget(highlight);
    }
}
