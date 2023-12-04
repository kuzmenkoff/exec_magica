using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    int Turn = 1, TurnTime, OriginalTurnTime = 60;
    public bool TimerIsOn = true, PlayerIsFirst, PlayersTurn;
    public TextMeshProUGUI TurnTimeTxt, WhoseTurn;
    public Button EndTurnButton;

    public List<CardInfoScript> PlayerHandCards = new List<CardInfoScript>(),
                                PlayerFieldCards = new List<CardInfoScript>(),
                                EnemyHandCards = new List<CardInfoScript>(),
                                EnemyFieldCards = new List<CardInfoScript>();

    void Start()
    {
        decksManager = GetComponent<DecksManagerScr>();
        CurrentGame = new Game(decksManager);
        TurnTimeTxt.enabled = TimerIsOn;
        PlayerIsFirst = FlipCoin();
        PlayersTurn = PlayerIsFirst;
        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand);
        if (!PlayerIsFirst)
        {
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand);
            WhoseTurn.text = "Enemy turn";
        }
        else
        {
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand);
            WhoseTurn.text = "Your turn";
        }

        Turn = 0;

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
        }
        else
        {
            foreach (var card in EnemyFieldCards)
                card.SelfCard.ChangeUsageState(true);
                
            

            while (TurnTime-- > OriginalTurnTime - 3)
            {
                TurnTimeTxt.text = TurnTime.ToString();
                yield return new WaitForSeconds(1);
            }
            if (EnemyHandCards.Count > 0)
                EnemyTurn(EnemyHandCards);
        }

        ChangeTurn();
    }

    void EnemyTurn(List<CardInfoScript> cards)
    {
        
        int randomCount = UnityEngine.Random.Range(0, cards.Count);
        for (int i = 0; i < randomCount; i++)
        {
            if (EnemyFieldCards.Count > 5)
                return;

            int randomIndex = UnityEngine.Random.Range(0, cards.Count);
            cards[randomIndex].ShowCardInfo(cards[randomIndex].SelfCard, false);
            cards[randomIndex].transform.SetParent(EnemyField);


            EnemyFieldCards.Add(cards[randomIndex]);
            EnemyHandCards.Remove(cards[randomIndex]);
        }

        foreach (var activeCard in EnemyFieldCards.FindAll (x => x.SelfCard.CanBeUsed))
        {
            if (EnemyFieldCards.Count == 0)
                return;
            var enemy = PlayerFieldCards[UnityEngine.Random.Range(0, PlayerFieldCards.Count)];

            Debug.Log(activeCard.SelfCard.Title + " (" + activeCard.SelfCard.Attack + "; " + activeCard.SelfCard.HP + ") ---> " +
                      enemy.SelfCard.Title + " (" + enemy.SelfCard.Attack + "; " + enemy.SelfCard.HP + ")");

            activeCard.SelfCard.ChangeUsageState(false);
            CardsFight(enemy, activeCard);
        }
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
        }
        else
        {
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand);
            WhoseTurn.text = "Enemy turn";
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
}
