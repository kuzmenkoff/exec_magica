using UnityEngine;

public class Player
{
    public int HP, Mana, Manapool;
    public const int MAX_MANAPOOL = 10;

    public Player()
    {
        HP = 30;
        Mana = Manapool = 1;
    }

    public void RestoreRoundMana()
    {
        Mana = Manapool;
        UIController.Instance.UpdateHPAndMana();
    }

    public void IncreaseManapool()
    {
        Manapool = Mathf.Clamp(Manapool + 1, 0, MAX_MANAPOOL);
        UIController.Instance.UpdateHPAndMana();
    }

    public void GetDamage(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, int.MaxValue);
        UIController.Instance.UpdateHPAndMana();
    }

    public int GetMaxManapool()
    {
        int MaxMana = MAX_MANAPOOL;
        return MaxMana;
    }
}
