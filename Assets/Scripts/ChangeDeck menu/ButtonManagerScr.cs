using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Dynamic;

public class ButtonManagerScr : MonoBehaviour
{
    public GameObject WhatToChangeMenu;
    public GameObject CardLine;
    public GameObject CardPref;
    public Transform MyDeck;
    public Transform EnemyDeck;
    public DecksManagerScr DecksManager;
    public TextMeshProUGUI Title;
    public Button ExitButton;
    public Button MyDeckButton;
    public Button EnemyDeckButton;
    public Button ChangeDeckButton;
    public Transform CardsLine;

    void Start()
    {
        DecksManager = gameObject.GetComponent<DecksManagerScr>();
        Title.text = "";
        MyDeck.gameObject.SetActive(false);
        ExitButton.onClick.AddListener(OnExitButtonClicked);
        MyDeckButton.onClick.AddListener(OnMyDeckButtonClicked);
        EnemyDeckButton.onClick.AddListener(OnEnemyDeckButtonClicked);
        ChangeDeckButton.onClick.AddListener(OnChangeDeckButtonClicked);
        ShowDeck(MyDeck);
        ShowDeck(EnemyDeck);
        PaintCardsGreen(MyDeck, DecksManager.MyDeck);
        PaintCardsGreen(EnemyDeck, DecksManager.EnemyDeck);

    }

    public void OnExitButtonClicked()
    {
        SceneManager.LoadScene("MainMenu_Scene");
        DecksManager.SaveAllDecks();
    }

    public void OnMyDeckButtonClicked()
    {
        EnemyDeck.gameObject.SetActive(false);
        WhatToChangeMenu.SetActive(false);
        Title.text = "My deck";
        MyDeck.gameObject.SetActive(true);
    }

    public void OnEnemyDeckButtonClicked()
    {
        MyDeck.gameObject.SetActive(false);
        WhatToChangeMenu.SetActive(false);
        Title.text = "Enemy deck";
        EnemyDeck.gameObject.SetActive(true);

    }

    public void OnChangeDeckButtonClicked()
    {
        Title.text = "";
        MyDeck.gameObject.SetActive(false);
        EnemyDeck.gameObject.SetActive(false);
        WhatToChangeMenu.SetActive(true);
        

    }

    public void ShowDeck(Transform Deck)
    {
        int NumOfCards = DecksManager.allCardsDeck.cards.Count;

        for (int i = 0; i < NumOfCards; i++)
        {
            Transform newCardLine = Instantiate(CardsLine, Deck, false);
            newCardLine.transform.SetParent(Deck.transform, false);
            newCardLine.gameObject.SetActive(true);

            
            for (int j = 0; j < 8 && i < NumOfCards; j++)
            {
                GameObject newCard = Instantiate(CardPref, newCardLine, false);
                newCard.SetActive(true);
                newCard.transform.SetParent(newCardLine.transform, false);

                CardInfoScript cardInfo = newCard.GetComponent<CardInfoScript>();
                if (cardInfo != null)
                {
                    cardInfo.ShowCardInfo(DecksManager.allCardsDeck.cards[i]);
                 
                    
                }
                i++;
            }


            i--;
        }
    }

    public void ChangeDeck(AllCards Deck, Card card)
    {
        if (Deck.ContainsCard(card))
        {
            DecksManager.DeleteCardFromDeck(Deck, card);
        }
        else
        {
            DecksManager.AddCardToDeck(Deck, card);
        }
    }

    public void PaintCardsGreen(Transform Deck, AllCards cards)
    {
        foreach (Transform cardline in Deck)
        {
            foreach (Transform Card in cardline)
            {
                
                CardInfoScript cardInfo = Card.GetComponentInChildren<CardInfoScript>();
                if (cards.ContainsCard(cardInfo.SelfCard))
                {
                    cardInfo.PaintGreen();
                }
            }
        }
    }

    




}
