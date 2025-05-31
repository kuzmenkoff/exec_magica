using System.Collections.Generic;
using UnityEngine;
using static Card;

public class CardController : MonoBehaviour
{
    public Card Card;

    public bool IsPlayerCard;

    public CardInfoScript Info;
    public CardMovementScr Movement;

    public GameManagerScr gameManager;
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

    public void OnCast(CardController target = null)
    {
        if (IsPlayerCard)
        {
            gameManager.Player.HandCards.Remove(this);
            gameManager.Player.FieldCards.Add(this);
            gameManager.ReduceMana(true, Card.ManaCost);
            gameManager.CheckCardForManaAvailability();
        }
        else
        {
            gameManager.Enemy.HandCards.Remove(this);
            gameManager.Enemy.FieldCards.Add(this);
            gameManager.ReduceMana(false, Card.ManaCost);
            Info.ShowCardInfo();
        }
        Card.IsPlaced = true;

        if (Card.HasAbility)
            Ability.OnCast();

        if (Card.IsSpell)
            UseSpell(target);
        UIController.Instance.UpdateHPAndMana();
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

    public void UseSpell(CardController target = null)
    {
        switch (Card.Spell)
        {
            case Card.SpellType.HEAL_ALLY_FIELD_CARDS:
                var allyCards = IsPlayerCard ?
                                gameManager.Player.FieldCards :
                                gameManager.Enemy.FieldCards;

                foreach (var card in allyCards)
                {
                    card.Card.HP += Card.SpellValue;
                    if (card.Card.Abilities.Exists(x => x == Card.AbilityType.HORDE))
                        card.Card.Attack = card.Card.HP;
                    card.Info.RefreshData();
                }
                break;

            case Card.SpellType.DAMAGE_ENEMY_FIELD_CARDS:
                var enemyCards = IsPlayerCard ?
                                 new List<CardController>(gameManager.Enemy.FieldCards) :
                                 new List<CardController>(gameManager.Player.FieldCards);
                foreach (var card in enemyCards)
                    GiveDamageTo(card, Card.SpellValue);
                break;

            case Card.SpellType.HEAL_ALLY_HERO:
                if (IsPlayerCard)
                    gameManager.Player.HP += Card.SpellValue;
                else
                    gameManager.Enemy.HP += Card.SpellValue;
                UIController.Instance.UpdateHPAndMana();
                break;

            case Card.SpellType.DAMAGE_ENEMY_HERO:
                if (IsPlayerCard)
                    gameManager.Enemy.HP -= Card.SpellValue;
                else
                    gameManager.Player.HP -= Card.SpellValue;
                UIController.Instance.UpdateHPAndMana();
                gameManager.CheckForVictory();
                break;

            case Card.SpellType.HEAL_ALLY_CARD:
                target.Card.HP += Card.SpellValue;
                if (target.Card.Abilities.Exists(x => x == Card.AbilityType.HORDE))
                    target.Card.Attack = target.Card.HP;
                break;

            case Card.SpellType.SHIELD_ON_ALLY_CARD:
                if (!target.Card.Abilities.Exists(x => x == Card.AbilityType.SHIELD))
                    target.Card.Abilities.Add(Card.AbilityType.SHIELD);
                break;

            case Card.SpellType.PROVOCATION_ON_ALLY_CARD:
                if (!target.Card.Abilities.Exists(x => x == Card.AbilityType.PROVOCATION))
                    target.Card.Abilities.Add(Card.AbilityType.PROVOCATION);
                break;

            case Card.SpellType.BUFF_CARD_DAMAGE:
                target.Card.Attack += Card.SpellValue;
                if (target.Card.Abilities.Exists(x => x == Card.AbilityType.HORDE))
                    target.Card.HP = target.Card.Attack;
                break;

            case Card.SpellType.DEBUFF_CARD_DAMAGE:
                target.Card.Attack = Mathf.Clamp(target.Card.Attack - Card.SpellValue, 0, int.MaxValue);
                if (target.Card.Abilities.Exists(x => x == Card.AbilityType.HORDE))
                    target.Card.HP = target.Card.Attack;
                break;

            case Card.SpellType.SILENCE:
                target.Card.Abilities.Clear();
                target.Card.Abilities.Add(AbilityType.NO_ABILITY);
                target.Card.Description = "";
                target.Info.ShowCardInfo();
                target.Ability.Provocation.SetActive(false);
                target.Ability.Shield.SetActive(false);
                break;

            case Card.SpellType.KILL_ALL:
                while (gameManager.Player.FieldCards.Count != 0)
                    gameManager.Player.FieldCards[0].DestroyCard();
                while (gameManager.Enemy.FieldCards.Count != 0)
                    gameManager.Enemy.FieldCards[0].DestroyCard();
                break;

        }

        if (target != null)
        {
            target.Ability.OnCast();
            target.CheckForAlive();
        }
        DestroyCard();
    }

    void GiveDamageTo(CardController card, int damage)
    {
        card.Card.GetDamage(damage);
        card.CheckForAlive();
        card.OnTakeDamage();
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

        RemoveCardFromList(gameManager.Enemy.FieldCards);
        RemoveCardFromList(gameManager.Enemy.HandCards);
        RemoveCardFromList(gameManager.Player.FieldCards);
        RemoveCardFromList(gameManager.Player.HandCards);

        Destroy(gameObject);
    }

    void RemoveCardFromList(List<CardController> list)
    {
        if (list.Exists(x => x == this))
            list.Remove(this);
    }
}
