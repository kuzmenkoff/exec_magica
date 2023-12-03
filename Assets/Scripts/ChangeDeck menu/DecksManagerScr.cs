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
    public int id;
    public string Title, Description, Class, SpecialAbility, LogoPath;
    public int Attack, HP, ManaCost;
    //public Sprite Logo;
    

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
    public AllCards allCardsDeck;
    public AllCards MyDeck;
    public AllCards EnemyDeck;
    public int MinDeckLen = 5;
    public int MaxDeckLen = 30;
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
