using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Card;

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

                case Card.AbilityType.ALLIES_INSPIRATION_1:
                    if (CC.IsPlayerCard)
                    {
                        foreach (var card in CC.gameManager.PlayerFieldCards)
                        {
                            if (card.Card.id != CC.Card.id && card.Card.Attack != 9)
                            {
                                card.Card.Attack++;
                                card.Info.RefreshData();
                            }
                        }
                    }
                    else
                    {
                        foreach (var card in CC.gameManager.EnemyFieldCards)
                        {
                            if (card.Card.id != CC.Card.id && card.Card.Attack != 9)
                            {
                                card.Card.Attack++;
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

                case Card.AbilityType.SILENCE:
                    if (defender == null)
                        return;
                    defender.Card.Abilities.Clear();
                    defender.Card.Abilities.Add(AbilityType.NO_ABILITY);
                    defender.Card.Description = "";
                    defender.Info.ShowCardInfo();
                    break;

                case Card.AbilityType.VAMPIRISM:
                    CC.Card.HP += CC.Card.Attack;
                    CC.Info.RefreshData();


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

                case Card.AbilityType.COUNTER_ATTACK:
                    if (attacker != null)
                        attacker.Card.GetDamage(CC.Card.Attack);
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
                case Card.AbilityType.REGENERATION_EACH_TURN_1:
                    CC.Card.HP += 1;
                    CC.Info.RefreshData();
                    break;

                case Card.AbilityType.REGENERATION_EACH_TURN_2:
                    CC.Card.HP += 2;
                    CC.Info.RefreshData();
                    break;
                case Card.AbilityType.INCREASE_ATTACK_EACH_TURN_1:
                    CC.Card.Attack += 1;
                    CC.Info.RefreshData();
                    break;
                case Card.AbilityType.INCREASE_ATTACK_EACH_TURN_2:
                    CC.Card.Attack += 2;
                    CC.Info.RefreshData();
                    break;
                case Card.AbilityType.ADDITIONAL_MANA_EACH_TURN_1:
                    if (CC.IsPlayerCard && CC.gameManager.CurrentGame.Player.Mana < CC.gameManager.CurrentGame.Player.GetMaxManapool())
                        CC.gameManager.CurrentGame.Player.Mana += 1;
                    else if (!CC.IsPlayerCard && CC.gameManager.CurrentGame.Enemy.Mana < CC.gameManager.CurrentGame.Enemy.GetMaxManapool())
                        CC.gameManager.CurrentGame.Enemy.Mana += 1;
                    UIController.Instance.UpdateHPAndMana();
                    break;
                case Card.AbilityType.ADDITIONAL_MANA_EACH_TURN_2:
                    if (CC.IsPlayerCard && CC.gameManager.CurrentGame.Player.Mana < CC.gameManager.CurrentGame.Player.GetMaxManapool() - 1)
                        CC.gameManager.CurrentGame.Player.Mana += 2;
                    else if (!CC.IsPlayerCard && CC.gameManager.CurrentGame.Enemy.Mana < CC.gameManager.CurrentGame.Enemy.GetMaxManapool() - 1)
                        CC.gameManager.CurrentGame.Enemy.Mana += 2;
                    UIController.Instance.UpdateHPAndMana();
                    break;
            }
        }
    }
}
