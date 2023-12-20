using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Dynamic;
using System.Reflection;
using System.IO;

public class ButtonManagerScr : MonoBehaviour
{
    public GameObject WhatToChangeMenu;
    public GameObject WarningObj;
    public GameObject CardLine;
    public GameObject CardPref;
    public Transform MyDeck;
    public Transform EnemyDeck;
    public Transform MyScrollView;
    public Transform EnemyScrollView;
    public DecksManagerScr DecksManager;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI WarningMsg;
    public TextMeshProUGUI DeckCounter;
    public Button ExitButton;
    public Button MyDeckButton;
    public Button EnemyDeckButton;
    public Button ChangeDeckButton;
    public Transform CardsLine;

    public GameSettings Settings = new GameSettings();

    private void Awake()
    {
        Settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText("Assets/Resources/Settings/Settings.json"));
        AudioListener.volume = Settings.soundVolume;
    }

    void Start()
    {
        DecksManager = gameObject.GetComponent<DecksManagerScr>();
        Title.text = "";
        DeckCounter.text = "";
        WarningMsg.text = "";
        MyDeck.gameObject.SetActive(false);
        MyScrollView.gameObject.SetActive(false);
        EnemyScrollView.gameObject.SetActive(false);
        WarningObj.SetActive(false);
        ExitButton.onClick.AddListener(OnExitButtonClicked);
        MyDeckButton.onClick.AddListener(OnMyDeckButtonClicked);
        EnemyDeckButton.onClick.AddListener(OnEnemyDeckButtonClicked);
        ChangeDeckButton.onClick.AddListener(OnChangeDeckButtonClicked);
        ShowDeck(MyDeck);
        ShowDeck(EnemyDeck);
        PaintCardsGreen(MyDeck, DecksManager.GetMyDeck());
        PaintCardsGreen(EnemyDeck, DecksManager.GetMyDeck());

    }

    public void OnExitButtonClicked()
    {
        EnemyScrollView.gameObject.SetActive(false);
        MyScrollView.gameObject.SetActive(false);
        EnemyDeck.gameObject.SetActive(false);
        MyDeck.gameObject.SetActive(false);
        if (DecksManager.GetEnemyDeck().cards.Count < DecksManager.MaxDeckLen || DecksManager.GetMyDeck().cards.Count < DecksManager.MaxDeckLen)
        {
            WarningObj.SetActive(true);

            if(DecksManager.GetMyDeck().cards.Count < DecksManager.MaxDeckLen)
                WarningMsg.text += "Player deck misses " + (DecksManager.MaxDeckLen - DecksManager.GetMyDeck().cards.Count).ToString() + " cards.";
            if (DecksManager.GetEnemyDeck().cards.Count < DecksManager.MaxDeckLen)
                WarningMsg.text += "\nEnemy deck misses " + (DecksManager.MaxDeckLen - DecksManager.GetEnemyDeck().cards.Count).ToString() + " cards.";
            WarningMsg.text += "\nMissing cards will be added automatically.";
        }
        else
        {
            Exit();
        }
    }

    public void Exit()
    {
        DecksManager.AddMissingCards();
        DecksManager.SaveAllDecks();
        SceneManager.LoadScene("MainMenu_Scene");
    }

    public void OnMyDeckButtonClicked()
    {
        EnemyScrollView.gameObject.SetActive(false);
        MyScrollView.gameObject.SetActive(true);
        EnemyDeck.gameObject.SetActive(false);
        WhatToChangeMenu.SetActive(false);
        Title.text = "My deck";
        DeckCounter.text = DecksManager.GetMyDeck().cards.Count.ToString() + " / 30";
        MyDeck.gameObject.SetActive(true);
    }

    public void OnEnemyDeckButtonClicked()
    {
        MyScrollView.gameObject.SetActive(false);
        EnemyScrollView.gameObject.SetActive(true);
        MyDeck.gameObject.SetActive(false);
        WhatToChangeMenu.SetActive(false);
        Title.text = "Enemy deck";
        DeckCounter.text = DecksManager.GetEnemyDeck().cards.Count.ToString() + " / 30";
        EnemyDeck.gameObject.SetActive(true);

    }

    public void OnChangeDeckButtonClicked()
    {
        Title.text = "";
        DeckCounter.text = "";
        MyDeck.gameObject.SetActive(false);
        EnemyDeck.gameObject.SetActive(false);
        WarningObj.SetActive(false);
        WhatToChangeMenu.SetActive(true);
        MyScrollView.gameObject.SetActive(false);
        EnemyScrollView.gameObject.SetActive(false);


    }

    public void ShowDeck(Transform Deck)
    {
        int NumOfCards = DecksManager.GetAllCards().cards.Count;

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

                //CardInfoScript cardInfo = newCard.GetComponent<CardInfoScript>();
                CardController cardC = newCard.GetComponent<CardController>();
                cardC.Init(DecksManager.GetAllCards().cards[i], true);

                //Debug.Log(cardC.Card.HP);
                if (cardC.Info != null)
                {
                    //CC.Info.ShowCardInfo();
                    cardC.Info.ShowCardInfo();


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

                CardController CC = Card.GetComponent<CardController>();
                if (cards.ContainsCard(CC.Card))
                {
                    CC.Info.PaintGreen();
                }
            }
        }
    }

    public void UpdateDeckCounters()
    {

        Debug.Log("Update called");
        if (MyDeck.gameObject.activeSelf)
        {
            DeckCounter.text = DecksManager.GetMyDeck().cards.Count.ToString() + " / 30";
        }
        else if (EnemyDeck.gameObject.activeSelf)
        {
            DeckCounter.text = DecksManager.GetEnemyDeck().cards.Count.ToString() + " / 30";
        }
    }





}
