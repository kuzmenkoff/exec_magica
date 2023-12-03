using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using UnityEngine.WSA;

public class CardInfoScript : MonoBehaviour
{
    public Image card_BG;
    public Image title_BG;
    public Image descr_BG;
    public Card SelfCard;
    public Image Logo;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI ManaCost;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI Attack;

    public void HideCardInfo(Card card)
    {
        //SelfCard = card;
        //Logo.sprite = null;
        ShowCardInfo(card);
        //Title.text = "";
        //Description.text = "";
        //ManaCost.text = "";
        //HP.text = "";
        //Attack.text = "";
    }

    public void ShowCardInfo (Card card)
    {
        
        SelfCard = card;
        Logo.sprite = Resources.Load<Sprite>(card.LogoPath);
        Logo.preserveAspect = true;
        Title.text = card.Title;
        Description.text = card.Description;
        ManaCost.text = card.ManaCost.ToString();
        HP.text = card.HP.ToString();
        Attack.text = card.Attack.ToString();
        card_BG.color = UnityEngine.Color.white;
        title_BG.color = UnityEngine.Color.white;
        descr_BG.color = UnityEngine.Color.white;


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

    private void Start()
    {
        //ShowCardInfo(CardManager.AllCards[transform.GetSiblingIndex()]);
    }
}
