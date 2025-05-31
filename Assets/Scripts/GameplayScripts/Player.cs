using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;

public abstract class BasePlayer
{
    public int HP, Mana, Manapool;
    public const int MAX_MANAPOOL = 10, MAX_CARDS_ON_HAND = 8;


    public abstract void RestoreRoundMana();
    public abstract void IncreaseManapool();
    public abstract void GetDamage(int damage);
}

public class Player : BasePlayer
{
    public AllCards DeckCards { get; set; }
    public AllCards OriginalDeck { get; set; }

    public List<CardController> HandCards = new List<CardController>();
    public List<CardController> FieldCards = new List<CardController>();

    public Player(AllCards Deck)
    {
        HP = 30;
        Mana = Manapool = 1;
        DeckCards = Deck.GetDeepCopy();
        DeckCards.Shuffle();
        OriginalDeck = Deck.GetDeepCopy();
    }

    public override void RestoreRoundMana()
    {
        Mana = Manapool;
        UIController.Instance.UpdateHPAndMana();
    }

    public override void IncreaseManapool()
    {
        Manapool = Mathf.Clamp(Manapool + 1, 0, MAX_MANAPOOL);
        UIController.Instance.UpdateHPAndMana();
    }

    public override void GetDamage(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, int.MaxValue);
        UIController.Instance.UpdateHPAndMana();
    }

    public int GetMaxManapool() => MAX_MANAPOOL;

    public void RenewDeck()
    {
        DeckCards = OriginalDeck.GetDeepCopy();
        DeckCards.Shuffle();
    }

    public SimPlayer ToSimPlayer()
    {
        var simPlayer = new SimPlayer(this.OriginalDeck.GetDeepCopy())
        {
            HP = this.HP,
            Mana = this.Mana,
            Manapool = this.Manapool
        };

        // Копіюємо карти з руки
        foreach (var cardCtrl in HandCards)
        {
            if (cardCtrl != null && cardCtrl.Card != null)
                simPlayer.HandCards.Add(cardCtrl.Card.GetDeepCopy());
        }

        // Копіюємо карти з поля
        foreach (var cardCtrl in FieldCards)
        {
            if (cardCtrl != null && cardCtrl.Card != null)
                simPlayer.FieldCards.Add(cardCtrl.Card.GetDeepCopy());
        }

        return simPlayer;
    }


}

public class SimPlayer : BasePlayer
{
    public List<Card> HandCards = new List<Card>();
    public List<Card> FieldCards = new List<Card>();
    public AllCards DeckCards { get; set; }
    public AllCards OriginalDeck { get; set; }

    public SimPlayer(AllCards deck)
    {
        HP = 30;
        Mana = Manapool = 1;
        DeckCards = deck.GetDeepCopy();
        DeckCards.Shuffle();
        OriginalDeck = deck.GetDeepCopy();
    }

    public SimPlayer() { }

    public SimPlayer GetDeepCopy()
    {
        var copy = new SimPlayer()
        {
            HP = this.HP,
            Mana = this.Mana,
            Manapool = this.Manapool,

            DeckCards = this.DeckCards.GetDeepCopy(),
            OriginalDeck = this.OriginalDeck.GetDeepCopy(),

            HandCards = this.HandCards.Where(c => c != null).Select(c => c.GetDeepCopy()).ToList(),
            FieldCards = this.FieldCards.Where(c => c != null).Select(c => c.GetDeepCopy()).ToList()
        };

        return copy;
    }

    public override void RestoreRoundMana()
    {
        Mana = Manapool;
    }

    public override void IncreaseManapool()
    {
        Manapool = Math.Min(Manapool + 1, MAX_MANAPOOL);
    }

    public override void GetDamage(int damage)
    {
        HP = Math.Max(0, HP - damage);
    }

    public void GiveCards(int count, int maxValue)
    {
        for (int i = 0; i < count; i++)
        {
            if (HandCards.Count == maxValue)
                break;

            if (DeckCards.cards.Count == 0)
            {
                DeckCards.cards.AddRange(OriginalDeck.GetDeepCopy().cards);
                DeckCards.Shuffle();
            }

            if (DeckCards.cards.Count > 0)
            {
                var card = DeckCards.cards[0];

                // Призначити InstanceId, якщо ще не призначений (0 — умовно «непризначений»)
                if (card.InstanceId == 0)
                    card.AssignNewInstanceId();

                HandCards.Add(card);
                DeckCards.cards.RemoveAt(0);
            }
        }
    }

}


