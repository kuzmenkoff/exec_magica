using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteractionScr : MonoBehaviour, IPointerExitHandler, IPointerDownHandler
{
    CardController CC;
    ButtonManagerScr buttonManager;
    UnityEngine.Color OriginalColor;
    public AudioSource audioSource;
    Camera MainCamera;
    DecksManagerScr DecksManager;
    UnityEngine.Color GreenColor;

    void Start()
    {
        GreenColor = new UnityEngine.Color(13f / 255f, 142f / 255f, 0f / 255f, 1f);
        CC = GetComponent<CardController>();
        MainCamera = Camera.allCameras[0];
        buttonManager = MainCamera.GetComponent<ButtonManagerScr>();

        OriginalColor = CC.Info.card_BG.color;



    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //CC.Info.PaintAnother(OriginalColor);
    }

    /*public void OnPointerDown(PointerEventData eventData)
    {

        if (buttonManager.MyDeck.gameObject.activeSelf)
        {
            if ((buttonManager.DecksManager.GetMyDeck().cards.Count <= buttonManager.DecksManager.MinDeckLen && CC.Info.card_BG.color.Equals(GreenColor)) || (buttonManager.DecksManager.GetMyDeck().cards.Count >= buttonManager.DecksManager.MaxDeckLen && CC.Info.card_BG.color.Equals(UnityEngine.Color.white)))
            {
                return;
            }
            ChangeCardColor();
            buttonManager.ChangeDeck(buttonManager.DecksManager.GetMyDeck(), CC.Card);
            buttonManager.UpdateDeckCounters();
        }
        else if (buttonManager.EnemyDeck.gameObject.activeSelf)
        {
            if ((buttonManager.DecksManager.GetEnemyDeck().cards.Count <= buttonManager.DecksManager.MinDeckLen && CC.Info.card_BG.color.Equals(GreenColor)) || (buttonManager.DecksManager.GetEnemyDeck().cards.Count >= buttonManager.DecksManager.MaxDeckLen && CC.Info.card_BG.color.Equals(UnityEngine.Color.white)))
            {
                return;
            }
            ChangeCardColor();
            buttonManager.ChangeDeck(buttonManager.DecksManager.GetEnemyDeck(), CC.Card);
            buttonManager.UpdateDeckCounters();
        }
    }*/

    public void OnPointerDown(PointerEventData eventData)
    {
        AllCards activeDeck = null;

        if (buttonManager.MyDeck.gameObject.activeSelf)
            activeDeck = buttonManager.DecksManager.GetMyDeck();
        else if (buttonManager.EnemyDeck.gameObject.activeSelf)
            activeDeck = buttonManager.DecksManager.GetEnemyDeck();

        if (activeDeck == null) return;

        int copiesCount = activeDeck.cards.Count(c => c.id == CC.Card.id);
        bool isInDeck = copiesCount > 0;

        // Ліва кнопка — додати карту
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log(activeDeck.cards.Count);
            if (activeDeck.cards.Count >= buttonManager.DecksManager.MaxDeckLen)
                return;

            if (copiesCount >= 2)
                return;

            buttonManager.DecksManager.AddCardToDeck(activeDeck, CC.Card);
            CC.Info.PaintGreen();
            CC.Info.SetQuantity(copiesCount + 1);
        }
        // Права кнопка — видалити карту
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (activeDeck.cards.Count <= buttonManager.DecksManager.MinDeckLen)
                return;
            if (!isInDeck)
                return;

            buttonManager.DecksManager.DeleteCardFromDeck(activeDeck, CC.Card);

            if (copiesCount == 1)
            {
                CC.Info.PaintWhite();
                CC.Info.SetQuantity(0);
            }
            else
            {
                CC.Info.PaintGreen();
                CC.Info.SetQuantity(copiesCount - 1);
            }
        }

        buttonManager.UpdateDeckCounters(activeDeck);
        audioSource.Play();

        // Обмеження по мінімуму та максимуму
        /*if (isInDeck && activeDeck.cards.Count <= buttonManager.DecksManager.MinDeckLen)
            return;
        if (!isInDeck && activeDeck.cards.Count >= buttonManager.DecksManager.MaxDeckLen)
            return;

        ChangeCardColorAndCounter(activeDeck);
        buttonManager.ChangeDeck(activeDeck, CC.Card);
        buttonManager.UpdateDeckCounters(activeDeck);*/
    }

    public void ChangeCardColorAndCounter(AllCards activeDeck)
    {
        audioSource.Play();

        switch (activeDeck.cards.Count(c => c.id == CC.Card.id)) {
            case 0:
                CC.Info.PaintGreen();
                CC.Info.SetQuantity(1);
                break;
            case 1:
                CC.Info.PaintGreen();
                CC.Info.SetQuantity(2);
                break;
            case 2:
                CC.Info.PaintWhite();
                CC.Info.SetQuantity(0);
                break;
        }
    }
}
