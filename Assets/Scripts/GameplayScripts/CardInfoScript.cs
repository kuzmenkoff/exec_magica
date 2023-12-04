using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
//using UnityEngine.WSA;

public class CardInfoScript : MonoBehaviour
{
    public Image card_BG;
    public Image title_BG;
    public Image descr_BG;
    public Card SelfCard;
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
    public bool IsPlayer;

    public void HideCardInfo(Card card)
    {
        SelfCard = card;
        HideObj.SetActive(true);
        //card_BG.gameObject.SetActive(false);
        ManaCostIndicator.SetActive(false);
        HPIndicator.SetActive(false);
        IsPlayer = false;
    }

    public void ShowCardInfo (Card card, bool isPlayer)
    {
        IsPlayer = isPlayer;
        HideObj.SetActive(false);
        card_BG.gameObject.SetActive(true);
        ManaCostIndicator.SetActive(true);
        HPIndicator.SetActive(true);
        SelfCard = card;
        Logo.sprite = Resources.Load<Sprite>(card.LogoPath);
        Logo.preserveAspect = true;
        Title.text = card.Title;
        Description.text = card.Description;
        ManaCost.text = card.ManaCost.ToString();
        HP.text = card.HP.ToString();
        Attack.text = card.Attack.ToString();
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

        if (card.Class == "Entity")
        {
            ClassLogo.sprite = EntityClassLogo;
        }
        else if (card.Class == "Spell")
        {
            ClassLogo.sprite = SpellClassLogo;
        }

    }

    public void RefreshData()
    {
        Attack.text = SelfCard.Attack.ToString();
        HP.text = SelfCard.HP.ToString();
        ManaCost.text = SelfCard.ManaCost.ToString();
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

    public void CheckForAvailability(int currentMana)
    {
        GetComponent<CanvasGroup>().alpha = currentMana >= SelfCard.ManaCost ? 1 : .75f;
        
    }

    public void HighliightAsTarget(bool highlight)
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

}
