using UnityEngine;
using UnityEngine.EventSystems;

public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {

        if (!GameManagerScr.Instance.PlayerTurn)
            return;
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>(),
                       defender = GetComponent<CardController>();

        if (attacker &&
            attacker.Card.CanAttack &&
            defender.Card.IsPlaced)
        {
            if (GameManagerScr.Instance.Enemy.FieldCards.Exists(x => x.Card.IsProvocation) &&
                !defender.Card.IsProvocation)
                return;
            if (attacker.IsPlayerCard)
                attacker.Info.PaintWhite();

            GameManagerScr.Instance.CardsFight(attacker, defender);
        }
    }

}
