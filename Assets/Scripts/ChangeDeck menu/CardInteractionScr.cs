using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteractionScr : MonoBehaviour, IPointerExitHandler, IPointerDownHandler
{
    CardInfoScript cardInfo;
    ButtonManagerScr buttonManager;
    UnityEngine.Color OriginalColor;
    public AudioSource audioSource;
    Camera MainCamera;
    DecksManagerScr DecksManager;
    UnityEngine.Color GreenColor;

    void Start()
    {
        GreenColor = new UnityEngine.Color(13f / 255f, 142f / 255f, 0f / 255f, 1f);
        cardInfo = GetComponent<CardInfoScript>();
        MainCamera = Camera.allCameras[0];
        buttonManager = MainCamera.GetComponent<ButtonManagerScr>();
        
        OriginalColor = cardInfo.card_BG.color;



    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cardInfo.PaintAnother(OriginalColor);                                  
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
        if (buttonManager.MyDeck.gameObject.activeSelf)
        {
            if (buttonManager.DecksManager.MyDeck.cards.Count <= buttonManager.DecksManager.MinDeckLen && cardInfo.card_BG.color.Equals(GreenColor))
            {
                return;
            }
            ChangeCardColor();
            buttonManager.ChangeDeck(buttonManager.DecksManager.MyDeck, cardInfo.SelfCard);
        }
        else if (buttonManager.EnemyDeck.gameObject.activeSelf)
        {
            if (buttonManager.DecksManager.EnemyDeck.cards.Count <= buttonManager.DecksManager.MinDeckLen && cardInfo.card_BG.color.Equals(GreenColor))
            {
                return;
            }
            ChangeCardColor();
            buttonManager.ChangeDeck(buttonManager.DecksManager.EnemyDeck, cardInfo.SelfCard);
        }
    }

    public void ChangeCardColor()
    {
        audioSource.Play();
        
        if (OriginalColor.Equals(GreenColor))
        {
            cardInfo.PaintWhite();
            OriginalColor = cardInfo.card_BG.color;
        }
        else
        {
            cardInfo.PaintGreen();
            OriginalColor = cardInfo.card_BG.color;
        }
    }
}
