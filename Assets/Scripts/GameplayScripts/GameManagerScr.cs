using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public Player Player, Enemy;
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

        Player = new Player();
        Enemy = new Player();
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
    public DecksManagerScr decksManager;
    public int Turn = 1, TurnTime, OriginalTurnTime = 60;
    public bool TimerIsOn = true, PlayerIsFirst, PlayersTurn;

    public AttackedHero EnemyHero, PlayerHero;
    public AI EnemyAI;
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
        UIController.Instance.EnableTurnTime(TimerIsOn);
        PlayerIsFirst = FlipCoin();
        PlayersTurn = PlayerIsFirst;

        UIController.Instance.EnableTurnTime(TimerIsOn);

        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand, false);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand, true);

        UIController.Instance.WhoseTurnUpdate();
        UIController.Instance.EnableTurnBtn();

        GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand, PlayersTurn);

        Turn = 0;

        CurrentGame.Player.Mana = CurrentGame.Player.Manapool = 1;
        CurrentGame.Enemy.Mana = CurrentGame.Enemy.Manapool = 1;

        UIController.Instance.UpdateHPAndMana();

        UIController.Instance.StartGame();

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
            UIController.Instance.UpdateTurnTime(TurnTime);
        }
        else
            TurnTime = int.MaxValue;

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
                UIController.Instance.UpdateTurnTime(TurnTime);
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


            EnemyAI.MakeTurn();
            while (TurnTime -- > OriginalTurnTime - 3)
            {
                UIController.Instance.UpdateTurnTime(TurnTime);
                yield return new WaitForSeconds(1);
            }

            ChangeTurn();
        }
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
        UIController.Instance.EnableTurnBtn();
        Debug.Log(CurrentGame.DecksManager.GetMyDeck().cards.Count);
        UIController.Instance.WhoseTurnUpdate();


        if (PlayersTurn)
        {
            if (CurrentGame.PlayerDeck.Count == 0)
                RenewDeck(true);
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand, true);
            if (Turn != 1)
                CurrentGame.Player.IncreaseManapool();
            CurrentGame.Player.RestoreRoundMana();
            
        }
        else
        {
            if (CurrentGame.EnemyDeck.Count == 0)
                RenewDeck(false);
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand, false);
            if(Turn != 1)
                CurrentGame.Enemy.IncreaseManapool();
            CurrentGame.Enemy.RestoreRoundMana();
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

    /*public void ShowMana()
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
    }*/


    public void ReduceMana(bool playerMana, int manacost)
    {
        if (playerMana)
            CurrentGame.Player.Mana -= manacost;
        else
            CurrentGame.Enemy.Mana -= manacost;
        UIController.Instance.UpdateHPAndMana();
    }    
    
    public void DamageHero(CardController card, bool isEnemyAttacked)
    {
        if (isEnemyAttacked)
            CurrentGame.Enemy.GetDamage(card.Card.Attack);
        else
            CurrentGame.Player.GetDamage(card.Card.Attack);

        UIController.Instance.UpdateHPAndMana();
        card.OnDamageDeal();
        CheckForVictory();
    }

    public void CheckForVictory()
    {
        if (CurrentGame.Enemy.HP == 0 || CurrentGame.Player.HP == 0)
        {
            StopAllCoroutines();
            UIController.Instance.ShowResult();
        }
    }

    public void CheckCardForManaAvailability()
    {
        foreach (var card in PlayerHandCards)
            card.Info.HighlightManaAvaliability(CurrentGame.Player.Mana);

        
    }

    public void HightLightTargets(CardController attacker, bool highlight)
    {
        List<CardController> targets = new List<CardController>();

        if(attacker.Card.IsSpell)
        {
            if (attacker.Card.SpellTarget == Card.TargetType.NO_TARGET)
                targets = new List<CardController>();
            else if (attacker.Card.SpellTarget == Card.TargetType.ALLY_CARD_TARGET)
                targets = PlayerFieldCards;
            else
                targets = EnemyFieldCards;
        }
        else
        {
            if (EnemyFieldCards.Exists(x => x.Card.IsProvocation))
                targets = EnemyFieldCards.FindAll(x => x.Card.IsProvocation);
            else
            {
                targets = EnemyFieldCards;
                EnemyHero.HighlightAsTarget(highlight);
            }
        }
            

        foreach (var card in targets)
        {
            if (attacker.Card.IsSpell)
                card.Info.HighlightAsSpellTarget(highlight);
            else
                card.Info.HighlightAsTarget(highlight);
        }
            
    }
}
