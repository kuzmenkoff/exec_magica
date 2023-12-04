using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public enum FieldType
{
    SELF_HAND, SELF_FIELD,
    ENEMY_HAND, ENEMY_FIELD
}

public class DropPlaceScr : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FieldType Type;
    public void OnDrop(PointerEventData eventData)
    {
        if(Type != FieldType.SELF_FIELD)
        {
            return;
        }
        CardMovementScr card = eventData.pointerDrag.GetComponent<CardMovementScr>();

        if (card && card.GameManager.PlayerFieldCards.Count < 6 &&
            card.GameManager.PlayersTurn && card.GameManager.PlayerMana >= 
            card.GetComponent<CardInfoScript>().SelfCard.ManaCost &&
            !card.GetComponent<CardInfoScript>().SelfCard.IsPlaced)
        {
            card.GameManager.PlayerHandCards.Remove(card.GetComponent<CardInfoScript>());
            card.GameManager.PlayerFieldCards.Add(card.GetComponent<CardInfoScript>());
            card.DefaultParent = transform;

            card.GetComponent<CardInfoScript>().SelfCard.IsPlaced = true;
            card.GameManager.ReduceMana(true, card.GetComponent<CardInfoScript>().SelfCard.ManaCost);
            card.GameManager.CheckCardForAvailability();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventData.pointerDrag == null || Type == FieldType.ENEMY_FIELD || Type == FieldType.ENEMY_HAND ||
            Type == FieldType.ENEMY_HAND || Type == FieldType.SELF_HAND)
            return;

        CardMovementScr card = eventData.pointerDrag.GetComponent<CardMovementScr>();

        if (card)
        {
            card.DefaultTempCardParent = transform;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        CardMovementScr card = eventData.pointerDrag.GetComponent<CardMovementScr>();

        if (card && card.DefaultTempCardParent == transform)
        {
            card.DefaultTempCardParent = card.DefaultParent;
        }
    }

}
