using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GetComponent<CardMovementScr>().GameManager.PlayersTurn)
            return;
        CardInfoScript card = eventData.pointerDrag.GetComponent<CardInfoScript>();

        if (card && 
            card.SelfCard.CanBeUsed &&
            transform.parent == GetComponent<CardMovementScr>().GameManager.EnemyField)
        {
            card.SelfCard.ChangeUsageState(false);

            if (card.IsPlayer)
                card.PaintWhite();

            GetComponent<CardMovementScr>().GameManager.CardsFight(card, GetComponent<CardInfoScript>());
        }
    }

}
