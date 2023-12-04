using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttackedHero : MonoBehaviour, IDropHandler
{
    public enum HeroType
    {
        ENEMY,
        PLAYER
    }
    public HeroType Type;
    public GameManagerScr GameManager;
    public Color NormalColor, TargetColor;

    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.PlayersTurn)
            return;

        CardInfoScript card = eventData.pointerDrag.GetComponent<CardInfoScript>();

        if(card &&
           card.SelfCard.CanBeUsed &&
           Type == HeroType.ENEMY)
        {
            card.SelfCard.ChangeUsageState(false);
            GameManager.DamageHero(card, true);
        }
    }

    public void HighlightAsTarget(bool highlight)
    {
        GetComponent<Image>().color = highlight ? TargetColor : NormalColor;  
    }
}
