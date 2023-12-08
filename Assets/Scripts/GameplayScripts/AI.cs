using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class AI : MonoBehaviour
{
    public void MakeTurn()
    {
        StartCoroutine(EnemyTurn(GameManagerScr.Instance.EnemyHandCards));
    }

    IEnumerator EnemyTurn(List<CardController> cards)
    {
        yield return new WaitForSeconds(1);

        int randomCount = UnityEngine.Random.Range(0, cards.Count);
        for (int i = 0; i < randomCount; i++)
        {
            if (GameManagerScr.Instance.EnemyFieldCards.Count > 5 ||
                GameManagerScr.Instance.CurrentGame.Enemy.Mana == 0 ||
                GameManagerScr.Instance.EnemyHandCards.Count == 0)
                break;

            List<CardController> cardsList = cards.FindAll(x => GameManagerScr.Instance.CurrentGame.Enemy.Mana >= x.Card.ManaCost);
            //List<CardController> cardsList = cards.FindAll(x => !x.Card.IsSpell);

            if (cardsList.Count == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, cardsList.Count);

            if (cardsList[randomIndex].Card.IsSpell)
            {
                CastSpell(cardsList[randomIndex]);
                yield return new WaitForSeconds(.51f);
                UIController.Instance.UpdateHPAndMana();
            }
            else
            {
                cardsList[randomIndex].GetComponent<CardMovementScr>().MoveToField(GameManagerScr.Instance.EnemyField);
                yield return new WaitForSeconds(.51f);
                cardsList[randomIndex].transform.SetParent(GameManagerScr.Instance.EnemyField);
                cardsList[randomIndex].OnCast();
                UIController.Instance.UpdateHPAndMana();
            }

        }

        yield return new WaitForSeconds(1);

        while (GameManagerScr.Instance.EnemyFieldCards.Exists(x => x.Card.CanAttack))
        {
            var activeCard = GameManagerScr.Instance.EnemyFieldCards.FindAll(x => x.Card.CanAttack)[0];
            bool hasProvocation = GameManagerScr.Instance.PlayerFieldCards.Exists(x => x.Card.IsProvocation);
            if (hasProvocation ||
                UnityEngine.Random.Range(0, 2) == 0 && GameManagerScr.Instance.PlayerFieldCards.Count > 0)
            {
                CardController enemy;

                if (hasProvocation)
                    enemy = GameManagerScr.Instance.PlayerFieldCards.Find(x => x.Card.IsProvocation);
                else
                    enemy = GameManagerScr.Instance.PlayerFieldCards[UnityEngine.Random.Range(0, GameManagerScr.Instance.PlayerFieldCards.Count)];


                Debug.Log(activeCard.Card.Title + " (" + activeCard.Card.Attack + "; " + activeCard.Card.HP + ") ---> " +
                          enemy.Card.Title + " (" + enemy.Card.Attack + "; " + enemy.Card.HP + ")");

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(enemy.transform);
                yield return new WaitForSeconds(.75f);

                GameManagerScr.Instance.CardsFight(enemy, activeCard);
            }
            else
            {
                Debug.Log(activeCard.Card.Title + " (" + activeCard.Card.Attack + "; " + activeCard.Card.HP + ") ---> Hero");

                activeCard.GetComponent<CardMovementScr>().MoveToTarget(GameManagerScr.Instance.PlayerHero.transform);
                yield return new WaitForSeconds(.75f);

                GameManagerScr.Instance.DamageHero(activeCard, false);
            }

            yield return new WaitForSeconds(.2f);
        }

        yield return new WaitForSeconds(1);
        //GameManagerScr.Instance.ChangeTurn();
    }

    void CastSpell(CardController card)
    {
        switch (card.Card.SpellTarget)
        {
            case Card.TargetType.NO_TARGET:
                switch (card.Card.Spell)
                {
                    case Card.SpellType.HEAL_ALLY_FIELD_CARDS:
                        if (GameManagerScr.Instance.EnemyFieldCards.Count > 0)
                            StartCoroutine(CastCard(card));

                        
                        break;

                    case Card.SpellType.DAMAGE_ENEMY_FIELD_CARDS:
                        if (GameManagerScr.Instance.EnemyFieldCards.Count > 0)
                            StartCoroutine(CastCard(card));

                        break;

                    case Card.SpellType.HEAL_ALLY_HERO:
                        StartCoroutine(CastCard(card));
                        break;

                    case Card.SpellType.DAMAGE_ENEMY_HERO:
                        StartCoroutine(CastCard(card));
                        break;
                }
                break;

            case Card.TargetType.ALLY_CARD_TARGET:
                if (GameManagerScr.Instance.EnemyFieldCards.Count > 0)
                    StartCoroutine(CastCard(card, 
                        GameManagerScr.Instance.EnemyFieldCards[UnityEngine.Random.Range(0, GameManagerScr.Instance.EnemyFieldCards.Count)]));
                break;

            case Card.TargetType.ENEMY_CARD_TARGET:
                if (GameManagerScr.Instance.PlayerFieldCards.Count > 0)
                    StartCoroutine(CastCard(card,
                        GameManagerScr.Instance.EnemyFieldCards[UnityEngine.Random.Range(0, GameManagerScr.Instance.EnemyFieldCards.Count)]));

                break;
        }
    }

    IEnumerator CastCard(CardController spell, CardController target = null)
    {
        if (spell.Card.SpellTarget == Card.TargetType.NO_TARGET)
        {
            spell.Info.ShowCardInfo();
            spell.GetComponent<CardMovementScr>().MoveToField(GameManagerScr.Instance.EnemyField);
            yield return new WaitForSeconds(.51f);

            spell.OnCast();

            yield return new WaitForSeconds(.49f);
        }
        else
        {
            
            spell.GetComponent<CardMovementScr>().MoveToTarget(target.transform);

            yield return new WaitForSeconds(.51f);
            spell.Info.ShowCardInfo();

            GameManagerScr.Instance.EnemyHandCards.Remove(spell);
            GameManagerScr.Instance.EnemyFieldCards.Add(spell);
            GameManagerScr.Instance.ReduceMana(false, spell.Card.ManaCost);

            spell.Card.IsPlaced = true;

            spell.UseSpell(target);

            //yield return new WaitForSeconds(.49f);
        }

        string targetStr = target == null ? "no_target" : target.Card.Title;
        Debug.Log("AI spell cast: " + spell.Card.Title + "---> target: " + targetStr);
    }
}
