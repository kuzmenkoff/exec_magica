using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
//using UnityEngine.WSA;

public class CardInfoScript : MonoBehaviour
{
    public CardController CC;

    public Image card_BG;
    public Image title_BG;
    public Image descr_BG;
    //public Card SelfCard;
    public Image Logo;
    public Image ClassLogo;
    public Sprite EntityClassLogo;
    public Sprite SpellClassLogo;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI ManaCost;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI Attack;
    public GameObject HideObj;
    public GameObject ManaCostIndicator;
    public GameObject HPIndicator;
    public GameObject AttackIndicator;
    Sprite CardLogo;
    //public bool IsPlayer;

    public void HideCardInfo()
    {
        //HideObj.SetActive(true);
        //ManaCostIndicator.SetActive(false);
        //HPIndicator.SetActive(false);
        ShowCardInfo();
    }

    public void ShowCardInfo ()
    {
        //IsPlayer = isPlayer;
        HideObj.SetActive(false);
        card_BG.gameObject.SetActive(true);
        ManaCostIndicator.SetActive(true);
        HPIndicator.SetActive(true);
        //SelfCard = card;
        
        Logo.sprite = Resources.Load<Sprite>(CC.Card.LogoPath);
        Logo.preserveAspect = true;
        Title.text = CC.Card.Title;
        Description.text = CC.Card.Description;
        ManaCost.text = CC.Card.ManaCost.ToString();
        HP.text = CC.Card.HP.ToString();
        Attack.text = CC.Card.Attack.ToString();
        if (card_BG != null)
        {
            card_BG.color = UnityEngine.Color.white;
        }
        if (title_BG != null)
        {
            title_BG.color = UnityEngine.Color.white;

        }
        if (descr_BG != null)
        {
            descr_BG.color = UnityEngine.Color.white;
        }

        if (CC.Card.Class == Card.CardClass.ENTITY || CC.Card.Class == Card.CardClass.ENTITY_WITH_ABILITY)
        {
            ClassLogo.sprite = EntityClassLogo;
        }
        else if (CC.Card.Class == Card.CardClass.SPELL)
        {
            ClassLogo.sprite = SpellClassLogo;
        }

        if (CC.Card.IsSpell)
        {
            HPIndicator.SetActive(false);
            AttackIndicator.SetActive(false);
        }

    }

    public void RefreshData()
    {
        Attack.text = CC.Card.Attack.ToString();
        HP.text = CC.Card.HP.ToString();
        ManaCost.text = CC.Card.ManaCost.ToString();
    }

    public void PaintGreen()
    {
        float red = 13f / 255f;
        float green = 142f / 255f;
        float blue = 0f / 255f;
        float alpha = 1f;

        card_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        title_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        descr_BG.color = new UnityEngine.Color(red, green, blue, alpha);


    }

    public void PaintWhite()
    {
        card_BG.color = UnityEngine.Color.white;
        title_BG.color = UnityEngine.Color.white;
        descr_BG.color = UnityEngine.Color.white;
    }

    public void PaintAnother(UnityEngine.Color color)
    {
        card_BG.color = color;
        title_BG.color = color;
        descr_BG.color = color;
    }

    public void HighliteUsableCard()
    {
        if (card_BG == null) 
            return;
        float red = 134f / 255f;
        float green = 47f / 255f;
        float blue = 255f / 255f;
        float alpha = 1f;

        card_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        title_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        descr_BG.color = new UnityEngine.Color(red, green, blue, alpha);
    }

    public void HighlightManaAvaliability(int currentMana)
    {
        GetComponent<CanvasGroup>().alpha = currentMana >= CC.Card.ManaCost ? 1 : .75f;
        
    }

    public void HighlightAsTarget(bool highlight)
    {
        if (card_BG == null)
            return;
        if (!highlight)
            PaintWhite();
        else
        {
            float red = 255f / 255f;
            float green = 127f / 255f;
            float blue = 129f / 255f;
            float alpha = 1f;

            card_BG.color = new UnityEngine.Color(red, green, blue, alpha);
            title_BG.color = new UnityEngine.Color(red, green, blue, alpha);
            descr_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        }
    }

    public void HighlightAsSpellTarget(bool highlight)
    {
        if (card_BG == null)
            return;
        if (!highlight)
            PaintWhite();
        else
        {
            float red = 66f / 255f;
            float green = 45f / 255f;
            float blue = 255f / 255f;
            float alpha = 1f;

            card_BG.color = new UnityEngine.Color(red, green, blue, alpha);
            title_BG.color = new UnityEngine.Color(red, green, blue, alpha);
            descr_BG.color = new UnityEngine.Color(red, green, blue, alpha);
        }
    }

}
