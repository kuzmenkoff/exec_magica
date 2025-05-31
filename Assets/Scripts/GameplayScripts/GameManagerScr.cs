using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IPlayerModel
{
    void MakeMove();
    string Name { get; }
}

public class GameManagerScr : MonoBehaviour
{
    const int MAX_CARDS_ON_FIELD = 6;
    const int MAX_CARDS_ON_HAND = 8;
    const int STARTER_NUMBER_OF_CARDS_ON_HAND = 5;

    public Player Player, Enemy;

    public GameSettings Settings;

    public bool PlayerTurn;
    public bool PlayerFirst;
    public bool Player1Win;
    public int Turn;

    public static GameManagerScr Instance;

    public DecksManagerScr decksManager;

    public Transform EnemyHand, PlayerHand,
                     EnemyField, PlayerField;
    public GameObject CardPref;
    public int TurnTime;


    public AttackedHero EnemyHero, PlayerHero;

    public IPlayerModel EnemyModel;

    public void Awake()
    {
        decksManager = GetComponent<DecksManagerScr>();

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

        AudioListener.volume = Settings.soundVolume;

        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    public GameState getGameState()
    {
        return new GameState(Player.ToSimPlayer(), Enemy.ToSimPlayer(), PlayerTurn, Turn);
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

        foreach (var card in Player.HandCards)
            Destroy(card.gameObject);
        foreach (var card in Player.FieldCards)
            Destroy(card.gameObject);
        foreach (var card in Enemy.HandCards)
            Destroy(card.gameObject);
        foreach (var card in Enemy.FieldCards)
            Destroy(card.gameObject);

        Player.HandCards.Clear();
        Player.FieldCards.Clear();
        Enemy.HandCards.Clear();
        Player.FieldCards.Clear();

        UIController.Instance.pausePanel.SetActive(false);
        UIController.Instance.ResumeGame();

        StartGame();
    }

    void StartGame()
    {

        Time.timeScale = 1f;

        Player = new Player(decksManager.GetMyDeck());
        Enemy = new Player(decksManager.GetEnemyDeck());

        string path = Application.persistentDataPath;


        UIController.Instance.EnableTurnTime(Settings.timerIsOn);

        GiveStarterCards(Enemy.DeckCards.cards, EnemyHand, false);
        GiveStarterCards(Player.DeckCards.cards, PlayerHand, true);

        UIController.Instance.WhoseTurnUpdate();
        UIController.Instance.EnableTurnBtn();

        UIController.Instance.UpdateHPAndMana();

        UIController.Instance.StartGame();

        switch (Settings.difficulty) {
            case "Easy":
                EnemyModel = new RandomModel(Enemy, Player, "RandomModel");
                break;
            case "Normal":
                EnemyModel = new FlatMCModel(Enemy, Player, "FlatMCModel");
                break;
            case "Hard":
                EnemyModel = new MCTSModel(Enemy, Player, "MCTSModel");
                break;
        }

        StartCoroutine(TurnFunc());
    }

    void GiveStarterCards(List<Card> deck, Transform hand, bool player)
    {
        int i = 0;
        while (i++ < STARTER_NUMBER_OF_CARDS_ON_HAND)
        {
            GiveCardToHand(deck, hand, player);
        }
    }
    void GiveCardToHand(List<Card> deck, Transform hand, bool player)
    {
        if ((player && Player.HandCards.Count >= MAX_CARDS_ON_HAND) || (!player && Enemy.HandCards.Count >= MAX_CARDS_ON_HAND))
            return;
        if (deck.Count == 0)
            return;

        CreateCardPref(deck[0], hand);

        deck.RemoveAt(0);

    }

    void CreateCardPref(Card card, Transform hand)
    {
        card.AssignNewInstanceId();
        GameObject cardGO = Instantiate(CardPref, hand, false);
        cardGO.SetActive(true);
        CardController cardC = cardGO.GetComponent<CardController>();

        cardC.Init(card, hand == PlayerHand);
        if (cardC.IsPlayerCard)
            Player.HandCards.Add(cardC);
        else
            Enemy.HandCards.Add(cardC);
    }

    public CardController GetCardCByInstanceId(int instanceId)
    {
        var allCards = Player.HandCards
            .Concat(Player.FieldCards)
            .Concat(Enemy.HandCards)
            .Concat(Enemy.FieldCards);

        var match = allCards.FirstOrDefault(c => c.Card.InstanceId == instanceId);
        return match != null ? match : null;
    }

    public GameObject GetCardGOByInstanceId(int instanceId)
    {
        var allCards = Player.HandCards
            .Concat(Player.FieldCards)
            .Concat(Enemy.HandCards)
            .Concat(Enemy.FieldCards);

        var match = allCards.FirstOrDefault(c => c.Card.InstanceId == instanceId);
        return match != null ? match.gameObject : null;
    }

    IEnumerator TurnFunc()
    {
        foreach (var card in Player.FieldCards)
            card.Info.PaintWhite();

        if (Settings.timerIsOn)
        {
            TurnTime = Settings.timer;
            UIController.Instance.UpdateTurnTime(TurnTime);
        }
        else
            TurnTime = int.MaxValue;

        CheckCardForManaAvailability();

        if (PlayerTurn)
        {
            foreach (var card in Player.FieldCards)
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
            foreach (var card in Enemy.FieldCards)
            {
                card.Card.CanAttack = true;
                card.Ability.OnNewTurn();
            }

            EnemyModel.MakeMove();

            //ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        StopAllCoroutines();
        Turn++;
        PlayerTurn = !PlayerTurn;
        UIController.Instance.EnableTurnBtn();
        UIController.Instance.WhoseTurnUpdate();


        if (PlayerTurn)
        {
            if (Player.DeckCards.cards.Count == 0)
                Player.RenewDeck();
            GiveCardToHand(Player.DeckCards.cards, PlayerHand, true);
            if (Turn > 2)
                Player.IncreaseManapool();
            Player.RestoreRoundMana();

        }
        else
        {
            if (Enemy.DeckCards.cards.Count == 0)
                Enemy.RenewDeck();
            GiveCardToHand(Enemy.DeckCards.cards, EnemyHand, false);
            if (Turn > 2)
                Enemy.IncreaseManapool();
            Enemy.RestoreRoundMana();
        }
        StartCoroutine(TurnFunc());
    }

    public void CardsFight(CardController attacker, CardController defender)
    {
        attacker.Card.CanAttack = false;

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
            Player.Mana -= manacost;
        else
            Enemy.Mana -= manacost;
        UIController.Instance.UpdateHPAndMana();
    }

    public void DamageHero(CardController card, Player target)
    {
        target.GetDamage(card.Card.Attack);

        UIController.Instance.UpdateHPAndMana();
        card.OnDamageDeal();
        CheckForVictory();
    }

    public void CheckForVictory()
    {
        if (Enemy.HP <= 0 || Player.HP <= 0)
        {
            StopAllCoroutines();
            Time.timeScale = 0f;
            UIController.Instance.ShowResult();
        }
    }

    public void CheckCardForManaAvailability()
    {
        foreach (var card in Player.HandCards)
            card.Info.HighlightManaAvaliability(Player.Mana);
    }

    public void HightLightTargets(CardController attacker, bool highlight)
    {
        List<CardController> targets = new List<CardController>();

        if (attacker.Card.IsSpell)
        {
            if (attacker.Card.SpellTarget == Card.TargetType.NO_TARGET)
                targets = new List<CardController>();
            else if (attacker.Card.SpellTarget == Card.TargetType.ALLY_CARD_TARGET)
                targets = Player.FieldCards;
            else
                targets = Enemy.FieldCards;
        }
        else
        {
            if (Enemy.FieldCards.Exists(x => x.Card.IsProvocation))
                targets = Enemy.FieldCards.FindAll(x => x.Card.IsProvocation);
            else
            {
                targets = Enemy.FieldCards;
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
