using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

//[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
//[Serializable]
public struct aCard
{
    public string Title, Description, Class;
    public Sprite Logo;
    public int Attack, HP, ManaCost;

    public aCard(string title, string description, string Class, string logoPath, int attack, int hp, int manaCost)
    {
        Title = title;
        Description = description;
        this.Class = Class;
        Logo = Resources.Load<Sprite>(logoPath);
        Attack = attack;
        HP = hp;
        ManaCost = manaCost;
    }
}

public static class CardManager
{
    public static List<aCard> AllCards = new List<aCard>();
}

public class CardManagerScr : MonoBehaviour
{
    public void Awake()
    {
        CardManager.AllCards.Add(new aCard("Card1", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 5, 5, 5));
        CardManager.AllCards.Add(new aCard("Card2", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 1, 2, 3));
        CardManager.AllCards.Add(new aCard("Card3", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 4, 5, 2));
        CardManager.AllCards.Add(new aCard("Card4", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 2, 5, 8));
        CardManager.AllCards.Add(new aCard("Card5", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 1, 9, 5));
        CardManager.AllCards.Add(new aCard("Card6", "This is desdcription!", "Entity", "CardsLogos/Shadow_Fiend_Lore", 3, 2, 2));
    }
    
}
