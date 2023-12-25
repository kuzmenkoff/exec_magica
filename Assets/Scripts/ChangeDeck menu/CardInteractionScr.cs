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
        CC.Info.PaintAnother(OriginalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
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
    }

    public void ChangeCardColor()
    {
        audioSource.Play();

        if (OriginalColor.Equals(GreenColor))
        {
            CC.Info.PaintWhite();
            OriginalColor = CC.Info.card_BG.color;
        }
        else
        {
            CC.Info.PaintGreen();
            OriginalColor = CC.Info.card_BG.color;
        }
    }
}
