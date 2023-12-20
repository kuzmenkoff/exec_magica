using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
//using UnityEditor.UIElements;
using UnityEngine;
using static Card;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using System.Reflection;

public class AI : MonoBehaviour
{
    GameState gameState;
    const int NumberOfSimulationsForCast = 1000;
    const int NumberOfSimulationsForSpellTarget = 1000;
    const int NumberOfSimulationsForAttackWithProvocation = 1000;
    const int NumberOfSimulationsForAttack = 1000;

    public bool CourutineIsRunning = false;
    public bool SubCourutineIsRunning = false;
    public bool SubSubCourutineIsRunning = false;
    public void MakeTurn()
    {
        StartCoroutine(EnemyTurn(GameManagerScr.Instance.EnemyHandCards));
    }

    IEnumerator EnemyTurn(List<CardController> cards)
    {
        CourutineIsRunning = true;
        yield return new WaitForSeconds(1);

        //Casting cards
        int targetindex;
        List<CardController> cardsList = cards.FindAll(x => GameManagerScr.Instance.CurrentGame.Enemy.Mana >= x.Card.ManaCost);

        //int randomCount = UnityEngine.Random.Range(0, cards.Count);
        while (cardsList.Count > 0)
        {
            if (GameManagerScr.Instance.EnemyFieldCards.Count > 5 ||
                GameManagerScr.Instance.CurrentGame.Enemy.Mana == 0 ||
                GameManagerScr.Instance.EnemyHandCards.Count == 0)
                break;

            if (cardsList.Count == 0)
                break;
            int index = FindBestCardToCast(cardsList);
            if (index == -1)
                break;

            if (cardsList[index].Card.IsSpell)
            {
                
                if (cardsList[index].Card.SpellTarget == Card.TargetType.ALLY_CARD_TARGET)
                {
                    if (GameManagerScr.Instance.EnemyFieldCards.Count == 0)
                    {
                        cardsList.RemoveAt(index);
                        cardsList = cards.FindAll(x => GameManagerScr.Instance.CurrentGame.Enemy.Mana >= x.Card.ManaCost);
                        continue;
                    }
                    else if (GameManagerScr.Instance.EnemyFieldCards.Count == 1)
                        targetindex = 0;
                    else
                        targetindex = FindBestTargetForSpell(index, GameManagerScr.Instance.EnemyFieldCards);
                    CastSpell(cardsList[index], targetindex);
                    while (SubCourutineIsRunning)
                        yield return new WaitForSeconds(0.1f);
                }
                else if (cardsList[index].Card.SpellTarget == Card.TargetType.ENEMY_CARD_TARGET)
                {
                    targetindex = FindBestTargetForSpell(index, GameManagerScr.Instance.PlayerFieldCards);
                    CastSpell(cardsList[index], targetindex);
                    while (SubCourutineIsRunning)
                        yield return new WaitForSeconds(0.1f);
                }
                else 
                    CastSpell(cardsList[index], -1);
                    while (SubCourutineIsRunning)
                        yield return new WaitForSeconds(0.1f);

                UIController.Instance.UpdateHPAndMana();
            }
            else
            {
                cardsList[index].GetComponent<CardMovementScr>().MoveToField(GameManagerScr.Instance.EnemyField);
                yield return new WaitForSeconds(.51f);
                cardsList[index].transform.SetParent(GameManagerScr.Instance.EnemyField);
                cardsList[index].OnCast();
                UIController.Instance.UpdateHPAndMana();
                cardsList = cards.FindAll(x => GameManagerScr.Instance.CurrentGame.Enemy.Mana >= x.Card.ManaCost);
            }
            cardsList = cards.FindAll(x => GameManagerScr.Instance.CurrentGame.Enemy.Mana >= x.Card.ManaCost);

        }

        yield return new WaitForSeconds(1);

        //Using cards

        while (GameManagerScr.Instance.EnemyFieldCards.Exists(x => x.Card.CanAttack))
        {
            CardController enemy, attacker;
            var activeCards = GameManagerScr.Instance.EnemyFieldCards.FindAll(x => x.Card.CanAttack);
            bool hasProvocation = GameManagerScr.Instance.PlayerFieldCards.Exists(x => x.Card.IsProvocation);
            if (hasProvocation)
            {
                int enemyIndex = GameManagerScr.Instance.PlayerFieldCards.FindIndex(x => x.Card.IsProvocation);
                if (activeCards.Count == 1)
                    attacker = activeCards[0];
                else
                    attacker = activeCards[FindBestAttacker(enemyIndex, activeCards)];
                enemy = GameManagerScr.Instance.PlayerFieldCards[enemyIndex];

                Debug.Log(attacker.Card.Title + " (" + attacker.Card.Attack + "; " + attacker.Card.HP + ") ---> " +
                          enemy.Card.Title + " (" + enemy.Card.Attack + "; " + enemy.Card.HP + ")");

                attacker.GetComponent<CardMovementScr>().MoveToTarget(enemy.transform);
                while (SubSubCourutineIsRunning)
                    yield return new WaitForSeconds(0.1f);
                GameManagerScr.Instance.CardsFight(enemy, attacker);
                attacker.Card.CanAttack = false;
            }
            else
            {
                //for (int i = 0; i < activeCards.Count; i++)
                attacker = activeCards[0];
                if (GameManagerScr.Instance.PlayerFieldCards.Count == 0)
                    targetindex = -1;
                else
                    targetindex = FindBestTargetForEntity(0, GameManagerScr.Instance.PlayerFieldCards);
                if (targetindex == -1)
                {
                    Debug.Log(attacker.Card.Title + " (" + attacker.Card.Attack + "; " + attacker.Card.HP + ") ---> Hero");
                    attacker.GetComponent<CardMovementScr>().MoveToTarget(GameManagerScr.Instance.PlayerHero.transform);
                    while (SubSubCourutineIsRunning)
                        yield return new WaitForSeconds(0.1f);
                    GameManagerScr.Instance.DamageHero(attacker, false);
                    attacker.Card.CanAttack = false;

                }
                else
                {
                    enemy = GameManagerScr.Instance.PlayerFieldCards[targetindex];
                    Debug.Log(attacker.Card.Title + " (" + attacker.Card.Attack + "; " + attacker.Card.HP + ") ---> " +
                    enemy.Card.Title + " (" + enemy.Card.Attack + "; " + enemy.Card.HP + ")");
                    attacker.GetComponent<CardMovementScr>().MoveToTarget(enemy.transform);
                    while (SubSubCourutineIsRunning)
                        yield return new WaitForSeconds(0.1f);
                    GameManagerScr.Instance.CardsFight(enemy, attacker);
                    attacker.Card.CanAttack = false;
                }
                    
                    
                
            }

        }

        yield return new WaitForSeconds(1);

        CourutineIsRunning = false;
        GameManagerScr.Instance.ChangeTurn();
    }

    int FindBestCardToCast(List<CardController> cards)
    {
        List<int> NumOfWins = new List<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            NumOfWins.Add(0);
            for (int sim = 0; sim < NumberOfSimulationsForCast; sim++)
            {
                gameState = new GameState();
                Card card = new Card();
                card = cards[i].Card.GetDeepCopy();
                gameState.AIFieldCards.Add(card);
                gameState.SimulateGame(0);
                if (gameState.Win)
                    NumOfWins[i]++;
            }
            Debug.Log("Card " + cards[i].Card.Title + " HP: " + cards[i].Card.HP + " has got winrate: " + NumOfWins[i] + "/ " + NumberOfSimulationsForCast);
        }
        NumOfWins.Add(0);
        for (int sim = 0; sim < NumberOfSimulationsForCast; sim++)
        {
            gameState = new GameState();
            Card card = new Card();
            gameState.SimulateGame(0);
            if (gameState.Win)
                NumOfWins[cards.Count]++;
        }
        Debug.Log("No card has got winrate: " + NumOfWins[cards.Count] + "/ " + NumberOfSimulationsForCast);
        int index = 0;
        if(GameManagerScr.Instance.Difficulty == "Hard")
            index = FindBiggestElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Normal")
            index = FindAverageElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Easy")
            index = FindSmallestElementIndex(NumOfWins);

        if (index == cards.Count)
        {
            
            return -1;

        }
        return index;
    }

    int FindBestTargetForSpell(int cardindex, List<CardController> targets)
    {
        List<int> NumOfWins = new List<int>();
        for(int i = 0; i < targets.Count; i++)
        {
            NumOfWins.Add(0);
            for (int sim = 0; sim < NumberOfSimulationsForSpellTarget; sim++)
            {
                gameState = new GameState();
                if(gameState.AIHandCards[cardindex].SpellTarget == Card.TargetType.ALLY_CARD_TARGET)
                    gameState.CastSpellOnTarget(gameState.AIHandCards[cardindex], gameState.AIFieldCards[i]);
                else if (gameState.AIHandCards[cardindex].SpellTarget == Card.TargetType.ENEMY_CARD_TARGET)
                    gameState.CastSpellOnTarget(gameState.AIHandCards[cardindex], gameState.PlayerFieldCards[i]);
                gameState.CastCards(true);
                if (gameState.CheckForVictory())
                    gameState.Win = gameState.ReturnResult();
                else
                {
                    gameState.UseCards(true);
                    if (gameState.CheckForVictory())
                        gameState.Win = gameState.ReturnResult();
                    else
                        gameState.AITurn = false;
                        gameState.SimulateGame(1);
                }
                if (gameState.Win)
                    NumOfWins[i]++;
            }
        }

        if (GameManagerScr.Instance.Difficulty == "Hard")
            return FindBiggestElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Normal")
            return FindAverageElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Easy")
            return FindSmallestElementIndex(NumOfWins);
        return FindBiggestElementIndex(NumOfWins);

    }

    int FindBestTargetForEntity(int attackerIndex, List<CardController> targets)
    {
        int index = 0;
        List<int> NumOfWins = new List<int>();
        for (int i = 0; i < targets.Count; i++)
        {
            NumOfWins.Add(0);
            for (int sim = 0; sim < NumberOfSimulationsForAttack; sim++)
            {
                gameState = new GameState();
                gameState.CardsFight(gameState.AIFieldCards.FindAll(x => x.CanAttack)[attackerIndex], gameState.PlayerFieldCards[i]);
                gameState.UseCards(true);
                if (gameState.CheckForVictory())
                    gameState.Win = gameState.ReturnResult();
                else
                    gameState.AITurn = false;
                gameState.SimulateGame(1);
                if (gameState.Win)
                    NumOfWins[i]++;
            }
        }
        NumOfWins.Add(0);
        for (int sim = 0; sim < NumberOfSimulationsForAttack; sim++)
        {
            gameState = new GameState();
            gameState.DamageHero(true, gameState.AIFieldCards.FindAll(x => x.CanAttack)[attackerIndex]);
            if (gameState.CheckForVictory())
                gameState.Win = gameState.ReturnResult();
            else
            {
                gameState.UseCards(true);
                if (gameState.CheckForVictory())
                    gameState.Win = gameState.ReturnResult();
                else
                    gameState.AITurn = false;
                    gameState.SimulateGame(1);
            }
            if (gameState.Win)
                NumOfWins[targets.Count]++;
        }
        if (GameManagerScr.Instance.Difficulty == "Hard")
            index = FindBiggestElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Normal")
            index = FindAverageElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Easy")
            index = FindSmallestElementIndex(NumOfWins);

        if (index == targets.Count)
            return -1;
        return index;

    }

    int FindBestAttacker(int targetIndex, List<CardController> cards)
    {
        if (cards.Count == 0) 
            return 0;
        List<int> NumOfWins = new List<int>();
        for (int i = 0; i < cards.Count; i++)
        {
            NumOfWins.Add(0);
            for (int sim = 0; sim < NumberOfSimulationsForAttackWithProvocation; sim++) {
                gameState = new GameState();
                //Debug.Log(cards.Count + " --- " + gameState.AIFieldCards.FindAll(x => x.CanAttack).Count);
                gameState.CardsFight(gameState.AIFieldCards.FindAll(x => x.CanAttack)[i], gameState.PlayerFieldCards[targetIndex]);
                gameState.UseCards(true);
                if (gameState.CheckForVictory())
                    gameState.Win = gameState.ReturnResult();
                else
                    gameState.AITurn = false;
                    gameState.SimulateGame(1);
                if (gameState.Win)
                    NumOfWins[i]++;
            }
        }
        if (GameManagerScr.Instance.Difficulty == "Hard")
            return FindBiggestElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Normal")
            return FindAverageElementIndex(NumOfWins);
        else if (GameManagerScr.Instance.Difficulty == "Easy")
            return FindSmallestElementIndex(NumOfWins);
        return FindBiggestElementIndex(NumOfWins);
    }

    int FindBiggestElementIndex(List<int> ints)
    {
        int maxNumber = int.MinValue;
        int maxIndex = -1;
        for(int i = 0; i < ints.Count; i++)
        {
            if (ints[i] > maxNumber)
            {
                maxNumber = ints[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    int FindAverageElementIndex(List<int> ints)
    {
        double average = ints.Average();

        int closestIndex = -1;
        double minDifference = double.MaxValue;

        // Iterate through the list to find the element closest to the average
        for (int i = 0; i < ints.Count; i++)
        {
            double difference = Math.Abs(ints[i] - average);
            if (difference < minDifference)
            {
                minDifference = difference;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    int FindSmallestElementIndex(List<int> ints)
    {
        int minNumber = int.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < ints.Count; i++)
        {
            if (ints[i] < minNumber)
            {
                minNumber = ints[i];
                minIndex = i;
            }
        }
        return minIndex;
    }

    void CastSpell(CardController card, int targetindex)
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
                        GameManagerScr.Instance.EnemyFieldCards[targetindex]));
                break;

            case Card.TargetType.ENEMY_CARD_TARGET:
                if (GameManagerScr.Instance.PlayerFieldCards.Count > 0)
                    StartCoroutine(CastCard(card,
                        GameManagerScr.Instance.PlayerFieldCards[targetindex]));

                break;
        }
    }

    IEnumerator CastCard(CardController spell, CardController target = null)
    {
        SubCourutineIsRunning = true;
        if (spell.Card.SpellTarget == Card.TargetType.NO_TARGET)
        {
            spell.Info.ShowCardInfo();
            spell.GetComponent<CardMovementScr>().MoveToField(GameManagerScr.Instance.EnemyField);
            while (SubSubCourutineIsRunning)
                yield return new WaitForSeconds(0.1f);

            spell.OnCast();

            
        }
        else
        {
            
            spell.GetComponent<CardMovementScr>().MoveToTarget(target.transform);

            while (SubSubCourutineIsRunning)
                yield return new WaitForSeconds(0.1f);
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
        SubCourutineIsRunning = false;
    }
}

public class GameState
{
    public int AIHP, PlayerHP;
    public List<Card> AIFieldCards = new List<Card>();
    public List<Card> PlayerFieldCards = new List<Card>();
    public List<Card> AIHandCards = new List<Card>();
    public List<Card> PlayerHandCards = new List<Card>();
    public AllCards AIDeckCards;
    public AllCards PlayerDeckCards;

    public DecksManagerScr decksManager;

    Player Player, AI;

    public bool AITurn;
    public bool Win;

    public GameState()
    {
        AITurn = !GameManagerScr.Instance.PlayersTurn;

        decksManager = new DecksManagerScr();
        Player = new Player();
        Player.HP = GameManagerScr.Instance.CurrentGame.Player.HP;
        Player.Mana = Player.Manapool = GameManagerScr.Instance.CurrentGame.Player.Manapool;

        AI = new Player();
        AI.HP = GameManagerScr.Instance.CurrentGame.Enemy.HP;
        AI.Mana = AI.Manapool = GameManagerScr.Instance.CurrentGame.Enemy.Manapool;

        AIHandCards = new List<Card>();
        PlayerHandCards = new List<Card>();
        AIFieldCards = new List<Card>();
        PlayerFieldCards = new List<Card>();

        AIDeckCards = new AllCards();
        PlayerDeckCards = new AllCards();

        AIFieldCards = DeepCopy(CardControllerToCards(GameManagerScr.Instance.EnemyFieldCards));
        PlayerFieldCards = DeepCopy(CardControllerToCards(GameManagerScr.Instance.PlayerFieldCards));
        AIHandCards = DeepCopy(CardControllerToCards(GameManagerScr.Instance.EnemyHandCards));
        PlayerHandCards = DeepCopy(CardControllerToCards(GameManagerScr.Instance.PlayerHandCards));
        AIDeckCards.cards = DeepCopy(GameManagerScr.Instance.decksManager.GetEnemyDeckCopy().cards);
        PlayerDeckCards.cards = DeepCopy(GameManagerScr.Instance.decksManager.GetMyDeckCopy().cards);

        int PlayerHandCount = PlayerHandCards.Count;

        PlayerDeckCards.cards.AddRange(PlayerHandCards);
        PlayerHandCards.Clear();

        PlayerDeckCards.cards = ShuffleDeck(PlayerDeckCards.cards);
        AIDeckCards.cards = ShuffleDeck(AIDeckCards.cards);

        for (int i = 0; i < PlayerHandCount; i++)
        {
            PlayerHandCards.Add(PlayerDeckCards.cards[0]);
            PlayerDeckCards.cards.RemoveAt(0);
        }

    }



    List<Card> CardControllerToCards(List<CardController> List)
    {
        List<Card> NewList = new List<Card>();
        for (int i = 0; i < List.Count; i++)
        {
            NewList.Add(List[i].Card.GetDeepCopy());
        }
        return NewList;
    }

     List<Card> DeepCopy(List<Card> source)
    {
        List<Card> list = new List<Card>();
        for(int i = 0; i < source.Count; i++)
        {
            list.Add(source[i].GetDeepCopy());
        }
        return list;
    }

    List<Card> ShuffleDeck(List<Card> Deck)
    {
        Card temp;
        System.Random random = new System.Random();
        // Fisher–Yates shuffle
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);

            temp = Deck[i];
            Deck[i] = Deck[randomIndex];
            Deck[randomIndex] = temp;
        }
        return Deck;
    }

    public void SimulateGame(int turn)
    {
        while (true)
        {
            
            AITurn = !AITurn;
            if(AITurn)
            {
                if (turn != 0)
                    AI.IncreaseManapool();
                AI.RestoreRoundMana();
                foreach (Card card in AIFieldCards)
                {
                    card.CanAttack = true;
                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.REGENERATION_EACH_TURN))
                        card.HP += card.SpellValue;
                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.INCREASE_ATTACK_EACH_TURN))
                        card.Attack += card.SpellValue;
                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.ADDITIONAL_MANA_EACH_TURN))
                        AI.Mana += card.SpellValue;
                }
            }
            else
            {
                if (turn != 0)
                    Player.IncreaseManapool();
                Player.RestoreRoundMana();
                foreach (Card card in PlayerFieldCards)
                {
                    card.CanAttack = true;

                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.REGENERATION_EACH_TURN))
                        card.HP += card.SpellValue;
                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.INCREASE_ATTACK_EACH_TURN))
                        card.Attack += card.SpellValue;
                    if (turn != 0 && card.Abilities.Exists(x => x == Card.AbilityType.ADDITIONAL_MANA_EACH_TURN))
                        Player.Mana += card.SpellValue;
                }
            }
            if (turn != 0)
                CastCards(AITurn);
            if (CheckForVictory())
                break;
            UseCards(AITurn);
            if (CheckForVictory())
                break;
            turn++;
        }
        Win = ReturnResult();
    }

    public void CastCards(bool AITurn)
    {
        if (AITurn)
        {
            GiveCardToHand(AIDeckCards.cards, AIHandCards, true);
            int randomCount = UnityEngine.Random.Range(0, AIHandCards.Count);
            for (int i = 0; i < randomCount; i++)
            {
                if (AIFieldCards.Count > 5 ||
                    AI.Mana == 0 ||
                    AIHandCards.Count == 0)
                    break;

                List<Card> cardsList = AIHandCards.FindAll(x => AI.Mana >= x.ManaCost);

                if (cardsList.Count == 0)
                    break;

                int randomIndex = UnityEngine.Random.Range(0, cardsList.Count);
                AI.Mana -= cardsList[randomIndex].ManaCost;

                if (cardsList[randomIndex].IsSpell)
                {
                    if (cardsList[randomIndex].SpellTarget == Card.TargetType.NO_TARGET ||
                       (cardsList[randomIndex].SpellTarget == Card.TargetType.ALLY_CARD_TARGET && AIFieldCards.Count > 0) ||
                       (cardsList[randomIndex].SpellTarget == Card.TargetType.ENEMY_CARD_TARGET && PlayerFieldCards.Count > 0))
                        CastSpell(cardsList[randomIndex], true);
                }
                else
                {
                    CastCard(cardsList[randomIndex], true);
                }

            }
        }
        else
        {
            GiveCardToHand(PlayerDeckCards.cards, PlayerHandCards, false);
            int randomCount = UnityEngine.Random.Range(0, PlayerHandCards.Count);
            for (int i = 0; i < randomCount; i++)
            {
                if (PlayerFieldCards.Count > 5 ||
                    Player.Mana == 0 ||
                    PlayerHandCards.Count == 0)
                    break;

                List<Card> cardsList = PlayerHandCards.FindAll(x => AI.Mana >= x.ManaCost);

                if (cardsList.Count == 0)
                    break;

                int randomIndex = UnityEngine.Random.Range(0, cardsList.Count);
                Player.Mana -= cardsList[randomIndex].ManaCost;

                if (cardsList[randomIndex].IsSpell)
                {
                    if(cardsList[randomIndex].SpellTarget == Card.TargetType.NO_TARGET ||
                       (cardsList[randomIndex].SpellTarget == Card.TargetType.ALLY_CARD_TARGET && PlayerFieldCards.Count > 0) ||
                       (cardsList[randomIndex].SpellTarget == Card.TargetType.ENEMY_CARD_TARGET && AIFieldCards.Count > 0))
                        CastSpell(cardsList[randomIndex], false);
                }
                else
                {
                    CastCard(cardsList[randomIndex], false);
                }

            }
        }
    }

    public void CastSpellOnTarget(Card spell, Card target)
    {
        AI.Mana -= spell.ManaCost;
        if(spell.SpellTarget == Card.TargetType.ALLY_CARD_TARGET)
        {
            switch(spell.Spell)
            {
                case Card.SpellType.HEAL_ALLY_CARD:
                    target.HP += spell.SpellValue;
                    break;

                case Card.SpellType.SHIELD_ON_ALLY_CARD:
                    target.Abilities.Add(Card.AbilityType.SHIELD);
                    break;

                case Card.SpellType.PROVOCATION_ON_ALLY_CARD:
                    target.Abilities.Add(Card.AbilityType.PROVOCATION);
                    break;

                case Card.SpellType.BUFF_CARD_DAMAGE:
                    target.Attack += spell.SpellValue;          
                    break;
            }
        }
        else if (spell.SpellTarget == Card.TargetType.ENEMY_CARD_TARGET)
        {
            switch(spell.Spell)
            {
                case Card.SpellType.DEBUFF_CARD_DAMAGE:
                    target.Attack -= spell.SpellValue;
                    break;

                case Card.SpellType.SILENCE:
                    target.Abilities.Clear();
                    target.Abilities.Add(AbilityType.NO_ABILITY);
                    break;
            }
        }
        DestroyCard(spell);
    }

    public void UseCards(bool AITurn)
    {
        int AttackerIndex, DefenderIndex;
        List<Card> Attackers, Defenders;
        if (AITurn)
        {
            Attackers = AIFieldCards.FindAll(x => x.CanAttack);
            Defenders = PlayerFieldCards;

        }
        else
        {
            Attackers = PlayerFieldCards.FindAll(x => x.CanAttack);
            Defenders = AIFieldCards;
        }
        foreach (Card card in Attackers)
            card.TimesDealedDamage = 0;
        for (int i = 0; i < Attackers.Count; i++)
        {
            AttackerIndex = UnityEngine.Random.Range(0, Attackers.Count);
            DefenderIndex = UnityEngine.Random.Range(0, Defenders.Count);
            if (!(Defenders.Count == 0))
            {
                for (int j = 0; j < Defenders.Count; j++)
                {
                    if (Defenders[j].IsProvocation)
                        DefenderIndex = j;
                }
            }
            if ((UnityEngine.Random.Range(0, 2) == 0 && !FieldHasProvocation(Defenders)) || Defenders.Count == 0)
            {
                DamageHero(AITurn, Attackers[AttackerIndex]);
                Attackers[AttackerIndex].TimesDealedDamage++;
                if (CheckForVictory())
                    return;

            }
            else
            {
                CardsFight(Attackers[AttackerIndex], Defenders[DefenderIndex]);
                Attackers[AttackerIndex].TimesDealedDamage++;
            }
            if (!(Attackers[AttackerIndex].Abilities.Exists(x => x == AbilityType.DOUBLE_ATTACK) && Attackers[AttackerIndex].TimesDealedDamage < 2))
                Attackers.RemoveAt(AttackerIndex);
        }
    }

    public void DamageHero(bool AITurn, Card card)
    {
        
        if (AITurn)
            Player.HP -= card.Attack;
        else
            AI.HP -= card.Attack;
        card.CanAttack = false;
    }


    public void CardsFight(Card attacker, Card defender)
    {
        defender.GetDamage(attacker.Attack);
        attacker.GetDamage(defender.Attack);


        if (attacker.Abilities.Exists(x => x == AbilityType.EXHAUSTION))
        {
            attacker.Attack += attacker.SpellValue;
            defender.Attack -= attacker.SpellValue;
        }
        if (attacker.Abilities.Exists(x => x == AbilityType.HORDE))
        {
            attacker.Attack = attacker.HP;
        }
        if (defender.Abilities.Exists(x => x == AbilityType.HORDE))
        {
            defender.Attack = defender.HP;
        }

        attacker.CanAttack = false;

        CheckForAlive(defender);
        CheckForAlive(attacker);
    }

    bool FieldHasProvocation(List<Card> FieldCards)
    {
        for (int i = 0; i < FieldCards.Count; i++)
        {
            if (FieldCards[i].IsProvocation)
                return true;
        }
        return false;
    }

    void GiveCardToHand(List<Card> deck, List<Card> hand, bool AI)
    {
        if ((AI && AIHandCards.Count >= 8) || (!AI && PlayerHandCards.Count >= 8))
            return;
        if (deck.Count == 0)
            deck = RenewDeck(AI);

        hand.Add(deck[0]);
        deck.RemoveAt(0);

    }

    public List<Card> RenewDeck(bool AI)
    {
        if (AI)
        {
            AIDeckCards.cards = new List<Card>(GameManagerScr.Instance.decksManager.GetEnemyDeckCopy().cards);
            AIDeckCards.cards = ShuffleDeck(AIDeckCards.cards);
            return AIDeckCards.cards;
        }
        else
        {
            PlayerDeckCards.cards = new List<Card>(GameManagerScr.Instance.decksManager.GetMyDeckCopy().cards);
            PlayerDeckCards.cards = ShuffleDeck(PlayerDeckCards.cards);
            return PlayerDeckCards.cards;
        }
    }
    

    void CastCard(Card card, bool AITurn)
    {
        if (AITurn)
        {
            foreach (Card fieldcard in AIFieldCards)
            {
                if (fieldcard.Abilities.Exists(x => x == Card.AbilityType.ALLIES_INSPIRATION))
                {
                    card.Attack += fieldcard.SpellValue;
                }
            }
            AIFieldCards.Add(card);
            AIHandCards.Remove(card);

        }
        else
        {
            foreach (Card fieldcard in PlayerFieldCards)
            {
                if (fieldcard.Abilities.Exists(x => x == Card.AbilityType.ALLIES_INSPIRATION))
                {
                    card.Attack += fieldcard.SpellValue;
                }
            }
            PlayerFieldCards.Add(card);
            PlayerHandCards.Remove(card);
        }

        if (card.HasAbility)
        {
            foreach (var ability in card.Abilities)
            {
                switch (ability)
                {
                    case Card.AbilityType.LEAP:
                        card.CanAttack = true;
                        break;

                    case Card.AbilityType.ALLIES_INSPIRATION:
                        if (AITurn)
                        {
                            foreach (var fieldcard in AIFieldCards)
                            {
                                if (fieldcard.id != card.id)
                                {
                                    fieldcard.Attack += card.SpellValue;
                                }
                            }
                        }
                        else
                        {
                            foreach (var fieldcard in PlayerFieldCards)
                            {
                                if (fieldcard.id != card.id)
                                {
                                    fieldcard.Attack += card.SpellValue;
                                }
                            }
                        }

                        break;
                }
            }
        }
    }

    void CastSpell(Card card, bool AITurn)
    {
        int targetIndex = 0;
        if (card.SpellTarget == Card.TargetType.ALLY_CARD_TARGET && AITurn)
            targetIndex = UnityEngine.Random.Range(0, AIFieldCards.Count);
        else if (card.SpellTarget == Card.TargetType.ALLY_CARD_TARGET && !AITurn)
            targetIndex = UnityEngine.Random.Range(0, PlayerFieldCards.Count);
        else if (card.SpellTarget == Card.TargetType.ENEMY_CARD_TARGET && AITurn)
            targetIndex = UnityEngine.Random.Range(0, PlayerFieldCards.Count);
        else if (card.SpellTarget == Card.TargetType.ENEMY_CARD_TARGET && !AITurn)
            targetIndex = UnityEngine.Random.Range(0, AIFieldCards.Count);
        switch (card.Spell)
        {
            case Card.SpellType.HEAL_ALLY_FIELD_CARDS:
                var allyCards = AITurn ?
                                 new List<Card>(AIFieldCards) :
                                 new List<Card>(PlayerFieldCards);
                foreach (Card fieldcard in allyCards)
                    fieldcard.HP += card.SpellValue;
                break;

            case Card.SpellType.DAMAGE_ENEMY_FIELD_CARDS:
                var enemyCards = AITurn ?
                                 new List<Card>(PlayerFieldCards) :
                                 new List<Card>(AIFieldCards);
                foreach (Card fieldcard in enemyCards)
                    GiveDamageTo(fieldcard, card.SpellValue);
                break;
            case Card.SpellType.HEAL_ALLY_HERO:
                if (AITurn)
                    AI.HP += card.SpellValue;
                else
                    Player.HP += card.SpellValue;
                break;
            case Card.SpellType.DAMAGE_ENEMY_HERO:
                if (AITurn)
                    Player.HP -= card.SpellValue;
                else
                    AI.HP -= card.SpellValue;
                break;

            case Card.SpellType.HEAL_ALLY_CARD:
                if (AITurn)
                    AIFieldCards[targetIndex].HP += card.SpellValue;
                else
                    PlayerFieldCards[targetIndex].HP += card.SpellValue;
                break;

            case Card.SpellType.SHIELD_ON_ALLY_CARD:
                if (AITurn)
                {
                    if (!AIFieldCards[targetIndex].Abilities.Exists(x => x == Card.AbilityType.SHIELD))
                        AIFieldCards[targetIndex].Abilities.Add(Card.AbilityType.SHIELD);
                }
                else
                {
                    if (!PlayerFieldCards[targetIndex].Abilities.Exists(x => x == Card.AbilityType.SHIELD))
                        PlayerFieldCards[targetIndex].Abilities.Add(Card.AbilityType.SHIELD);
                }
                break;

            case Card.SpellType.PROVOCATION_ON_ALLY_CARD:
                if (AITurn)
                {
                    if (!AIFieldCards[targetIndex].Abilities.Exists(x => x == Card.AbilityType.PROVOCATION))
                        AIFieldCards[targetIndex].Abilities.Add(Card.AbilityType.PROVOCATION);
                }
                else
                {
                    if (!PlayerFieldCards[targetIndex].Abilities.Exists(x => x == Card.AbilityType.PROVOCATION))
                        PlayerFieldCards[targetIndex].Abilities.Add(Card.AbilityType.PROVOCATION);
                }
                break;

            case Card.SpellType.BUFF_CARD_DAMAGE:
                if (AITurn)
                {
                    AIFieldCards[targetIndex].Attack += card.SpellValue;
                }
                else
                {
                    PlayerFieldCards[targetIndex].Attack += card.SpellValue;
                }
                break;

            case Card.SpellType.DEBUFF_CARD_DAMAGE:

                if (AITurn)
                {
                    PlayerFieldCards[targetIndex].Attack = Mathf.Clamp(PlayerFieldCards[targetIndex].Attack - card.SpellValue, 0, int.MaxValue);
                }
                else
                {
                    AIFieldCards[targetIndex].Attack = Mathf.Clamp(AIFieldCards[targetIndex].Attack - card.SpellValue, 0, int.MaxValue);
                }
                break;

            case Card.SpellType.SILENCE:
                if (AITurn)
                {
                    PlayerFieldCards[targetIndex].Abilities.Clear();
                    PlayerFieldCards[targetIndex].Abilities.Add(AbilityType.NO_ABILITY);
                }
                else
                {
                    AIFieldCards[targetIndex].Abilities.Clear();
                    AIFieldCards[targetIndex].Abilities.Add(AbilityType.NO_ABILITY);
                }
                break;

            case Card.SpellType.KILL_ALL:
                while (AIFieldCards.Count != 0)
                    DestroyCard(AIFieldCards[0]);
                while (PlayerFieldCards.Count != 0)
                    DestroyCard(PlayerFieldCards[0]);
                break;
        }

        DestroyCard(card);
    }

    void GiveDamageTo(Card card, int damage)
    {
        card.GetDamage(damage);
        CheckForAlive(card);
    }

    void CheckForAlive(Card card)
    {
        if (!card.IsAlive())
        {
            DestroyCard(card);
        }
    }

    void DestroyCard(Card card)
    {
        RemoveCardFromList(card, AIHandCards);
        RemoveCardFromList(card, AIFieldCards);
        RemoveCardFromList(card, PlayerHandCards);
        RemoveCardFromList(card, PlayerFieldCards);
    }

    void RemoveCardFromList(Card card, List<Card> list)
    {
        if (list.Exists(x => x == card))
            list.Remove(card);
    }

    public bool CheckForVictory()
    {
        if (Player.HP <= 0 || AI.HP <= 0)
            return true;
        return false;
    }

    public bool ReturnResult()
    {
        if (Player.HP <= 0)
            return true;
        else
            return false;
    }

}
