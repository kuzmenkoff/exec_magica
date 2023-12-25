using UnityEngine;

public class CardAbility : MonoBehaviour
{
    public CardController CC;
    public GameObject Shield, Provocation;

    public void OnCast()
    {
        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.LEAP:
                    CC.Card.CanAttack = true;
                    if (CC.IsPlayerCard)
                        CC.Info.HighliteUsableCard();
                    break;

                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;

                case Card.AbilityType.PROVOCATION:
                    Provocation.SetActive(true);
                    break;

                case Card.AbilityType.ALLIES_INSPIRATION:
                    if (CC.IsPlayerCard)
                    {
                        foreach (var card in CC.gameManager.PlayerFieldCards)
                        {
                            if (card.Card.id != CC.Card.id)
                            {
                                card.Card.Attack += CC.Card.SpellValue;
                                card.Info.RefreshData();
                            }
                        }
                    }
                    else
                    {
                        foreach (var card in CC.gameManager.EnemyFieldCards)
                        {
                            if (card.Card.id != CC.Card.id)
                            {
                                card.Card.Attack += CC.Card.SpellValue;
                                card.Info.RefreshData();
                            }
                        }
                    }

                    break;
            }
        }
    }

    public void OnDamageDeal(CardController defender = null)
    {
        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.DOUBLE_ATTACK:
                    if (CC.Card.TimesDealedDamage == 1)
                    {
                        CC.Card.CanAttack = true;
                        if (CC.IsPlayerCard)
                            CC.Info.HighliteUsableCard();
                    }
                    break;

                case Card.AbilityType.EXHAUSTION:
                    if (defender != null && defender.Card.Attack > 0)
                    {
                        CC.Card.Attack += CC.Card.SpellValue;
                        CC.Info.RefreshData();
                        defender.Card.Attack = Mathf.Clamp(defender.Card.Attack - CC.Card.SpellValue, 0, int.MaxValue);
                        defender.Info.RefreshData();
                    }
                    break;


            }
        }
    }

    public void OnDamageTake(CardController attacker = null)
    {
        Shield.SetActive(false);

        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.SHIELD:
                    Shield.SetActive(true);
                    break;


                case Card.AbilityType.HORDE:
                    CC.Card.Attack = CC.Card.HP;
                    CC.Info.RefreshData();
                    break;
            }
        }
    }

    public void OnNewTurn()
    {

        CC.Card.TimesDealedDamage = 0;

        foreach (var ability in CC.Card.Abilities)
        {
            switch (ability)
            {
                case Card.AbilityType.REGENERATION_EACH_TURN:
                    CC.Card.HP += CC.Card.SpellValue;
                    CC.Info.RefreshData();
                    break;


                case Card.AbilityType.INCREASE_ATTACK_EACH_TURN:
                    CC.Card.Attack += CC.Card.SpellValue;
                    CC.Info.RefreshData();
                    break;

                case Card.AbilityType.ADDITIONAL_MANA_EACH_TURN:
                    if (CC.IsPlayerCard && CC.gameManager.CurrentGame.Player.Mana < CC.gameManager.CurrentGame.Player.GetMaxManapool())
                        CC.gameManager.CurrentGame.Player.Mana += CC.Card.SpellValue;
                    else if (!CC.IsPlayerCard && CC.gameManager.CurrentGame.Enemy.Mana < CC.gameManager.CurrentGame.Enemy.GetMaxManapool())
                        CC.gameManager.CurrentGame.Enemy.Mana += CC.Card.SpellValue;
                    UIController.Instance.UpdateHPAndMana();
                    break;

                case Card.AbilityType.ALLIES_INSPIRATION:
                    if (CC.IsPlayerCard)
                    {
                        foreach (var card in CC.gameManager.PlayerFieldCards)
                        {
                            if (card.Card.id != CC.Card.id)
                            {
                                Card OriginalCard = CC.gameManager.decksManager.GetMyDeck().cards.Find(Card => Card.id == card.Card.id);
                                if (card.Card.Attack == OriginalCard.Attack)
                                {
                                    card.Card.Attack += CC.Card.SpellValue;
                                    card.Info.RefreshData();
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var card in CC.gameManager.EnemyFieldCards)
                        {
                            if (card.Card.id != CC.Card.id)
                            {
                                Card OriginalCard = CC.gameManager.decksManager.GetMyDeck().cards.Find(Card => Card.id == card.Card.id);
                                if (card.Card.Attack == OriginalCard.Attack)
                                {
                                    card.Card.Attack++;
                                    card.Info.RefreshData();
                                }
                            }
                        }
                    }

                    break;

                case Card.AbilityType.HORDE:
                    if (CC.Card.Attack > CC.Card.HP)
                        CC.Card.HP = CC.Card.Attack;
                    else
                        CC.Card.Attack = CC.Card.HP;

                    CC.Info.RefreshData();
                    break;
            }
        }
    }
}
