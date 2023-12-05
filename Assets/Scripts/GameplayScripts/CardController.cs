using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    public Card Card;

    public bool IsPlayerCard;

    public CardInfoScript Info;
    public CardMovementScr Movement;

    GameManagerScr gameManager;
    public CardAbility Ability;

    public void Init(Card card, bool isPlayerCard)
    {
        Card = card;
        gameManager = GameManagerScr.Instance;
        IsPlayerCard = isPlayerCard;

        if (isPlayerCard)
        {
            Info.ShowCardInfo();
            GetComponent<AttackedCard>().enabled = false;
        }
        else
            Info.HideCardInfo();
    }

    public void OnCast()
    {
        if (IsPlayerCard)
        {
            gameManager.PlayerHandCards.Remove(this);
            gameManager.PlayerFieldCards.Add(this);
            gameManager.ReduceMana(true, Card.ManaCost);
            gameManager.CheckCardForManaAvailability();
        }
        else
        {
            gameManager.EnemyHandCards.Remove(this);
            gameManager.EnemyFieldCards.Add(this);
            gameManager.ReduceMana(false, Card.ManaCost);
            Info.ShowCardInfo();
        }
        Card.IsPlaced = true;

        if (Card.HasAbility)
            Ability.OnCast();
    }

    public void OnTakeDamage(CardController attacker = null)
    {
        CheckForAlive();
        Ability.OnDamageTake(attacker);
    }

    public void OnDamageDeal(CardController defender = null)
    {
        Card.TimesDealedDamage++;
        Card.CanAttack = false;
        Info.PaintWhite();

        if (Card.HasAbility)
            Ability.OnDamageDeal(defender);
    }

    public void CheckForAlive()
    {
        if (Card.IsAlive())
            Info.RefreshData();
        else
            DestroyCard();
    }

    void DestroyCard()
    {
        Movement.OnEndDrag(null);

        RemoveCardFromList(gameManager.EnemyFieldCards);
        RemoveCardFromList(gameManager.EnemyHandCards);
        RemoveCardFromList(gameManager.PlayerFieldCards);
        RemoveCardFromList(gameManager.PlayerHandCards);

        Destroy(gameObject);
    }

    void RemoveCardFromList(List<CardController> list)
    {
        if (list.Exists(x => x == this))
            list.Remove(this);
    }
}
