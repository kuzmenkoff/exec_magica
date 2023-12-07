using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;

[Serializable]
public class Card
{

    public enum CardClass
    {
        /*0*/ENTITY,
        /*1*/ENTITY_WITH_ABILITY,
        /*2*/SPELL
    }
    public enum AbilityType
    {
        /*0*/NO_ABILITY,
        /*1*/LEAP,
        /*2*/PROVOCATION,
        /*3*/SHIELD,
        /*4*/SILENCE,
        /*5*/DOUBLE_ATTACK,
        /*6*/COUNTER_ATTACK,
        /*7*/REGENERATION_EACH_TURN_1,
        /*8*/REGENERATION_EACH_TURN_2,
        /*9*/INCREASE_ATTACK_EACH_TURN_1,
        /*10*/INCREASE_ATTACK_EACH_TURN_2,
        /*11*/VAMPIRISM,
        /*12*/HORDE,
        /*13*/
        ADDITIONAL_MANA_EACH_TURN_1,
        /*14*/
        ADDITIONAL_MANA_EACH_TURN_2,
        /*15*/
        ALLIES_INSPIRATION_1

    }

    public enum SpellType
    {
        NO_SPELL,
        HEAL_ALLY_FIELD_CARDS,
        DAMAGE_ENEMY_FIELD_CARDS,
        HEAL_ALLY_HERO,
        DAMAGE_ENEMY_HERO,
        HEAL_ALLY_CARD,
        SHIELD_ON_ALLY_CARD,
        PROVOCATION_ON_ALLY_CARD,
        BUFF_CARD_DAMAGE,
        DEBUFF_CARD_DAMAGE
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
        if(HP > 0)
        {
            return true;
        }
        return false;
    }

    public Card GetCopy()
    {
        Card card = this;
        card.Abilities = new List<AbilityType>(Abilities);
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
            if(card.id == CheckedCard.id)
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

    public void Awake()
    {
        allCardsDeck = new AllCards();
        MyDeck = new AllCards();
        EnemyDeck = new AllCards();

        string jsonToLoad = System.IO.File.ReadAllText("Assets/Resources/CardsInfo/AllCards.json");
        allCardsDeck = JsonUtility.FromJson<AllCards>(jsonToLoad);

        jsonToLoad = System.IO.File.ReadAllText("Assets/Resources/CardsInfo/MyDeck.json");
        MyDeck = JsonUtility.FromJson<AllCards>(jsonToLoad);
        
        jsonToLoad = System.IO.File.ReadAllText("Assets/Resources/CardsInfo/EnemyDeck.json");
        EnemyDeck = JsonUtility.FromJson<AllCards>(jsonToLoad);
    }

    public void SaveAllDecks()
    {
        string jsonToSave = JsonUtility.ToJson(MyDeck, true);
        System.IO.File.WriteAllText("Assets/Resources/CardsInfo/MyDeck.json", jsonToSave);
        jsonToSave = JsonUtility.ToJson(EnemyDeck, true);
        System.IO.File.WriteAllText("Assets/Resources/CardsInfo/EnemyDeck.json", jsonToSave);
    }

    public void DeleteCardFromDeck(AllCards Deck, Card card)
    {
        for (int i = 0; i < Deck.cards.Count; i++)
        {
            if(card.id == Deck.cards[i].id)
            {
                Deck.cards.RemoveAt(i);
            }
        }
    }

    public void AddCardToDeck(AllCards Deck, Card card)
    {
        Deck.cards.Add(card);
    }


}
