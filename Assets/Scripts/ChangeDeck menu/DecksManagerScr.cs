using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Card;

[Serializable]
public class Card
{

    public enum CardClass
    {
        /*0*/
        ENTITY,
        /*1*/
        ENTITY_WITH_ABILITY,
        /*2*/
        SPELL
    }
    public enum AbilityType
    {
        /*0*/
        NO_ABILITY,
        /*1*/
        LEAP,
        /*2*/
        PROVOCATION,
        /*3*/
        SHIELD,
        /*4*/
        DOUBLE_ATTACK,
        /*5*/
        REGENERATION_EACH_TURN,
        /*6*/
        INCREASE_ATTACK_EACH_TURN,
        /*7*/
        HORDE,
        /*8*/
        ADDITIONAL_MANA_EACH_TURN,
        /*9*/
        ALLIES_INSPIRATION,
        /*10*/
        EXHAUSTION
    }

    public enum SpellType
    {
        /*0*/
        NO_SPELL,
        /*1*/
        HEAL_ALLY_FIELD_CARDS,
        /*2*/
        DAMAGE_ENEMY_FIELD_CARDS,
        /*3*/
        HEAL_ALLY_HERO,
        /*4*/
        DAMAGE_ENEMY_HERO,
        /*5*/
        HEAL_ALLY_CARD,
        /*6*/
        SHIELD_ON_ALLY_CARD,
        /*7*/
        PROVOCATION_ON_ALLY_CARD,
        /*8*/
        BUFF_CARD_DAMAGE,
        /*9*/
        DEBUFF_CARD_DAMAGE,
        /*10*/
        SILENCE,
        /*11*/
        KILL_ALL

    }

    public enum TargetType
    {
        NO_TARGET,
        ALLY_CARD_TARGET,
        ENEMY_CARD_TARGET
    }

    public int id;
    public string Title, Description, LogoPath;
    public CardClass Class;
    public int Attack, HP, ManaCost;
    public bool CanAttack;
    public bool IsPlaced;

    public List<AbilityType> Abilities;
    public SpellType Spell;
    public TargetType SpellTarget;
    public int SpellValue;

    public int TimesTookDamage;
    public int TimesDealedDamage;

    public bool HasAbility
    {
        get { return !Abilities.Exists(x => x == AbilityType.NO_ABILITY); }
    }

    public bool IsProvocation
    {
        get { return Abilities.Exists(x => x == AbilityType.PROVOCATION); }
    }

    public bool IsSpell
    {
        get { return Spell != SpellType.NO_SPELL; }
    }

    public void GetDamage(int dmg)
    {
        if (dmg >= 0)
        {
            if (Abilities.Exists(x => x == AbilityType.SHIELD))
            {
                Abilities.Remove(AbilityType.SHIELD);
                if (Abilities.Count == 0)
                {
                    Abilities.Add(AbilityType.NO_ABILITY);
                }
            }
            else
                HP -= dmg;
        }

    }

    public bool IsAlive()
    {
        if (HP > 0)
        {
            return true;
        }
        return false;
    }

    public Card GetCopy()
    {
        Card card = new Card();
        card = this;
        //card.Abilities = new List<AbilityType>(Abilities);
        return card;
    }

    public Card GetDeepCopy()
    {
        Card card = new Card();

        // Копируем простые и перечисляемые типы данных
        card.id = this.id;
        card.Title = this.Title;
        card.Description = this.Description;
        card.LogoPath = this.LogoPath;
        card.Class = this.Class;
        card.Attack = this.Attack;
        card.HP = this.HP;
        card.ManaCost = this.ManaCost;
        card.CanAttack = this.CanAttack;
        card.IsPlaced = this.IsPlaced;
        card.Spell = this.Spell;
        card.SpellTarget = this.SpellTarget;
        card.SpellValue = this.SpellValue;
        card.TimesTookDamage = this.TimesTookDamage;
        card.TimesDealedDamage = this.TimesDealedDamage;

        // Для коллекций создаем новые экземпляры (глубокое копирование)
        card.Abilities = new List<AbilityType>(this.Abilities);

        return card;
    }
}

public class AllCards
{
    public List<Card> cards = new List<Card>();

    public bool ContainsCard(Card CheckedCard)
    {
        foreach (Card card in cards)
        {
            if (card.id == CheckedCard.id)
            {
                return true;
            }
        }
        return false;
    }
}

public class DecksManagerScr : MonoBehaviour
{
    private AllCards allCardsDeck;
    private AllCards MyDeck;
    private AllCards EnemyDeck;
    public int MinDeckLen = 5;
    public int MaxDeckLen = 30;

    public AllCards GetAllCards() { return allCardsDeck; }
    public AllCards GetMyDeck() { return MyDeck; }
    public AllCards GetEnemyDeck() { return EnemyDeck; }
    public AllCards GetMyDeckCopy()
    {
        AllCards deck = new AllCards();
        foreach (Card card in MyDeck.cards)
        {
            deck.cards.Add(card.GetDeepCopy());
        }
        return deck;
    }
    public AllCards GetEnemyDeckCopy()
    {
        AllCards deck = new AllCards();
        foreach (Card card in EnemyDeck.cards)
        {
            deck.cards.Add(card.GetDeepCopy());
        }
        return deck;
    }

    public void Awake()
    {
        allCardsDeck = new AllCards();
        MyDeck = new AllCards();
        EnemyDeck = new AllCards();

        TextAsset allCardsText = Resources.Load<TextAsset>("CardsInfo/AllCards");
        allCardsDeck = JsonUtility.FromJson<AllCards>(allCardsText.text);

        LoadOrCreateDeck(ref MyDeck, "MyDeck.json");
        LoadOrCreateDeck(ref EnemyDeck, "EnemyDeck.json");
        UpdateDecksInfo();
    }

    private void LoadOrCreateDeck(ref AllCards deck, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            deck = JsonUtility.FromJson<AllCards>(json);
        }
        else
        {

            for (int i = 0; i < Math.Min(30, allCardsDeck.cards.Count); i++)
                deck.cards.Add(allCardsDeck.cards[i]);
            SaveDeck(deck, filePath);
        }
    }

    public void UpdateDecksInfo()
    {
        foreach (Card card in MyDeck.cards)
        {
            Card allCardsDeckCard = allCardsDeck.cards.Find(x => x.id == card.id);

            card.Title = allCardsDeckCard.Title;
            card.Description = allCardsDeckCard.Description;
            card.LogoPath = allCardsDeckCard.LogoPath;
            card.Class = allCardsDeckCard.Class;
            card.Attack = allCardsDeckCard.Attack;
            card.HP = allCardsDeckCard.HP;
            card.ManaCost = allCardsDeckCard.ManaCost;
            card.CanAttack = allCardsDeckCard.CanAttack;
            card.IsPlaced = allCardsDeckCard.IsPlaced;
            card.Spell = allCardsDeckCard.Spell;
            card.SpellTarget = allCardsDeckCard.SpellTarget;
            card.SpellValue = allCardsDeckCard.SpellValue;
            card.TimesTookDamage = allCardsDeckCard.TimesTookDamage;
            card.TimesDealedDamage = allCardsDeckCard.TimesDealedDamage;

            card.Abilities = new List<AbilityType>(allCardsDeckCard.Abilities);
        }

        foreach (Card card in EnemyDeck.cards)
        {
            Card allCardsDeckCard = allCardsDeck.cards.Find(x => x.id == card.id);

            card.Title = allCardsDeckCard.Title;
            card.Description = allCardsDeckCard.Description;
            card.LogoPath = allCardsDeckCard.LogoPath;
            card.Class = allCardsDeckCard.Class;
            card.Attack = allCardsDeckCard.Attack;
            card.HP = allCardsDeckCard.HP;
            card.ManaCost = allCardsDeckCard.ManaCost;
            card.CanAttack = allCardsDeckCard.CanAttack;
            card.IsPlaced = allCardsDeckCard.IsPlaced;
            card.Spell = allCardsDeckCard.Spell;
            card.SpellTarget = allCardsDeckCard.SpellTarget;
            card.SpellValue = allCardsDeckCard.SpellValue;
            card.TimesTookDamage = allCardsDeckCard.TimesTookDamage;
            card.TimesDealedDamage = allCardsDeckCard.TimesDealedDamage;

            card.Abilities = new List<AbilityType>(allCardsDeckCard.Abilities);
        }
    }

    public void SaveAllDecks()
    {
        SaveDeck(MyDeck, Path.Combine(Application.persistentDataPath, "MyDeck.json"));
        SaveDeck(EnemyDeck, Path.Combine(Application.persistentDataPath, "EnemyDeck.json"));
    }

    private void SaveDeck(AllCards deck, string filePath)
    {
        string json = JsonUtility.ToJson(deck, true);
        File.WriteAllText(filePath, json);
    }

    public void DeleteCardFromDeck(AllCards Deck, Card card)
    {
        for (int i = 0; i < Deck.cards.Count; i++)
        {
            if (card.id == Deck.cards[i].id)
            {
                Deck.cards.RemoveAt(i);
            }
        }
    }

    public void AddCardToDeck(AllCards Deck, Card card)
    {
        Deck.cards.Add(card);
    }

    public void AddMissingCards()
    {
        if (MyDeck.cards.Count < MaxDeckLen)
        {
            foreach (Card card in allCardsDeck.cards)
            {
                if (!MyDeck.cards.Contains(card))
                    AddCardToDeck(MyDeck, card);
                if (MyDeck.cards.Count >= MaxDeckLen)
                    break;
            }
        }
        if (EnemyDeck.cards.Count < MaxDeckLen)
        {
            foreach (Card card in allCardsDeck.cards)
            {
                if (!MyDeck.cards.Contains(card))
                    AddCardToDeck(EnemyDeck, card);
                if (EnemyDeck.cards.Count >= MaxDeckLen)
                    break;
            }
        }
    }


}
