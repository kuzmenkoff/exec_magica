using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public Player Player, Enemy;
    public DecksManagerScr DecksManager;
    public List<Card> EnemyDeck, PlayerDeck;
    public int StarterCardsNum = 4;
    public GameSettings Settings;

    public Game(DecksManagerScr decksManager)
    {
        DecksManager = decksManager;

        EnemyDeck = new List<Card>(DecksManager.GetEnemyDeckCopy().cards);
        PlayerDeck = new List<Card>(DecksManager.GetMyDeckCopy().cards);
        List<Card> ShuffledDeck = ShuffleDeck(EnemyDeck);
        EnemyDeck = ShuffledDeck;
        ShuffledDeck = ShuffleDeck(PlayerDeck);
        PlayerDeck = ShuffledDeck;

        Player = new Player();
        Enemy = new Player();

        Settings = new GameSettings();
        string filePath = Path.Combine(Application.persistentDataPath, "Settings.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings.soundVolume = .5f;
            Settings.timer = 120;
            Settings.timerIsOn = true;
            Settings.difficulty = "Normal";
        }
    }

    public List<Card> ShuffleDeck(List<Card> Deck)
    {
        Card temp;
        System.Random random = new System.Random();
        // FisherЦYates shuffle
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
    public int Turn = 1, TurnTime, OriginalTurnTime;
    public bool TimerIsOn, PlayerIsFirst, PlayersTurn;

    public string Difficulty;

    public AttackedHero EnemyHero, PlayerHero;
    public AI EnemyAI;
    public List<CardController> PlayerHandCards = new List<CardController>(),
                                PlayerFieldCards = new List<CardController>(),
                                EnemyHandCards = new List<CardController>(),
                                EnemyFieldCards = new List<CardController>();

    public GameSettings Settings = new GameSettings();

    public void Awake()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Settings.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings.soundVolume = .5f;
            Settings.timer = 120;
            Settings.timerIsOn = true;
            Settings.difficulty = "Normal";
        }
        AudioListener.volume = Settings.soundVolume;

        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu_Scene");
    }

    public void PauseGame()
    {
        UIController.Instance.PauseGame();
    }

    public void ResumeGame()
    {
        UIController.Instance.ResumeGame();
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

        UIController.Instance.pausePanel.SetActive(false);
        UIController.Instance.ResumeGame();

        StartGame();
    }

    void StartGame()
    {
        Time.timeScale = 1f;

        decksManager = GetComponent<DecksManagerScr>();
        CurrentGame = new Game(decksManager);

        OriginalTurnTime = CurrentGame.Settings.timer;
        TimerIsOn = CurrentGame.Settings.timerIsOn;
        Difficulty = CurrentGame.Settings.difficulty;

        UIController.Instance.EnableTurnTime(TimerIsOn);
        PlayerIsFirst = FlipCoin();
        PlayersTurn = PlayerIsFirst;

        UIController.Instance.EnableTurnTime(TimerIsOn);

        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand, false);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand, true);

        UIController.Instance.WhoseTurnUpdate();
        UIController.Instance.EnableTurnBtn();


        if (PlayersTurn)
            GiveCardToHand(CurrentGame.PlayerDeck, PlayerHand, true);
        else
            GiveCardToHand(CurrentGame.EnemyDeck, EnemyHand, false);

        Turn = 0;

        CurrentGame.Player.Mana = CurrentGame.Player.Manapool = 1;
        CurrentGame.Enemy.Mana = CurrentGame.Enemy.Manapool = 1;

        UIController.Instance.UpdateHPAndMana();

        UIController.Instance.StartGame();

        StartCoroutine(TurnFunc());
    }

    void GiveHandCards(List<Card> deck, Transform hand, bool player)
    {
        int i = 0;
        while (i++ < CurrentGame.StarterCardsNum)
        {
            GiveCardToHand(deck, hand, player);
        }
    }
    void GiveCardToHand(List<Card> deck, Transform hand, bool player)
    {
        if ((player && PlayerHandCards.Count >= 8) || (!player && EnemyHandCards.Count >= 8))
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
                Debug.Log(card.Card.CanAttack);
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


            StartCoroutine(EnemyAITurn());
            while (TurnTime-- > 0)
            {
                UIController.Instance.UpdateTurnTime(TurnTime);
                yield return new WaitForSeconds(1);
            }

            //ChangeTurn();
        }
    }

    IEnumerator EnemyAITurn()
    {
        EnemyAI.MakeTurn();
        yield return null; // Ёто нужно, чтобы корутина корректно завершилась
    }



    public void RenewDeck(bool playerdeck)
    {
        if (playerdeck)
        {

            CurrentGame.PlayerDeck = new List<Card>(decksManager.GetMyDeckCopy().cards);
            CurrentGame.PlayerDeck = CurrentGame.ShuffleDeck(CurrentGame.PlayerDeck);
        }
        else
        {
            CurrentGame.EnemyDeck = new List<Card>(decksManager.GetEnemyDeckCopy().cards);
            CurrentGame.EnemyDeck = CurrentGame.ShuffleDeck(CurrentGame.EnemyDeck);
        }
    }

    public void ChangeTurn()
    {
        StopAllCoroutines();
        Turn++;
        PlayersTurn = !PlayersTurn;
        UIController.Instance.EnableTurnBtn();
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
            if (Turn != 1)
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

        attacker.CheckForAlive();
        defender.CheckForAlive();
    }


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
            Time.timeScale = 0f;
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

        if (attacker.Card.IsSpell)
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
