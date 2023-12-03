using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game
{
    public List<aCard> EnemyDeck, PlayerDeck, EnemyHand, PlayerHand, EnemyField, PlayerField;
    public int StarterCardsNum = 4;

    public Game()
    {
        EnemyDeck = CardManager.AllCards;
        PlayerDeck = CardManager.AllCards;

        EnemyHand = new List<aCard>();
        PlayerHand = new List<aCard>();

        EnemyField = new List<aCard>();
        PlayerField = new List<aCard>();
    }

    List<aCard> GiveDeckCard()
    {
        List<aCard> list = new List<aCard>();
        for (int i = 0; i < StarterCardsNum; i++)
        {
            list.Add(CardManager.AllCards[Random.Range(0, CardManager.AllCards.Count)]);
        }
        return list;
    }
}

public class GameManagerScr : MonoBehaviour
{
    public Game CurrentGame;
    public Transform EnemyHand, PlayerHand;
    public GameObject CardPref;
    
    /*void Start()
    {
        CurrentGame = new Game();
        GiveHandCards(CurrentGame.EnemyDeck, EnemyHand);
        GiveHandCards(CurrentGame.PlayerDeck, PlayerHand);
    }

    void GiveHandCards (List<Card> deck, Transform hand)
    {
        int i = 0;
        while (i++ < CurrentGame.StarterCardsNum - 1)
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

        if (hand == EnemyHand)
            cardGO.GetComponent<CardInfoScript>().HideCardInfo(card);
        else
            cardGO.GetComponent<CardInfoScript>().ShowCardInfo(card);

        deck.RemoveAt(0);

    }*/
}
